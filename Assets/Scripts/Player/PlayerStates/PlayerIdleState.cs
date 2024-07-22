using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerGroundState
{
    public PlayerIdleState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player) : base(entityStateMachine, entity, animationName, player)
    {
    }

    public override void Enter()
    {
        base.Enter();

    }

    public override void Update()
    {
        base.Update();

        Player.ZeroHorVelocity();

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            EntityStateMachine.ChangeState(Player.MoveState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
