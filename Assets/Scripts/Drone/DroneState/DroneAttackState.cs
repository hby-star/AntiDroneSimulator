using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAttackState : DroneState
{
    public DroneAttackState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Drone drone) :
        base(entityStateMachine, entity, animationName, drone)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (Drone.target == null)
        {
            EntityStateMachine.ChangeState(Drone.IdleState);
        }
        else
        {
            Vector3 direction = (Drone.target.transform.position - Drone.transform.position).normalized;
            Drone.Attack();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}