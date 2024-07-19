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

        Drone.ZeroVelocity();
    }

    public override void Update()
    {
        base.Update();

        if (Drone.target != null)
        {
            EntityStateMachine.ChangeState(Drone.MoveState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
