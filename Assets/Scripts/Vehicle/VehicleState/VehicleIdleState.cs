using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleIdleState : VehicleState
{
    public VehicleIdleState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Vehicle vehicle) :
        base(entityStateMachine, entity, animationName, vehicle)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (Vehicle.HorizontalInput != 0 || Vehicle.VerticalInput != 0)
        {
            EntityStateMachine.ChangeState(Vehicle.MoveState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
