using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Drone : Entity
{
    # region Drone States

    public EntityStateMachine StateMachine;
    public DroneMoveState MoveState;
    public DroneIdleState IdleState;

    # endregion

    #region Audio

    public AudioSource soundSource;
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

    #region Attack

    [SerializeField] GameObject bombPrefab;
    private GameObject bomb;
    private bool hasBomb = true;

    public void Attack()
    {
        if (hasBomb)
        {
            Vector3 spawnPosition = transform.position + Vector3.down * 0.1f;
            GameObject bombInstance = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
            Rigidbody bombRb = bombInstance.GetComponent<Rigidbody>();
            if (bombRb != null)
            {
                bombRb.velocity = Rigidbody.velocity;
            }

            hasBomb = false;
        }
    }

    #endregion

    public float speed = 10f;

    private Camera _camera;

    public bool isLeader = false;

    public IDroneControlAlgorithm DroneControlAlgorithm;

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

        DroneControlAlgorithm = new Flocking();
        DroneControlAlgorithm.DroneControlSet(this);

        if (!isLeader)
        {
            return;
        }

        _camera = GetComponentInChildren<Camera>();

        SetOperate(true);
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();
        AudioUpdate();
        MouseLookUpdate();

        if (!isLeader)
        {
            DroneControlAlgorithm.DroneControlUpdate();
        }
    }

    public void SetOperate(bool operate)
    {
        operateNow = operate;
        _camera.gameObject.SetActive(operate);
        transform.rotation = Quaternion.identity;
        if (!operate)
        {
            StateMachine.ChangeState(IdleState);
        }
    }

    #region MouseLook

    public float sensitivityHor = 9.0f;

    public Transform target;

    void MouseLookUpdate()
    {
        if (isLeader && operateNow)
            MouseXLookUpdate();
    }

    void MouseXLookUpdate()
    {
        float rotationY = CameraHorizontalInput * sensitivityHor;
        target.Rotate(0, rotationY, 0);
    }

    #endregion


    #region Handle Input

    public float HorizontalInput;
    public float VerticalInput;
    public float UpInput;
    public bool AttackInput;

    public float CameraHorizontalInput;

    void OnEnable()
    {
        if (isLeader)
        {
            Messenger<float>.AddListener(InputEvent.DRONE_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
            Messenger<float>.AddListener(InputEvent.DRONE_VERTICAL_INPUT, (value) => { VerticalInput = value; });
            Messenger<float>.AddListener(InputEvent.DRONE_UP_INPUT, (value) => { UpInput = value; });
            Messenger<bool>.AddListener(InputEvent.DRONE_ATTACK_INPUT, (value) => { AttackInput = value; });
            Messenger<float>.AddListener(InputEvent.CAMERA_HORIZONTAL_INPUT,
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
            Messenger<float>.RemoveListener(InputEvent.CAMERA_HORIZONTAL_INPUT,
                (value) => { CameraHorizontalInput = value; });
        }
    }

    #endregion
}