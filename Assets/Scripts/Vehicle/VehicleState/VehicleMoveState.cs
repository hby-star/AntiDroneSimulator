using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMoveState : VehicleState
{
    public VehicleMoveState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Vehicle vehicle) :
        base(entityStateMachine, entity, animationName, vehicle)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        Move(Vehicle.moveSpeed);

        if (Vehicle.HorizontalInput == 0 && Vehicle.VerticalInput == 0)
        {
            EntityStateMachine.ChangeState(Vehicle.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void Move(float moveSpeed)
    {
        // Steer the front wheels based on horizontal input
        Vehicle.currentSteerAngle = Vehicle.maxSteerAngle * Vehicle.HorizontalInput;
        Vehicle.wheelLf.steerAngle = Vehicle.currentSteerAngle;
        Vehicle.wheelRf.steerAngle = Vehicle.currentSteerAngle;

        // Apply motor force to all wheels based on vertical input
        float motorTorque = Vehicle.motorForce * Vehicle.VerticalInput;
        Vehicle.wheelLf.motorTorque = motorTorque;
        Vehicle.wheelRf.motorTorque = motorTorque;
        Vehicle.wheelLb.motorTorque = motorTorque;
        Vehicle.wheelRb.motorTorque = motorTorque;
    }

}
