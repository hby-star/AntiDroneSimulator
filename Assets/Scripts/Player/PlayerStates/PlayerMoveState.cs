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

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            Move(Player.moveSpeed);
        }
        else
        {
            EntityStateMachine.ChangeState(Player.IdleState);
        }


    }

    public override void Exit()
    {
        base.Exit();
    }

    public void Move(float moveSpeed)
    {
        if (Player.operateNow)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            float mouseX = Input.GetAxis("Mouse X");

            // Calculate movement based on vertical input & horizontal input
            Vector3 moveDirectionX = Player.transform.forward * (verticalInput * moveSpeed);
            Vector3 moveDirectionZ = Player.transform.right * (horizontalInput * moveSpeed);
            Vector3 moveDirection = moveDirectionX + moveDirectionZ;
            moveDirection = Vector3.ClampMagnitude(moveDirection, moveSpeed);
            Player.Rigidbody.velocity = new Vector3(moveDirection.x, Player.Rigidbody.velocity.y, moveDirection.z);
        }
    }
}