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
    public DroneAttackState AttackState;

    # endregion

    #region MouseLook

    public MouseLook mouseLookX;
    public MouseLook mouseLookY;

    #endregion

    public float speed = 10f;
    public float attackRange = 10f;
    public float attackRate = 1f;
    public float attackDamage = 10f;
    public float attackCooldown = 0f;

    public Player target;

    private Camera _camera;

    protected override void Awake()
    {
        base.Awake();

        StateMachine = new EntityStateMachine();

        MoveState = new DroneMoveState(StateMachine, this, "Move", this);
        IdleState = new DroneIdleState(StateMachine, this, "Idle", this);
        AttackState = new DroneAttackState(StateMachine, this, "Attack", this);
    }

    protected override void Start()
    {
        base.Start();

        StateMachine.Initialize(IdleState);
        target = null;
        _camera = GetComponentInChildren<Camera>();

        SetOperate(false);
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();
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