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

public class OperableDrone : Drone
{
    # region States

    public EntityStateMachine StateMachine;
    public OperableDroneMoveState MoveState;
    public OperableDroneIdleState IdleState;

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

    #region Algorithm

    public IDroneSearchAlgorithm DroneSearchAlgorithm;
    public IDroneAttackAlgorithm DroneAttackAlgorithm;
    public bool GetTrainningData { get; private set; }


    private float lastSendRequestTime = 0;
    public float sendRequestInterval = 1f; // 发送请求的间隔

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
        DroneAttackAlgorithm.DroneAttackAlgorithmUpdate();
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
            Physics.OverlapSphere(transform.position, 3);

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
    #endregion

    protected override void Awake()
    {
        base.Awake();

        StateMachine = new EntityStateMachine();

        MoveState = new OperableDroneMoveState(StateMachine, this, "Move", this);
        IdleState = new OperableDroneIdleState(StateMachine, this, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();

        droneType = DroneType.Operate;

        StateMachine.Initialize(IdleState);

        // 无人机操作
        SetOperate(false);
        BombPathStart();

        // 无人机算法
        DroneAlgorithmStart();
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();

        if (!operateNow)
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
        else
        {
            // 摄像机控制
            MouseLookUpdate();

            // 炸弹路径指示
            BombPathUpdate();
        }
    }



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