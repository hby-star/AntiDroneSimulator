using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

public class Drone : Entity
{
    # region States

    public EntityStateMachine StateMachine;
    public DroneMoveState MoveState;
    public DroneIdleState IdleState;

    # endregion

    #region Attack

    [Header("Attack Info")] [SerializeField]
    GameObject bomb;

    [SerializeField] private float bombBelowLength = 0.3f;

    public bool hasBomb;

    public void Attack()
    {
        if (hasBomb)
        {
            bomb.transform.parent = null;
            bomb.AddComponent<Rigidbody>();
            Rigidbody bombRigidbody = bomb.GetComponent<Rigidbody>();
            bombRigidbody.velocity = Rigidbody.velocity;
            hasBomb = false;
        }
    }

    #endregion

    #region Operate

    [Header("Operate Info")] public bool isLeader = false;

    public override void SetOperate(bool operate)
    {
        base.SetOperate(operate);

        if (!operate)
        {
            StateMachine.ChangeState(IdleState);
        }
    }

    // 设置当前移动方向
    public void SetCurrentMoveDirection(Vector3 moveDirection)
    {
        CurrentMoveDirection = moveDirection;
    }

    // 向当前移动方向移动
    public void MoveToCurrentMoveDirection(float speed)
    {
        // 先水平转向到目标方向
        Vector3 horMoveDirection = CurrentMoveDirection;
        horMoveDirection.y = 0;

        // 平滑转向
        Quaternion targetRotation = Quaternion.LookRotation(horMoveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed * 3);

        // 再向前移动
        Rigidbody.velocity = transform.forward * (horMoveDirection.magnitude * speed / 2);

        // 再垂直移动
        Rigidbody.velocity += transform.up * (CurrentMoveDirection.normalized.y * speed);
    }

    #endregion

    #region Algorithm

    public IDroneSearchAlgorithm DroneSearchAlgorithm;
    public IDroneAttackAlgorithm DroneAttackAlgorithm;
    public bool GetTrainningData { get; private set; }

    public Player targetPlayer;
    private Renderer[] targetRenderers;
    private Rect targetRect;
    [NonSerialized] public bool FoundPlayer = false;
    [NonSerialized] public Vector3 CurrentMoveDirection;
    private float lastSendRequestTime = 0;
    public float sendRequestInterval = 1f; // 发送请求的间隔
    public float detectObstacleDistance = 5; // 避障距离

    private ObjectDetectionHelper objectDetectionHelper;
    private RoutePlanningHelper routePlanningHelper;

    // 无人机算法初始化
    void DroneAlgorithmStart()
    {
        // 初始化无人机进攻状态
        hasBomb = true;
        FoundPlayer = false;

        // 初始化无人机算法
        DroneSearchAlgorithm = DroneAlgorithmManager.Instance.GetDroneSearchAlgorithm();
        DroneSearchAlgorithm.DroneSearchAlgorithmSet(this);
        DroneAttackAlgorithm = DroneAlgorithmManager.Instance.GetDroneAttackAlgorithm();
        DroneAttackAlgorithm.DroneAttackAlgorithmSet(this);

        // 初始化服务器通信工具
        objectDetectionHelper = new ObjectDetectionHelper(this);
        routePlanningHelper = new RoutePlanningHelper(this);
    }

    // 更新攻击算法
    private void AttackPlayerUpdate()
    {
        // 攻击玩家
        //DroneAttackAlgorithm.DroneAttackAlgorithmUpdate();
        //CurrentMoveDirection = routePlanningHelper.responseDirection;
        //MoveToCurrentMoveDirection(moveSpeed);

        // 攻击距离内释放炸弹
        if (CanAttackPlayer())
        {
            Attack();
        }
    }

    public bool CanAttackPlayer()
    {
        Collider[] hitColliders =
            Physics.OverlapSphere(transform.position, bomb.GetComponent<Bomb>().explosionRadius / 2);

        foreach (var hitCollider in hitColliders)
        {
            Player player = hitCollider.GetComponent<Player>();
            if (player != null)
            {
                return true;
            }
        }

        return false;
    }

    // 更新搜索算法
    private void SearchPlayerUpdate()
    {
        DroneSearchAlgorithm.DroneSearchAlgorithmUpdate();
    }

    // 搜索玩家
    private void FindPlayer()
    {
        if (Time.time - lastSendRequestTime > sendRequestInterval)
        {
            StartCoroutine(routePlanningHelper.SendRoutePlanningRequest(Camera));
            lastSendRequestTime = Time.time;
        }
    }

    private void SimulateDetectPlayer()
    {
        if (!targetPlayer)
        {
            targetPlayer = FindObjectOfType<Player>();
            targetRenderers = targetPlayer.GetComponentsInChildren<Renderer>();
            FoundPlayer = true;
        }

        // 摄像机朝向玩家
        Camera.transform.LookAt(targetPlayer.transform);

        // 获取物体的边界
        Bounds[] bounds = new Bounds[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            bounds[i] = targetRenderers[i].bounds;
        }

        // 找到屏幕空间中的边界
        Vector3 min = Camera.WorldToScreenPoint(bounds[0].min);
        Vector3 max = Camera.WorldToScreenPoint(bounds[0].max);

        for (int i = 1; i < bounds.Length; i++)
        {
            min = Vector3.Min(min, Camera.WorldToScreenPoint(bounds[i].min));
            min = Vector3.Min(min, Camera.WorldToScreenPoint(bounds[i].max));
            max = Vector3.Max(max, Camera.WorldToScreenPoint(bounds[i].min));
            max = Vector3.Max(max, Camera.WorldToScreenPoint(bounds[i].max));
        }

        // 检查物体是否被遮挡
        bool isVisible = true;
        RaycastHit hit;
        if (Physics.Linecast(Camera.transform.position,
                targetPlayer.transform.position + targetPlayer.standColliderHeight / 2 * Vector3.up,
                out hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                isVisible = true;
            }
            else
            {
                isVisible = false;
            }
        }

        // 计算边界框的位置和大小
        if (isVisible)
        {
            targetRect = new Rect(min.x, Screen.height - max.y, max.x - min.x, max.y - min.y);
        }
        else
        {
            targetRect = new Rect(0, 0, 0, 0);
        }
    }

    void OnGUI()
    {
        // 只在无人机的摄像机上显示
        if (Camera.enabled)
        {
            if (FoundPlayer)
            {
                // 在屏幕上绘制玩家的边界框
                GUI.Box(targetRect, "");
            }
        }
    }

    // 获取距离最近的障碍物的相对位置
    public Vector3 GetObstaclePosition()
    {
        Vector3 obstaclePosition = new Vector3(detectObstacleDistance, detectObstacleDistance, detectObstacleDistance);

        // 检测前后左右上下以及8个拐角方向的障碍物
        Vector3[] directions = new Vector3[]
        {
            transform.forward, -transform.forward, transform.right, -transform.right, transform.up, -transform.up,
            transform.forward + transform.right + transform.up, transform.forward + transform.right - transform.up,
            transform.forward - transform.right + transform.up, transform.forward - transform.right - transform.up,
            -transform.forward + transform.right + transform.up, -transform.forward + transform.right - transform.up,
            -transform.forward - transform.right + transform.up, -transform.forward - transform.right - transform.up
        };

        foreach (var direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, detectObstacleDistance))
            {
                // 忽略玩家和无人机
                if (!hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Drone"))
                {
                    if (hit.distance < obstaclePosition.magnitude)
                    {
                        obstaclePosition = hit.point - transform.position;
                    }
                }
            }
        }

        return obstaclePosition;
    }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        StateMachine = new EntityStateMachine();

        MoveState = new DroneMoveState(StateMachine, this, "Move", this);
        IdleState = new DroneIdleState(StateMachine, this, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();

        StateMachine.Initialize(IdleState);

        // 无人机操作
        SetOperate(false);
        BombPathStart();

        // 无人机算法
        DroneAlgorithmStart();

        GetTrainningData = DroneAlgorithmManager.Instance.getTrainingData;
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();

        if (GetTrainningData && Time.time - lastSendRequestTime > sendRequestInterval)
        {
            lastSendRequestTime = Time.time;
            objectDetectionHelper.GetTrainingDataSendRequest(Camera);
        }

        // 无人机音效
        AudioUpdate();

        // 模拟检测玩家
        SimulateDetectPlayer();


        if (operateNow)
        {
            // 摄像机控制
            MouseLookUpdate();

            // 炸弹路径指示
            //BombPathUpdate();
        }
        else
        {
            // 搜索玩家
            //FindPlayer();

            if (FoundPlayer && hasBomb)
            {
                // 攻击玩家
                AttackPlayerUpdate();
            }
            else
            {
                // 搜索玩家
                SearchPlayerUpdate();
            }
        }
    }

    #region React To Hit

    public enum HitType
    {
        NormalBullet,
        EmpBullet,
        NetBullet,
        ElectricInterference,
    }

    public void ReactToHit(HitType hitType)
    {
        if (hitType == HitType.ElectricInterference)
        {
            DroneSearchAlgorithm = new SearchStay();
            DroneSearchAlgorithm.DroneSearchAlgorithmSet(this);
        }
        else
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        this.transform.Rotate(-75, 0, 0);

        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }

    #endregion

    #region MouseLook

    [Header("Mouse Look Info")] public float sensitivityHor = 9.0f;

    public Transform mouseLookTarget;

    void MouseLookUpdate()
    {
        if (operateNow)
            MouseXLookUpdate();
    }

    void MouseXLookUpdate()
    {
        float rotationY = CameraHorizontalInput * sensitivityHor;
        mouseLookTarget.Rotate(0, rotationY, 0);
    }

    #endregion

    #region Audio

    [Header("Audio Info")] public AudioSource soundSource;
    public AudioClip flySound;

    void AudioUpdate()
    {
        if (!soundSource.isPlaying)
        {
            soundSource.clip = flySound;
            soundSource.loop = true;
            soundSource.Play();
        }
    }

    #endregion

    #region BombPath

    [Header("Bomb Path Info")] public int numberOfPoints = 30;
    public float timeBetweenPoints = 0.1f;
    public GameObject sphereMarkerPrefab; // 用于标记的球体预制体
    private GameObject sphereMarkerInstance; // 实例化的球体标记
    private LineRenderer lineRenderer;

    void BombPathStart()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        sphereMarkerInstance = Instantiate(sphereMarkerPrefab, Vector3.zero, Quaternion.identity);
        sphereMarkerInstance.SetActive(false);
    }

    void BombPathUpdate()
    {
        if (hasBomb && operateNow)
        {
            lineRenderer.positionCount = numberOfPoints;
            Vector3[] points = new Vector3[numberOfPoints];
            Vector3 startingPosition = transform.position;
            startingPosition.y -= bombBelowLength;
            Vector3 startingVelocity = Rigidbody.velocity;
            bool hitDetected = false;

            for (int i = 0; i < numberOfPoints; i++)
            {
                float time = i * timeBetweenPoints;
                Vector3 position = CalculatePosition(startingPosition, startingVelocity, time);
                points[i] = position;

                // 检测碰撞
                if (!hitDetected)
                {
                    if (Physics.Raycast(startingPosition, position - startingPosition, out RaycastHit hit,
                            (position - startingPosition).magnitude))
                    {
                        if (hit.collider.CompareTag("Ground"))
                        {
                            // 更新球体标记的位置并显示
                            sphereMarkerInstance.transform.position = hit.point;
                            sphereMarkerInstance.SetActive(true);
                            hitDetected = true;
                        }
                    }
                }
            }

            // 如果没有检测到碰撞，则隐藏球体标记
            if (!hitDetected)
            {
                sphereMarkerInstance.SetActive(false);
            }

            lineRenderer.SetPositions(points);
        }
        else
        {
            lineRenderer.positionCount = 0;
            sphereMarkerInstance.SetActive(false);
        }
    }

    Vector3 CalculatePosition(Vector3 start, Vector3 velocity, float time)
    {
        Vector3 position = start + velocity * time;
        position.y += 0.5f * Physics.gravity.y * time * time;
        return position;
    }

    public bool IsPlayerDetectedOnPath()
    {
        if (hasBomb)
        {
            Vector3 startingPosition = transform.position;
            startingPosition.y -= bombBelowLength;
            Vector3 startingVelocity = Rigidbody.velocity;

            for (int i = 0; i < numberOfPoints; i++)
            {
                float time = i * timeBetweenPoints;
                Vector3 position = CalculatePosition(startingPosition, startingVelocity, time);

                if (Physics.Raycast(startingPosition, position - startingPosition, out RaycastHit hit,
                        (position - startingPosition).magnitude))
                {
                    if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Vehicle"))
                    {
                        Collider[] hitColliders =
                            Physics.OverlapSphere(hit.point, bomb.GetComponent<Bomb>().explosionRadius / 2);
                        foreach (var hitCollider in hitColliders)
                        {
                            Player player = hitCollider.GetComponent<Player>();
                            if (player != null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region Handle Input

    public float HorizontalInput { get; private set; }
    public float VerticalInput { get; private set; }
    public float UpInput { get; private set; }
    public bool AttackInput { get; private set; }
    public float CameraHorizontalInput { get; private set; }

    void OnEnable()
    {
        if (isLeader)
        {
            Messenger<float>.AddListener(InputEvent.DRONE_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
            Messenger<float>.AddListener(InputEvent.DRONE_VERTICAL_INPUT, (value) => { VerticalInput = value; });
            Messenger<float>.AddListener(InputEvent.DRONE_UP_INPUT, (value) => { UpInput = value; });
            Messenger<bool>.AddListener(InputEvent.DRONE_ATTACK_INPUT, (value) => { AttackInput = value; });
            Messenger<float>.AddListener(InputEvent.DRONE_CAMERA_HORIZONTAL_INPUT,
                (value) => { CameraHorizontalInput = value; });
        }
    }

    void OnDisable()
    {
        if (isLeader)
        {
            Messenger<float>.RemoveListener(InputEvent.DRONE_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
            Messenger<float>.RemoveListener(InputEvent.DRONE_VERTICAL_INPUT, (value) => { VerticalInput = value; });
            Messenger<float>.RemoveListener(InputEvent.DRONE_UP_INPUT, (value) => { UpInput = value; });
            Messenger<bool>.RemoveListener(InputEvent.DRONE_ATTACK_INPUT, (value) => { AttackInput = value; });
            Messenger<float>.RemoveListener(InputEvent.DRONE_CAMERA_HORIZONTAL_INPUT,
                (value) => { CameraHorizontalInput = value; });
        }
    }

    #endregion
}