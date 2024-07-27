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

        Move(Player.moveSpeed / 2);
    }

    public override void Exit()
    {
        Player.SetColliderHeight(Player.standColliderHeight);
        base.Exit();
    }

    public void Move(float moveSpeed)
    {
        if (Player.IsOperateNow())
        {
            // Calculate movement based on vertical input & horizontal input
            Vector3 moveDirectionX = Player.transform.forward * (Player.VerticalInput * moveSpeed);
            Vector3 moveDirectionZ = Player.transform.right * (Player.HorizontalInput * moveSpeed);
            Vector3 moveDirection = moveDirectionX + moveDirectionZ;
            moveDirection = Vector3.ClampMagnitude(moveDirection, moveSpeed);
            Player.Rigidbody.velocity = new Vector3(moveDirection.x, Player.Rigidbody.velocity.y, moveDirection.z);
        }
    }
}