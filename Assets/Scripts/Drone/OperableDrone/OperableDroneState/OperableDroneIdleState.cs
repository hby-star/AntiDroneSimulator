using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperableDroneIdleState : OperableDroneState
{
    public OperableDroneIdleState(EntityStateMachine entityStateMachine, Entity entity, string animationName, OperableDrone operableDrone) : base(entityStateMachine, entity, animationName, operableDrone)
    {
    }

    public override void Enter()
    {
        base.Enter();

    }

    public override void Update()
    {
        base.Update();

        OperableDrone.ZeroVelocity();

        if (OperableDrone.HorizontalInput != 0 || OperableDrone.VerticalInput != 0 || OperableDrone.UpInput != 0)
        {
            EntityStateMachine.ChangeState(OperableDrone.MoveState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
