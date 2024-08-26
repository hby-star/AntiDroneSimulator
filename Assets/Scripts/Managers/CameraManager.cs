using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    public Canvas display1;
    public Canvas display2;

    public Camera playerCamera;
    public Camera playerCameraCopy;
    public Camera[] droneCameras;
    public Camera vehicleCamera;
    public Camera backgroundCamera; // 新增的背景摄像机

    private void Start()
    {
        // 启用Display 2（假设已经有两个显示器连接）
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate(); // 启用Display 2
            SetCanvasToDisplay(display2, 1);
            Debug.Log("Display 2 activated.");
        }

        SetupCameras();
        SetCanvasToDisplay(display1, 0);
    }

    void SetupCameras()
    {
        // 获取所有摄像机
        vehicleCamera = GameObject.FindWithTag("Vehicle").GetComponentInChildren<Camera>();

        GameObject[] droneObjects = GameObject.FindGameObjectsWithTag("Drone");
        droneCameras = new Camera[droneObjects.Length];
        for (int i = 0; i < droneObjects.Length; i++)
        {
            droneCameras[i] = droneObjects[i].GetComponentInChildren<Camera>();
        }

        // 初始化背景摄像机
        SetupBackgroundCamera();

        // Display 1: Player视角
        playerCamera.targetDisplay = 0; // Display 1
        playerCamera.enabled = true;
        playerCamera.rect = new Rect(0, 0, 1, 1); // 全屏覆盖

        // Display 2: Swarm & Player视角
        playerCameraCopy.targetDisplay = 1; // Display 2
        playerCameraCopy.enabled = true;
        for (int i = 0; i < droneCameras.Length; i++)
        {
            droneCameras[i].targetDisplay = 1; // Display 2
            droneCameras[i].enabled = true;
        }

        // 配置Display 2的布局
        float divide = Mathf.Ceil(Mathf.Sqrt(droneCameras.Length + 1));
        float droneViewWidth = Screen.width / divide;
        float droneViewHeight = Screen.height / divide;

        for (int i = 0; i < droneCameras.Length; i++)
        {
            int row = i / (int)divide;
            int col = i % (int)divide;
            Rect rect = new Rect(col * droneViewWidth / Screen.width,
                1 - (row + 1) * droneViewHeight / Screen.height,
                droneViewWidth / Screen.width, droneViewHeight / Screen.height);
            droneCameras[i].rect = rect;
        }
        playerCameraCopy.rect = new Rect((divide - 1) * droneViewWidth / Screen.width,
            0, droneViewWidth / Screen.width, droneViewHeight / Screen.height);
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
        backgroundCamera.backgroundColor = Color.black;  // 设置背景颜色为黑色或其他颜色
        backgroundCamera.cullingMask = 0;  // 不渲染任何物体
        backgroundCamera.depth = -1;  // 确保它渲染在其他摄像机之前

        // 设置背景摄像机的目标显示器
        backgroundCamera.targetDisplay = 1; // Display 2
        backgroundCamera.enabled = true;
        backgroundCamera.rect = new Rect(0, 0, 1, 1); // 全屏覆盖
    }

    void SetCanvasToDisplay(Canvas canvas, int displayIndex)
    {

        // 获取目标显示器的分辨率
        var display = Display.displays[displayIndex];
        float displayWidth = display.systemWidth;
        float displayHeight = display.systemHeight;

        // 获取 Canvas 的 RectTransform 组件
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

        // 设置 Canvas 尺寸以匹配目标显示器
        canvasRectTransform.sizeDelta = new Vector2(displayWidth, displayHeight);
    }

}