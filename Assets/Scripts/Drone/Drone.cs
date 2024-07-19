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

    #region MouseLook

    public MouseLook mouseLookX;
    public MouseLook mouseLookY;

    #endregion

    public float speed = 10f;

    private Camera _camera;

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
        _camera = GetComponentInChildren<Camera>();

        SetOperate(false);
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();
        AudioUpdate();
    }

    public void OperateMove(float moveSpeed)
    {
        if (operateNow)
        {
            // wasd 控制水平移动， qe 控制垂直移动
            float forwardInput = Input.GetAxis("Vertical");
            float rightInput = Input.GetAxis("Horizontal");
            float upInput = Input.GetKey(KeyCode.Q) ? 1 : 0;
            float downInput = Input.GetKey(KeyCode.E) ? 1 : 0;
            upInput = upInput - downInput;

            // Calculate movement based on vertical input & horizontal input
            Vector3 moveDirectionX = transform.forward * (forwardInput * moveSpeed);
            Vector3 moveDirectionZ = transform.right * (rightInput * moveSpeed);
            Vector3 moveDirectionY = transform.up * (upInput * moveSpeed);
            Vector3 moveDirection = moveDirectionX + moveDirectionZ + moveDirectionY;
            moveDirection = Vector3.ClampMagnitude(moveDirection, moveSpeed);
            Rigidbody.velocity = new Vector3(moveDirection.x, moveDirection.y, moveDirection.z);

            if (Input.GetKeyDown(KeyCode.F))
            {
                Attack();
            }
        }
    }

    public void SetOperate(bool operate)
    {
        operateNow = operate;
        _camera.gameObject.SetActive(operate);
        mouseLookX.enabled = operate;
        mouseLookY.enabled = operate;
        transform.rotation = Quaternion.identity;
        if (!operate)
        {
            StateMachine.ChangeState(IdleState);
        }
    }
}