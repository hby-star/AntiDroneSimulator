using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReloadState : PlayerGroundState
{
    public PlayerReloadState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player) :
        base(entityStateMachine, entity, animationName, player)
    {
    }

    public override void Enter()
    {
        base.Enter();

        Player.Rigidbody.velocity = Vector3.zero;

        Player.StartCoroutine("BusyFor", 1f);
    }

    public override void Update()
    {
        base.Update();

        if (IsAnimationFinished())
        {
            EntityStateMachine.ChangeState(Player.IdleState);
        }

    }

    public override void Exit()
    {
        Player.bullets = Player.maxBullets;

        base.Exit();
    }
}
