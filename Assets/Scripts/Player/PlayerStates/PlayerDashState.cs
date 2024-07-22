using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerGroundState
{
    private float originalColliderHeight;

    public PlayerDashState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player) :
        base(entityStateMachine, entity, animationName, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        CapsuleCollider capsuleCollider = Player.Collider as CapsuleCollider;
        if (capsuleCollider != null)
        {
            originalColliderHeight = capsuleCollider.height;
            capsuleCollider.height = originalColliderHeight / 2;
        }

        Player.soundSource.PlayOneShot(Player.dashSound);

        Player.StartCoroutine("BusyFor", 0.5f);
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

            Player.Rigidbody.velocity = dashDirection.normalized * Player.dashSpeed;

        }
    }

    public override void Exit()
    {
        CapsuleCollider capsuleCollider = Player.Collider as CapsuleCollider;
        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalColliderHeight;
        }

        base.Exit();
    }
}