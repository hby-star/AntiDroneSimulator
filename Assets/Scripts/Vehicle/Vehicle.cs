using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Vehicle : Entity
{
    #region States

    public EntityStateMachine StateMachine { get; private set; }
    public VehicleState IdleState { get; private set; }
    public VehicleState MoveState { get; private set; }

    #endregion

    public Player currentPlayer;

    public WheelCollider wheelLf;
    public WheelCollider wheelRf;
    public WheelCollider wheelLb;
    public WheelCollider wheelRb;

    public float maxSteerAngle = 30;
    public float currentSteerAngle;
    public float motorForce = 50;

    protected override void Awake()
    {
        base.Awake();

        StateMachine = new EntityStateMachine();

        IdleState = new VehicleIdleState(StateMachine, this, "Idle", this);
        MoveState = new VehicleMoveState(StateMachine, this, "Move", this);
    }

    protected override void Start()
    {
        base.Start();

        StateMachine.Initialize(IdleState);

        SetOperate(InputManager.Instance.currentEntity is Vehicle);

        currentPlayer = null;
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();
    }


    public override void SetOperate(bool operateNow)
    {
        base.SetOperate(operateNow);

        if (!operateNow)
        {
            StateMachine.ChangeState(IdleState);
        }
    }

    public override void InteractUpdate()
    {
        if (PlayerExitInput)
        {
            if (InputManager.Instance.currentEntity is Vehicle)
            {
                if (currentPlayer)
                {
                    currentPlayer.transform.parent = null;
                    currentPlayer.gameObject.SetActive(true);
                    InputManager.Instance.ChangeOperateEntity(currentPlayer);
                    currentPlayer = null;
                }
            }
        }
    }


    #region HandleInput

    public float HorizontalInput { get; private set; }
    public float VerticalInput { get; private set; }
    public float CameraHorizontalInput { get; private set; }
    public float CameraVerticalInput { get; private set; }
    public bool PlayerExitInput { get; private set; }

    void OnEnable()
    {
        Messenger<float>.AddListener(InputEvent.Vehicle_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.Vehicle_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<float>.AddListener(InputEvent.Vehicle_CAMERA_HORIZONTAL_INPUT,
            (value) => { CameraHorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.Vehicle_CAMERA_VERTICAL_INPUT,
            (value) => { CameraVerticalInput = value; });

        Messenger<bool>.AddListener(InputEvent.Vehicle_ENTER_EXIT_INPUT, (value) => { PlayerExitInput = value; });
    }

    void OnDisable()
    {
        Messenger<float>.RemoveListener(InputEvent.Vehicle_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.Vehicle_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.Vehicle_CAMERA_HORIZONTAL_INPUT,
            (value) => { CameraHorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.Vehicle_CAMERA_VERTICAL_INPUT,
            (value) => { CameraVerticalInput = value; });

        Messenger<bool>.RemoveListener(InputEvent.Vehicle_ENTER_EXIT_INPUT, (value) => { PlayerExitInput = value; });
    }

    #endregion
}