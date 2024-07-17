using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundState : PlayerState
{
    public PlayerGroundState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player) : base(entityStateMachine, entity, animationName, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if(Input.GetKeyDown(KeyCode.Space) && Player.IsGrounded())
        {
            EntityStateMachine.ChangeState(Player.JumpState);
        }

        if (!Player.IsGrounded())
        {
            EntityStateMachine.ChangeState(Player.AirState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

}
