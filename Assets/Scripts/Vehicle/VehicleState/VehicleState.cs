using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleState : EntityState
{
    protected Vehicle Vehicle;

    public VehicleState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Vehicle vehicle) : base(entityStateMachine, entity, animationName)
    {
        Vehicle = vehicle;
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
