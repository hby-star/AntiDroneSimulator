using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMoveState : DroneState
{
    public DroneMoveState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Drone drone) : base(entityStateMachine, entity, animationName, drone)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        Drone.OperateMove(Drone.speed);

        if (FowardInput == 0 && RightInput == 0 && UpInput == 0 && DownInput == 0)
        {
            EntityStateMachine.ChangeState(Drone.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

}
