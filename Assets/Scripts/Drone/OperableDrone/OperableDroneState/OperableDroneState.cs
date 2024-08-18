using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperableDroneState : EntityState
{
    protected OperableDrone OperableDrone;



    public OperableDroneState(EntityStateMachine entityStateMachine, Entity entity, string animationName, OperableDrone operableDrone) : base(entityStateMachine, entity, animationName)
    {
        OperableDrone = operableDrone;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (OperableDrone.AttackInput)
        {
            OperableDrone.ThrowBomb();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}