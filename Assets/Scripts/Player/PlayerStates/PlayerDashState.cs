using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerGroundState
{
    private float originalColliderHeight;
    private Vector3 savedVeciocity;

    public PlayerDashState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player) :
        base(entityStateMachine, entity, animationName, player)
    {
    }

    public override void Enter()
    {
        base.Enter();

        savedVeciocity = Player.Rigidbody.velocity;

        Player.SetColliderHeight(Player.crouchColliderHeight);

        Player.soundSource.PlayOneShot(Player.dashSound);

        Player.StartCoroutine(Player.BusyFor(0.5f));
    }

    public override void Update()
    {
        base.Update();

        if (IsAnimationFinished())
        {
            EntityStateMachine.ChangeState(Player.IdleState);
        }
        else
        {
            Vector3 dashDirection = Player.transform.forward;

            Player.Rigidbody.velocity = dashDirection.normalized * Player.dashSpeed + savedVeciocity;

        }
    }

    public override void Exit()
    {
        Player.SetColliderHeight(Player.standColliderHeight);

        base.Exit();
    }
}