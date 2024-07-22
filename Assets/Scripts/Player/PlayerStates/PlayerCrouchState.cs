using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerCrouchState : PlayerGroundState
{

    public PlayerCrouchState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player)
        : base(entityStateMachine, entity, animationName, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Player.SetColliderHeight(Player.crouchColliderHeight);
        Player.ZeroHorVelocity();
        Player.StartCoroutine("BusyFor", 0.5f);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        Player.SetColliderHeight(Player.standColliderHeight);
        base.Exit();
    }
}