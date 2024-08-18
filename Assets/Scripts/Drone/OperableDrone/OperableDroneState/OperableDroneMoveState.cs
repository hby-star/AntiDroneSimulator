using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperableDroneMoveState : OperableDroneState
{
    public OperableDroneMoveState(EntityStateMachine entityStateMachine, Entity entity, string animationName, OperableDrone operableDrone) : base(entityStateMachine, entity, animationName, operableDrone)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        Move(OperableDrone.moveSpeed);

        if (OperableDrone.HorizontalInput == 0 && OperableDrone.VerticalInput == 0 && OperableDrone.UpInput == 0)
        {
            EntityStateMachine.ChangeState(OperableDrone.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public void Move(float moveSpeed)
    {
        if (OperableDrone.IsOperateNow())
        {
            Vector3 moveDirectionX = OperableDrone.transform.forward * (OperableDrone.HorizontalInput * moveSpeed);
            Vector3 moveDirectionZ = OperableDrone.transform.right * (OperableDrone.VerticalInput * moveSpeed);
            Vector3 moveDirectionY = OperableDrone.transform.up * (OperableDrone.UpInput * moveSpeed);
            Vector3 moveDirection = moveDirectionX + moveDirectionZ + moveDirectionY;
            moveDirection = Vector3.ClampMagnitude(moveDirection, moveSpeed);
            OperableDrone.Rigidbody.velocity = new Vector3(moveDirection.x, moveDirection.y, moveDirection.z);
        }
    }

}
