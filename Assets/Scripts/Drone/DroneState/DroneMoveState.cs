using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMoveState : DroneState
{
    public DroneMoveState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Drone drone) : base(entityStateMachine, entity, animationName, drone)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        Move(Drone.speed);

        if (Drone.HorizontalInput == 0 && Drone.VerticalInput == 0 && Drone.UpInput == 0)
        {
            EntityStateMachine.ChangeState(Drone.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public void Move(float moveSpeed)
    {
        if (Drone.operateNow)
        {
            Vector3 moveDirectionX = Drone.transform.forward * (Drone.HorizontalInput * moveSpeed);
            Vector3 moveDirectionZ = Drone.transform.right * (Drone.VerticalInput * moveSpeed);
            Vector3 moveDirectionY = Drone.transform.up * (Drone.UpInput * moveSpeed);
            Vector3 moveDirection = moveDirectionX + moveDirectionZ + moveDirectionY;
            moveDirection = Vector3.ClampMagnitude(moveDirection, moveSpeed);
            Drone.Rigidbody.velocity = new Vector3(moveDirection.x, moveDirection.y, moveDirection.z);
        }
    }

}
