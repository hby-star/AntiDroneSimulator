using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player) : base(entityStateMachine, entity, animationName, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Player.Jump();
    }

    public override void Update()
    {
        base.Update();

        if(Player.Rigidbody.velocity.y < 0)
        {
            EntityStateMachine.ChangeState(Player.AirState);
        }

    }

    public override void Exit()
    {
        base.Exit();
    }
}
