using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

public class CameraManager : MonoBehaviour
{
    #region Singleton

    private static CameraManager _instance;

    public static CameraManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CameraManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CameraManager");
                    _instance = obj.AddComponent<CameraManager>();
                }
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    public Canvas display1;
    public Canvas display2;

    //public Camera playerCameraCopy;
    public Camera[] droneCameras;
    public Camera vehicleCamera;
    public Camera backgroundCamera; // 新增的背景摄像机

    public float droneViewWidth;
    public float droneViewHeight;

    private Vector2Int display1Resolution;
    private Vector2Int display2Resolution;

    private void Start()
    {
        // 设置显示器2
        if (Display.displays.Length > 1)
        {
            display2Resolution = new Vector2Int(Display.displays[1].systemWidth, Display.displays[1].systemHeight);
        }
        else
        {
            display2Resolution = new Vector2Int(1920, 1080);
        }

        display1Resolution = new Vector2Int(Display.displays[0].systemWidth, Display.displays[0].systemHeight);
        RectTransform display1RectTransform = display1.GetComponent<RectTransform>();
        display1RectTransform.sizeDelta = new Vector2(display1Resolution.x, display1Resolution.y);
        RectTransform display2RectTransform = display2.GetComponent<RectTransform>();
        display2RectTransform.sizeDelta = new Vector2(display2Resolution.x, display2Resolution.y);

        SetupCameras();
    }

    void SetupCameras()
    {
        // 获取所有摄像机
        // playerCamera = GameObject.FindWithTag("PlayerCamera").transform.GetChild(0).GetComponent<Camera>();
        // playerCameraCopy = GameObject.FindWithTag("PlayerCamera").transform.GetChild(1).GetComponent<Camera>();

        GameObject swarmObject = GameObject.FindWithTag("Swarm");
        Swarm swarm = swarmObject.GetComponent<Swarm>();
        droneCameras = new Camera[swarm.droneCount];
        for (int i = 0; i < swarm.droneCount; i++)
        {
            if (i < swarm.detectDrones.Count)
            {
                droneCameras[i] = swarm.detectDrones[i].Camera;
            }
            else
            {
                droneCameras[i] = swarm.attackDrones[i - swarm.detectDrones.Count].Camera;
            }
        }

        // Display 1: Player视角
        // playerCamera.targetDisplay = 0; // Display 1
        // playerCamera.enabled = true;
        // playerCamera.rect = new Rect(0, 0, 1, 1); // 全屏覆盖

        // Display 2: Swarm & Player视角
        // playerCameraCopy.targetDisplay = 1; // Display 2
        // playerCameraCopy.enabled = true;
        for (int i = 0; i < droneCameras.Length; i++)
        {
            droneCameras[i].targetDisplay = 1; // Display 2
            droneCameras[i].enabled = true;
        }

        // 配置Display 2的布局
        float divide = Mathf.Ceil(Mathf.Sqrt(droneCameras.Length + 1));
        droneViewWidth = display2Resolution[0] / divide;
        droneViewHeight = display2Resolution[1] / divide;

        float droneRectWidth = 1 / divide;
        float droneRectHeight = 1 / divide;

        for (int i = 0; i < droneCameras.Length; i++)
        {
            int row = i / (int)divide;
            int col = i % (int)divide;
            Rect rect = new Rect(col * droneRectWidth,
                1 - (row + 1) * droneRectHeight,
                droneRectWidth, droneRectHeight);
            droneCameras[i].rect = rect;
        }
        // playerCameraCopy.rect = new Rect((divide - 1) * droneViewWidth / Screen.width,
        //     0, droneViewWidth / Screen.width, droneViewHeight / Screen.height);
    }

    void SetupBackgroundCamera()
    {
        if (backgroundCamera == null)
        {
            GameObject backgroundCameraObject = new GameObject("BackgroundCamera");
            backgroundCamera = backgroundCameraObject.AddComponent<Camera>();
            DontDestroyOnLoad(backgroundCameraObject);
        }

        // 设置背景摄像机的属性
        backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        backgroundCamera.backgroundColor = Color.black; // 设置背景颜色为黑色或其他颜色
        backgroundCamera.cullingMask = 0; // 不渲染任何物体
        backgroundCamera.depth = -1; // 确保它渲染在其他摄像机之前

        // 设置背景摄像机的目标显示器
        backgroundCamera.targetDisplay = 1; // Display 2
        backgroundCamera.enabled = true;
        backgroundCamera.rect = new Rect(0, 0, 1, 1); // 全屏覆盖
        backgroundCamera.stereoTargetEye = StereoTargetEyeMask.None; // 关闭VR
    }
}