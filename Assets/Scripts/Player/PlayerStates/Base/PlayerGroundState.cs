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
            if (Player.IsBusy) return;

            if (Player.StateMachine.CurrentState is not PlayerCrouchState)
            {
                _isCrouching = false;
            }

            // move
            if ((Player.HorizontalInput != 0 || Player.VerticalInput != 0) && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.MoveState);
            }

            // idle
            if (Player.HorizontalInput == 0 && Player.VerticalInput == 0 && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.IdleState);
            }

            // jump
            if (Player.JumpInput && Player.IsGrounded() && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.JumpState);
            }

            // air
            if (!Player.IsGrounded() && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.AirState);
            }

            // attack
            if (Player.AttackInput && !_isCrouching)
            {
                if (Player.CanAttack())
                {
                    EntityStateMachine.ChangeState(Player.AttackState);
                }
                else if (Player.CanReload())
                {
                    EntityStateMachine.ChangeState(Player.ReloadState);
                }
            }

            // reload
            if (Player.ReloadInput && !_isCrouching)
            {
                if (Player.CanReload())
                {
                    EntityStateMachine.ChangeState(Player.ReloadState);
                }
            }

            // dash
            if (Player.DashInput && !_isCrouching)
            {
                EntityStateMachine.ChangeState(Player.DashState);
            }

            // crouch
            if (Player.CrouchInput)
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

            // change gun
            if (Player.ChangeGunInput)
            {
                Player.ChangeGun();
            }

            // interact
            if (!Player.IsBusy &&
                (Player.PlayerEnterVehicleInput ||
                 Player.PlayerUseVehicleEmpInput ||
                 Player.PlayerUseVehicleRadarInput ||
                 Player.PlayerUseVehicleElectromagneticInterferenceInput ||
                 Player.PlayerPlacePickupShieldInput))
            {
                Player.InteractUpdate();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}