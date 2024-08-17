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

    public Camera playerCamera;
    public Camera[] droneCameras;
    public Camera vehicleCamera;
    public Camera backgroundCamera;  // 新增的背景摄像机

    public enum ViewType
    {
        Player,
        Drone,
    }

    public ViewType currentViewType;

    public void SwitchView(ViewType viewType)
    {
        DisableAllCameras();

        // 启用背景摄像机
        backgroundCamera.enabled = true;

        switch (viewType)
        {
            case ViewType.Player:
                playerCamera.enabled = true;
                playerCamera.rect = new Rect(0, 0, 1, 1);  // 全屏覆盖
                break;

            case ViewType.Drone:
                // 启用所有 Drone 摄像机
                foreach (var droneCamera in droneCameras)
                {
                    droneCamera.enabled = true;
                }
                playerCamera.enabled = true;  // 将 Player 相机显示在右下角

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

                // playerCamera 显示在右下角
                playerCamera.rect = new Rect((divide - 1) * droneViewWidth / Screen.width,
                    0, droneViewWidth / Screen.width, droneViewHeight / Screen.height);

                break;
        }

        currentViewType = viewType;
    }

    private void DisableAllCameras()
    {
        playerCamera.enabled = false;
        vehicleCamera.enabled = false;
        backgroundCamera.enabled = false;  // 禁用背景摄像机

        foreach (var droneCamera in droneCameras)
        {
            droneCamera.enabled = false;
        }
    }

    private void Start()
    {
        playerCamera = GameObject.FindWithTag("Player").GetComponentInChildren<Camera>();
        vehicleCamera = GameObject.FindWithTag("Vehicle").GetComponentInChildren<Camera>();

        GameObject[] droneObjects = GameObject.FindGameObjectsWithTag("Drone");
        droneCameras = new Camera[droneObjects.Length];
        for (int i = 0; i < droneObjects.Length; i++)
        {
            droneCameras[i] = droneObjects[i].GetComponentInChildren<Camera>();
        }

        // 创建背景摄像机
        CreateBackgroundCamera();

        SwitchView(currentViewType);
    }

    private void CreateBackgroundCamera()
    {
        GameObject backgroundCameraObj = new GameObject("BackgroundCamera");
        backgroundCamera = backgroundCameraObj.AddComponent<Camera>();

        // 设置背景摄像机的属性
        backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        backgroundCamera.backgroundColor = Color.black;  // 设置背景颜色为黑色或其他颜色
        backgroundCamera.cullingMask = 0;  // 不渲染任何物体
        backgroundCamera.depth = -1;  // 确保它渲染在其他摄像机之前

        // 保持背景摄像机存在于场景中
        DontDestroyOnLoad(backgroundCameraObj);
    }
}
