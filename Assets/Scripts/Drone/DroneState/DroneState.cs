using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneState : EntityState
{
    protected Drone Drone;
    protected float FowardInput;
    protected float RightInput;
    protected float UpInput;
    protected float DownInput;


    public DroneState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Drone drone) : base(entityStateMachine, entity, animationName)
    {
        Drone = drone;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        FowardInput = Input.GetAxis("Vertical");
        RightInput = Input.GetAxis("Horizontal");
        UpInput = Input.GetKey(KeyCode.Q) ? 1 : 0;
        DownInput = Input.GetKey(KeyCode.E) ? 1 : 0;
    }

    public override void Exit()
    {
        base.Exit();
    }
}