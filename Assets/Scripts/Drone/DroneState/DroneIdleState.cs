using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneIdleState : DroneState
{
    public DroneIdleState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Drone drone) : base(entityStateMachine, entity, animationName, drone)
    {
    }

    public override void Enter()
    {
        base.Enter();

    }

    public override void Update()
    {
        base.Update();

        Drone.ZeroVelocity();

        if (Drone.HorizontalInput != 0 || Drone.VerticalInput != 0 || Drone.UpInput != 0)
        {
            EntityStateMachine.ChangeState(Drone.MoveState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
