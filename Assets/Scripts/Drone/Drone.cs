using System.Collections;
using System.Collections.Generic;
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

    [Header("Attack Info")] [SerializeField]
    GameObject bombPrefab;

    private GameObject bomb;
    private bool hasBomb = true;

    public void Attack()
    {
        if (hasBomb)
        {
            Vector3 spawnPosition = transform.position + Vector3.down * 0.5f;
            GameObject bombInstance = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
            bombInstance.transform.forward = transform.forward;
            Rigidbody bombRb = bombInstance.GetComponent<Rigidbody>();
            if (bombRb != null)
            {
                bombRb.velocity = Rigidbody.velocity;
            }

            hasBomb = false;
        }
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

        DroneControlAlgorithm = AlgorithmManager.Instance.GetAlgorithm();
        DroneControlAlgorithm.DroneControlSet(this);

        droneCamera = GetComponentInChildren<Camera>();

        SetOperate(InputManager.Instance.operateTarget == InputManager.OperateTarget.Drone && isLeader);

        Rigidbody.freezeRotation = true;
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();

        AudioUpdate();
        MouseLookUpdate();

        if (!operateNow)
            DroneControlAlgorithm.DroneControlUpdate();
    }

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