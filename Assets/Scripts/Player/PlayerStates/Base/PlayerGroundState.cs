using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundState : PlayerState
{
    public PlayerGroundState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player)
        : base(entityStateMachine, entity, animationName, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (Player.operateNow)
        {
            if (Input.GetKeyDown(KeyCode.Space) && Player.IsGrounded())
            {
                EntityStateMachine.ChangeState(Player.JumpState);
            }

            if (!Player.IsGrounded())
            {
                EntityStateMachine.ChangeState(Player.AirState);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (Player.CanAttack() && !Player.IsBusy)
                {
                    EntityStateMachine.ChangeState(Player.AttackState);
                }
                else
                {
                    if (Player.bullets <= 0 && !Player.IsBusy)
                    {
                        EntityStateMachine.ChangeState(Player.ReloadState);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.C) && !Player.IsBusy)
            {
                EntityStateMachine.ChangeState(Player.DashState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}