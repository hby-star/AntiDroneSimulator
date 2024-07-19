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
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();
    }

    public void Move(Vector3 direction)
    {
        Rigidbody.velocity = direction * speed;
    }

    public void Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                hitCollider.GetComponent<Player>().TakeDamage(attackDamage);
            }
        }
    }

}
