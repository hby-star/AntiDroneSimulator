using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : EntityState
{
    protected float HorizontalInput;
    protected float VerticalInput;
    protected Player Player;

    public PlayerState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player) : base(entityStateMachine, entity, animationName)
    {
        Player = player;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        HorizontalInput = Input.GetAxis("Horizontal");
        VerticalInput = Input.GetAxis("Vertical");
    }

    public override void Exit()
    {
        base.Exit();
    }
}