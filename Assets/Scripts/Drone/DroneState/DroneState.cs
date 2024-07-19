using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneState : EntityState
{
    protected Drone Drone;

    public DroneState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Drone drone) : base(entityStateMachine, entity, animationName)
    {
        Drone = drone;
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