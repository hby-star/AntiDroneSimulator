using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using static KATXRWalker;

public class KATXRWalker : MonoBehaviour
{
    public GameObject xr;
    public GameObject eye;

    public enum ExecuteMethod
    {
        RigidBody,
        CharactorController,
        MovePosition
    }
    public ExecuteMethod executeMethod = ExecuteMethod.RigidBody;

    public enum InputSource
    {
        Local,
        Nexus
    }

    public InputSource inputSource = InputSource.Local;

    [Header("SpeedSettings")]
    [Range(0.5f, 10.0f)]
    public float speedMul = 1.0f;
    public enum SpeedMode
    {
        [Tooltip("Movement speed increases as actual speed increases")]
        linear,
        [Tooltip("Movement speed remains constant")]
        constant
    }
    [Tooltip("Character movement speed mode")]
    public SpeedMode speedMode = SpeedMode.linear;
    [Range(0.0f, 6.0f)]
    [Tooltip("Movement speed in constant mode")]
    public float constantSpeed = 2.0f;
    [Range(0.0f, 6.0f)]
    [Tooltip("Only when the actual speed exceeds this value will it move")]
    public float constantSpeedThreshold = 1.0f;

    protected Vector3 lastPosition = Vector3.zero;
    //protected Vector3 defaultEyeOffset = Vector3.zero;

    protected float yawCorrection;

    void Awake()
    {
        var lib_name = "";
        if(inputSource == InputSource.Local)
        {
            lib_name = KATNativeSDK.sdk_warpper_lib;
        }
        else
        {
            lib_name = KATNativeSDK.nexus_client_lib;
        }

        if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            lib_name += ".dll";
        }

        KATNativeSDK.sdkLoader = new NativeFunctionLoader(lib_name);
    }

    void Start()
    {
        
    }

    void OnDestroy()
    {
#if UNITY_EDITOR_WIN
        KATNativeSDK.UnloadSDKLibrary();
#endif
    }

    void FixedUpdate()
    {
        var ws = KATNativeSDK.GetWalkStatus();

        if (!ws.connected)
        {
            return;
        }

        //Calibration Stage 
        var lastCalibrationTime = KATNativeSDK.GetLastCalibratedTimeEscaped();                 //Get last calibration time as double

        if (ws.deviceDatas[0].btnPressed || lastCalibrationTime < 0.08)                                               //Check if need calibration
        {
            var hmdYaw = eye.transform.eulerAngles.y;
            var bodyYaw = ws.bodyRotationRaw.eulerAngles.y;

            yawCorrection = bodyYaw - hmdYaw;

            var pos = transform.position;
            var eyePos = eye.transform.position;
            pos.x = eyePos.x;
            pos.z = eyePos.z;
            transform.position = pos;
            lastPosition = transform.position;
            return;
        }

        transform.rotation = ws.bodyRotationRaw * Quaternion.Inverse( Quaternion.Euler(new Vector3(0,yawCorrection,0)));

        if (Input.GetKey(KeyCode.W))
        {
            ws.moveSpeed = new Vector3(0, 0, 1);
        }

        switch (speedMode)
        {
            case SpeedMode.linear:
                break;
            case SpeedMode.constant:
                if (ws.moveSpeed.magnitude >= constantSpeedThreshold)
                    ws.moveSpeed = ws.moveSpeed.normalized * constantSpeed;
                else
                    ws.moveSpeed = Vector3.zero;
                break;
            default:
                break;
        }

        switch(executeMethod)
        {
            case ExecuteMethod.CharactorController: 
                {
                    var ch = GetComponent<CharacterController>();
                    ch.SimpleMove(transform.rotation * ws.moveSpeed);
                }
                break;
            case ExecuteMethod.MovePosition:
                {
                    transform.position += (transform.rotation * ws.moveSpeed * Time.fixedDeltaTime);
                }
                break;
            case ExecuteMethod.RigidBody:
                {
                    var r = GetComponent<Rigidbody>();
                    r.velocity = transform.rotation * ws.moveSpeed;
                }
                break;
        } 
    }


    void LateUpdate()
    {
        var offset = transform.position - lastPosition;
        offset.y = 0;
        xr.transform.position += offset;

        lastPosition = transform.position;
    }
}
