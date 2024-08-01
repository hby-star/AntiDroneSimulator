using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    #endregion

    #region Algorithm

    private const string DroneServerObjectDetectionUrl = "http://localhost:8000//drone_object_detection/";
    public IDroneSearchAlgorithm DroneSearchAlgorithm;
    public IDroneAttackAlgorithm DroneAttackAlgorithm;

    [NonSerialized] public Player TargetPlayer;
    [NonSerialized] public Vector3 CurrentMoveDirection;
    private float lastSendRequestTime = 0;
    public float sendRequestInterval = 1;

    void DroneAlgorithmStart()
    {
        // 初始化无人机进攻状态
        hasBomb = true;
        TargetPlayer = null;

        // 初始化无人机算法
        DroneSearchAlgorithm = DroneAlgorithmManager.Instance.GetDroneSearchAlgorithm();
        DroneSearchAlgorithm.DroneSearchAlgorithmSet(this);
        DroneAttackAlgorithm = DroneAlgorithmManager.Instance.GetDroneAttackAlgorithm();
        DroneAttackAlgorithm.DroneAttackAlgorithmSet(this);
    }

    private void AttackPlayerUpdate()
    {
        // 攻击玩家
        DroneAttackAlgorithm.DroneAttackAlgorithmUpdate();

        // 攻击距离内释放炸弹
        if (Vector3.Distance(transform.position, TargetPlayer.transform.position) <
            (bomb.GetComponent<Bomb>().explosionRadius / 2))
        {
            Attack();
        }

        // 炸弹路径上有玩家则释放炸弹
        if (IsPlayerDetectedOnPath())
        {
            Attack();
        }
    }

    private void SearchPlayerUpdate()
    {
        DroneSearchAlgorithm.DroneSearchAlgorithmUpdate();
    }

    private Player FindPlayer()
    {
        if (Time.time - lastSendRequestTime > sendRequestInterval)
        {
            //StartCoroutine(SendObjectDetectionRequest());
            lastSendRequestTime = Time.time;
        }

        return null;
    }

    IEnumerator SendObjectDetectionRequest()
    {
        // 将渲染的结果保存到RenderTexture
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        Camera.targetTexture = renderTexture;

        // 创建一个Texture2D来保存图像数据
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        // 渲染摄像机
        Camera.Render();

        // 从RenderTexture读取像素
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();

        // 重置摄像机的目标纹理和活动渲染纹理
        Camera.targetTexture = null;
        RenderTexture.active = null;

        // 销毁RenderTexture
        Destroy(renderTexture);

        // 编码图像为JPEG
        byte[] bytes = screenShot.EncodeToJPG();

        // 创建一个WWWForm并添加图像数据
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes, "screenshot.jpg", "image/jpeg");

        // 发送POST请求
        UnityWebRequest www = UnityWebRequest.Post(DroneServerObjectDetectionUrl, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            ProcessObjectDetectionResponse(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Image upload failed: " + www.error);
        }
    }

    void ProcessObjectDetectionResponse(string jsonResponse)
    {
        JObject json = JObject.Parse(jsonResponse);
        JToken outputToken = json["output"];
        if (outputToken != null)
        {
            string output = outputToken.ToString();
            Debug.Log("Output: " + output);

            // 解析output数据
            JArray outputData = JArray.Parse(output);
            foreach (var item in outputData)
            {
                string name = item["name"].ToString();
                int classId = (int)item["class"];
                float confidence = (float)item["confidence"];
                float x1 = (float)item["box"]["x1"];
                float y1 = (float)item["box"]["y1"];
                float x2 = (float)item["box"]["x2"];
                float y2 = (float)item["box"]["y2"];

                Debug.Log($"Detected object: {name}, Class: {classId}, Confidence: {confidence}, Box: ({x1}, {y1}, {x2}, {y2})");
            }
        }
    }

    public void SetCurrentMoveDirection(Vector3 moveDirection)
    {
        CurrentMoveDirection = moveDirection;
    }

    public void MoveToCurrentMoveDirection(float speed)
    {
        // 先水平转向到目标方向
        Vector3 horMoveDirection = CurrentMoveDirection;
        horMoveDirection.y = 0;

        // 平滑转向
        Quaternion targetRotation = Quaternion.LookRotation(horMoveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed / 2);

        // 再向前移动
        Rigidbody.velocity = transform.forward * horMoveDirection.magnitude * speed;

        // 再垂直移动
        Rigidbody.velocity += transform.up * CurrentMoveDirection.normalized.y * speed;
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
        SetOperate((InputManager.Instance.currentEntity is Drone) && isLeader);
        BombPathStart();

        // 无人机算法
        DroneAlgorithmStart();
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();

        // 无人机音效
        AudioUpdate();

        if (operateNow)
        {
            // 摄像机控制
            MouseLookUpdate();

            // 炸弹路径指示
            BombPathUpdate();
        }
        else
        {
            // 搜索玩家
            TargetPlayer = FindPlayer();

            if (TargetPlayer && hasBomb)
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