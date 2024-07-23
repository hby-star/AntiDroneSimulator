using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerState
{
    public PlayerAttackState(EntityStateMachine entityStateMachine, Entity entity, string animationName,
        Player player) :
        base(entityStateMachine, entity, animationName, player)
    {
    }

    public override void Enter()
    {
        base.Enter();

        Player.StartCoroutine("BusyFor", 0.5f);

        Player.Rigidbody.velocity = 0.5f * Player.Rigidbody.velocity;
        Player.bullets--;
        Player.Attack();
    }

    public override void Update()
    {
        base.Update();

        if (Player.bullets <= 0 && !Player.IsBusy)
        {
            EntityStateMachine.ChangeState(Player.ReloadState);
        }

        if (IsAnimationFinished() && !Player.IsBusy)
        {
            EntityStateMachine.ChangeState(Player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}