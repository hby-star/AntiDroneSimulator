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

        if (Player.operateNow)
        {
            if (Input.GetKeyDown(KeyCode.Space) && Player.IsGrounded() && !Player.IsBusy && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.JumpState);
            }

            if (!Player.IsGrounded() && !Player.IsBusy && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.AirState);
            }

            if (Input.GetMouseButtonDown(0) && !Player.IsBusy && !_isCrouching)
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
            if (Input.GetKeyDown(KeyCode.C) && !Player.IsBusy && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.DashState);
            }

            // crouch
            if (Input.GetKeyDown(KeyCode.LeftControl) && !Player.IsBusy)
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