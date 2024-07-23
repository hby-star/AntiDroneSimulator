using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundState
{
    public PlayerMoveState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player) :
        base(entityStateMachine, entity, animationName, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        Move();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public void Move()
    {
        if (Player.IsOperateNow())
        {
            // Calculate movement based on vertical input & horizontal input
            Vector3 moveDirectionX = Player.transform.forward * (Player.VerticalInput * Player.moveSpeed);
            Vector3 moveDirectionZ = Player.transform.right * (Player.HorizontalInput * Player.moveSpeed);
            Vector3 moveDirection = moveDirectionX + moveDirectionZ;
            moveDirection = Vector3.ClampMagnitude(moveDirection, Player.moveSpeed);
            Player.Rigidbody.velocity = new Vector3(moveDirection.x, Player.Rigidbody.velocity.y, moveDirection.z);
        }
    }
}