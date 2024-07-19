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

    public float speed = 10f;
    public float attackRange = 10f;
    public float attackRate = 1f;
    public float attackDamage = 10f;
    public float attackCooldown = 0f;

    public Player target;

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
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();
    }

    public void OperateMove(float moveSpeed)
    {
        // wasd 控制水平移动， qe 控制垂直移动
        float forwardInput = Input.GetAxis("Vertical");
        float rightInput = Input.GetAxis("Horizontal");
        float upInput = Input.GetKey(KeyCode.Q) ? 1 : 0;
        float downInput = Input.GetKey(KeyCode.E) ? 1 : 0;

        Vector3 moveDirection = new Vector3(rightInput, upInput - downInput, forwardInput).normalized;
        Rigidbody.velocity = moveDirection * moveSpeed;
    }
}
