using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class Drone : Entity
{
    # region States

    public EntityStateMachine StateMachine;
    public DroneMoveState MoveState;
    public DroneIdleState IdleState;

    # endregion

    #region Attack

    [Header("Attack Info")]
    [SerializeField] GameObject bomb;

    [SerializeField] private float bombBelowLength = 0.3f;

    private bool hasBomb = true;

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
        if (hasBomb)
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

    #endregion

    #region Control

    [Header("Control Info")] public bool isLeader = false;

    public IDroneControlAlgorithm DroneControlAlgorithm;

    public void SetOperate(bool operate)
    {
        operateNow = operate;
        droneCamera.gameObject.SetActive(operate);
        transform.rotation = Quaternion.identity;
        if (!operate)
        {
            StateMachine.ChangeState(IdleState);
        }
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

        DroneControlAlgorithm = DroneAlgorithmManager.Instance.GetAlgorithm();
        DroneControlAlgorithm.DroneControlSet(this);

        droneCamera = GetComponentInChildren<Camera>();

        SetOperate((InputManager.Instance.operateTarget == InputManager.OperateTarget.Drone) && isLeader);

        Rigidbody.freezeRotation = true;

        BombPathStart();
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();

        AudioUpdate();
        MouseLookUpdate();

        if (operateNow)
        {
            BombPathUpdate();
        }
        else
        {
            DroneControlAlgorithm.DroneControlUpdate();
        }
    }

    #region React To Hit

    public void ReactToHit()
    {
        StartCoroutine(Die());
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
    public Camera droneCamera { get; private set; }


    void MouseLookUpdate()
    {
        if (isLeader && operateNow)
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