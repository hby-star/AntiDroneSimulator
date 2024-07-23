using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundState : PlayerState
{
    private static bool _isCrouching = false;

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

        if (Player.IsOperateNow())
        {
            // move
            if ((Player.HorizontalInput != 0 || Player.VerticalInput != 0) && !Player.IsBusy && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.MoveState);
            }

            // idle
            if (Player.HorizontalInput == 0 && Player.VerticalInput == 0 && !Player.IsBusy && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.IdleState);
            }

            // jump
            if (Player.JumpInput && Player.IsGrounded() && !Player.IsBusy && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.JumpState);
            }

            // air
            if (!Player.IsGrounded() && !Player.IsBusy && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.AirState);
            }

            // attack
            if (Player.AttackInput && !Player.IsBusy && !_isCrouching)
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

            // dash
            if (Player.DashInput && !Player.IsBusy && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.DashState);
            }

            // crouch
            if (Player.CrouchInput && !Player.IsBusy)
            {
                if (!_isCrouching)
                {
                    _isCrouching = true;
                    EntityStateMachine.ChangeState(Player.CrouchState);
                }
                else
                {
                    _isCrouching = false;
                    EntityStateMachine.ChangeState(Player.IdleState);
                }
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}