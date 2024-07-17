using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    # region Player States
    public EntityStateMachine StateMachine;
    public PlayerMoveState MoveState;
    public PlayerIdleState IdleState;
    # endregion

    protected override void Awake()
    {
        base.Awake();

        StateMachine = new EntityStateMachine();

        MoveState = new PlayerMoveState(StateMachine, this, "Move", this);
        IdleState = new PlayerIdleState(StateMachine, this, "Idle", this);
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
}
