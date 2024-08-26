using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    #region Singleton
    private static SkillManager _instance;

    public static SkillManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SkillManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    _instance = obj.AddComponent<SkillManager>();
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

    public bool IsPaused { get; private set; }
    public bool IsObserving { get; private set; }
    public Camera SavedCamera { get; private set; }
    public Camera TempCamera { get; private set; }

    void Start()
    {
        IsPaused = false;
        IsObserving = false;
        CreateTempCamera();
    }

    void Update()
    {
        InputManager.Instance.operateEntityNow = !IsPaused && !IsObserving;
        if (IsObserving)
            MoveTempCamera();
    }

    private void OnEnable()
    {
        Messenger.AddListener(InputEvent.GAME_PAUSE_INPUT, GamePause);
        Messenger.AddListener(InputEvent.OBSERVER_MODE_INPUT, Observe);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener(InputEvent.GAME_PAUSE_INPUT, GamePause);
        Messenger.RemoveListener(InputEvent.OBSERVER_MODE_INPUT, Observe);
    }

    void GamePause()
    {
        IsPaused = !IsPaused;
        if (IsPaused)
        {
            Time.timeScale = 0;
            AudioListener.pause = true;
        }
        else
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
        }
    }

    public void Observe()
    {
        IsObserving = !IsObserving;
        if (IsObserving)
        {
            SwitchToTempCamera();
        }
        else
        {
            SwitchToMainCamera();
        }
    }

    void CreateTempCamera()
    {
        GameObject tempCameraObj = new GameObject("TempCamera");
        TempCamera = tempCameraObj.AddComponent<Camera>();
        TempCamera.enabled = false;
        tempCameraObj.SetActive(false);
    }

    void SwitchToTempCamera()
    {
        SavedCamera = InputManager.Instance.currentCamera;
        SavedCamera.enabled = false;
        TempCamera.transform.position = SavedCamera.transform.position;
        TempCamera.transform.rotation = SavedCamera.transform.rotation;
        TempCamera.gameObject.SetActive(true);
        TempCamera.enabled = true;
    }

    void SwitchToMainCamera()
    {
        TempCamera.enabled = false;
        TempCamera.gameObject.SetActive(false);
        SavedCamera.enabled = true;
    }

    void MoveTempCamera()
    {
        float moveSpeed = 50f;
        float rotateSpeed = 500f;

        float h = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        float v = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
        h *= moveSpeed * Time.unscaledDeltaTime;
        v *= moveSpeed * Time.unscaledDeltaTime;
        float x = Input.GetAxis("Mouse X") * rotateSpeed * Time.unscaledDeltaTime;
        float y = Input.GetAxis("Mouse Y") * rotateSpeed * Time.unscaledDeltaTime;

        // 前后和左右移动
        TempCamera.transform.Translate(new Vector3(h, 0, v));

        // 镜头水平和垂直旋转
        TempCamera.transform.Rotate(-y, x, 0);

        // 限制旋转角度，防止相机翻转
        Vector3 currentRotation = TempCamera.transform.eulerAngles;
        if (currentRotation.z != 0)
        {
            currentRotation.z = 0;
            TempCamera.transform.eulerAngles = currentRotation;
        }
    }
}