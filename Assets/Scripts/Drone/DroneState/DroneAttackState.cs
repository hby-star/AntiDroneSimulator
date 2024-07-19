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
    }

    public override void Exit()
    {
        base.Exit();
    }
}