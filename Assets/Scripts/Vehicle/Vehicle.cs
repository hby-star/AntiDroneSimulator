using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : Entity
{
    public WheelCollider wheelLf;
    public WheelCollider wheelRf;
    public WheelCollider wheelLb;
    public WheelCollider wheelRb;

    public float maxSteerAngle = 30;
    private float currentSteerAngle;
    public float motorForce = 50;

    public Camera vehicleCamera;

    #region HandleInput

    public float HorizontalInput { get; private set; }
    public float VerticalInput { get; private set; }
    public float CameraHorizontalInput { get; private set; }
    public float CameraVerticalInput { get; private set; }

    void OnEnable()
    {
        Messenger<float>.AddListener(InputEvent.PLAYER_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.PLAYER_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<float>.AddListener(InputEvent.PLAYER_CAMERA_HORIZONTAL_INPUT, (value) => { CameraHorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.PLAYER_CAMERA_VERTICAL_INPUT, (value) => { CameraVerticalInput = value; });
    }

    void OnDisable()
    {
        Messenger<float>.RemoveListener(InputEvent.PLAYER_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.PLAYER_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.PLAYER_CAMERA_HORIZONTAL_INPUT, (value) => { CameraHorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.PLAYER_CAMERA_VERTICAL_INPUT, (value) => { CameraVerticalInput = value; });
    }

    #endregion

}
