using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.XR;

public class InputManager : MonoBehaviour
{
    #region Singleton

    private static InputManager _instance;

    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InputManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("InputManager");
                    _instance = obj.AddComponent<InputManager>();
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
            operateEntityNow = true;
            operateEntityIndex = 0;
            operateEntities = new List<Entity>();
            operateEntities.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<Player>());
            currentEntity = operateEntities[operateEntityIndex];
            currentCamera = currentEntity.Camera;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion


    #region XR input

    InputDevice _leftController;
    InputDevice _rightController;

    void XRDeviceStart()
    {
        _leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        _rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    bool IsLeftTriggerPressed()
    {
        bool value;
        _leftController.TryGetFeatureValue(CommonUsages.triggerButton, out value);
        return value;
    }

    bool IsRightTriggerPressed()
    {
        bool value;
        _rightController.TryGetFeatureValue(CommonUsages.triggerButton, out value);
        return value;
    }

    bool IsLeftGripPressed()
    {
        bool value;
        _leftController.TryGetFeatureValue(CommonUsages.gripButton, out value);
        return value;
    }

    bool IsRightGripPressed()
    {
        bool value;
        _rightController.TryGetFeatureValue(CommonUsages.gripButton, out value);
        return value;
    }

    bool IsLeftPrimaryButtonPressed()
    {
        bool value;
        _leftController.TryGetFeatureValue(CommonUsages.primaryButton, out value);
        return value;
    }

    bool IsRightPrimaryButtonPressed()
    {
        bool value;
        _rightController.TryGetFeatureValue(CommonUsages.primaryButton, out value);
        return value;
    }

    bool IsLeftSecondaryButtonPressed()
    {
        bool value;
        _leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out value);
        return value;
    }

    bool IsRightSecondaryButtonPressed()
    {
        bool value;
        _rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out value);
        return value;
    }

    Vector2 GetLeftControllerPrimary2DAxis()
    {
        Vector2 value;
        _leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out value);
        return value;
    }

    Vector2 GetRightControllerPrimary2DAxis()
    {
        Vector2 value;
        _rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out value);
        return value;
    }

    bool IsLeftMenuButtonPressed()
    {
        bool value;
        _leftController.TryGetFeatureValue(CommonUsages.menuButton, out value);
        return value;
    }

    bool IsRightMenuButtonPressed()
    {
        bool value;
        _rightController.TryGetFeatureValue(CommonUsages.menuButton, out value);
        return value;
    }

    bool IsLeftControllerPrimary2DAxisClicked()
    {
        bool value;
        _leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out value);
        return value;
    }

    bool IsRightControllerPrimary2DAxisClicked()
    {
        bool value;
        _rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out value);
        return value;
    }

    #endregion


    public List<Entity> operateEntities;
    [NonSerialized] public bool operateEntityNow;
    public int operateEntityIndex { get; private set; }
    public Entity currentEntity { get; private set; }
    public Camera currentCamera { get; private set; }

    void Start()
    {
        XRDeviceStart();
    }

    void Update()
    {
        HandleGameInput();
        //HandleViewSwitch();

        if (operateEntityNow)
        {
            if (operateEntities.Count > 1)
            {
                HandleOperateSwitch();
            }

            if (currentEntity is Player player && GameObject.FindWithTag("Player"))
            {
                currentCamera = player.Camera;
                HandlePlayerInput();
            }
            else if (currentEntity is Vehicle vehicle && GameObject.FindWithTag("Vehicle"))
            {
                currentCamera = vehicle.Camera;
                HandleVehicleInput();
            }
        }
    }


    void HandleOperateSwitch()
    {
        // Tab 切换操作对象
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            operateEntityIndex++;
            if (operateEntityIndex >= operateEntities.Count)
            {
                operateEntityIndex = 0;
            }

            ChangeOperateEntity(operateEntities[operateEntityIndex]);
        }
    }

    // void HandleViewSwitch()
    // {
    //     // V 切换视角
    //     if (Input.GetKeyDown(KeyCode.V))
    //     {
    //         if(CameraManager.Instance.currentViewType == CameraManager.ViewType.Player)
    //         {
    //             CameraManager.Instance.SwitchView(CameraManager.ViewType.Drone);
    //         }
    //         else if(CameraManager.Instance.currentViewType == CameraManager.ViewType.Drone)
    //         {
    //             CameraManager.Instance.SwitchView(CameraManager.ViewType.Player);
    //         }
    //
    //     }
    // }

    public void ChangeOperateEntity(Entity newEntity)
    {
        if (!newEntity)
        {
            Debug.LogError("ChangeOperateEntity: newEntity is null");
            return;
        }

        currentEntity.SetOperate(false);
        currentEntity = newEntity;
        currentEntity.SetOperate(true);
    }

    void HandlePlayerInput()
    {
        Vector2 leftControllerPrimary2DAxis = GetLeftControllerPrimary2DAxis();
        Vector2 rightControllerPrimary2DAxis = GetRightControllerPrimary2DAxis();

        // Player Horizontal
        // 在 VR 中可能需要通过手柄的特定轴来模拟水平移动，假设左摇杆的水平轴对应水平移动
        Messenger<float>.Broadcast(InputEvent.PLAYER_HORIZONTAL_INPUT,  leftControllerPrimary2DAxis.x);

        // Player Vertical
        // 假设左摇杆的垂直轴对应垂直移动
        Messenger<float>.Broadcast(InputEvent.PLAYER_VERTICAL_INPUT, leftControllerPrimary2DAxis.y);

        // Player Camera Horizontal
        // VR 中可能没有类似鼠标的相机水平移动，这里可以根据实际需求进行调整，比如通过头部转动等方式模拟相机水平移动，目前暂不处理
        Messenger<float>.Broadcast(InputEvent.PLAYER_CAMERA_HORIZONTAL_INPUT, rightControllerPrimary2DAxis.x);

        // Player Camera Vertical
        // 类似相机水平移动
        Messenger<float>.Broadcast(InputEvent.PLAYER_CAMERA_VERTICAL_INPUT, rightControllerPrimary2DAxis.y);

        // Player Jump
        Messenger<bool>.Broadcast(InputEvent.PLAYER_JUMP_INPUT, IsRightPrimaryButtonPressed());

        // Player Attack
        Messenger<bool>.Broadcast(InputEvent.PLAYER_ATTACK_INPUT, IsRightTriggerPressed());

        // Player Reload
        Messenger<bool>.Broadcast(InputEvent.PLAYER_RELOAD_INPUT, IsRightSecondaryButtonPressed());

        // Player Dash
        Messenger<bool>.Broadcast(InputEvent.PLAYER_DASH_INPUT, IsLeftTriggerPressed());

        // Player Crouch
        Messenger<bool>.Broadcast(InputEvent.PLAYER_CROUCH_INPUT, IsLeftGripPressed());

        // Player Change Gun
        Messenger<bool>.Broadcast(InputEvent.PLAYER_CHANGE_GUN_INPUT, IsRightGripPressed());

        // Interact with vehicle
        // Player enter vehicle
        Messenger<bool>.Broadcast(InputEvent.VEHICLE_ENTER_EXIT_INPUT, false);

        // Player use radar
        Messenger<bool>.Broadcast(InputEvent.VECHILE_RADAR_SWITCH_INPUT, false);

        // Player use electric interference
        Messenger<bool>.Broadcast(InputEvent.VECHILE_ELERTIC_INTERFERENCE_INPUT, false);

        // Player use emp
        Messenger<bool>.Broadcast(InputEvent.VECHILE_EMP_USE_INPUT, false);

        // Interact with shield
        // Player place & pickup shield
        Messenger<bool>.Broadcast(InputEvent.PLAYER_PLACE_PICKUP_SHIELD_INPUT, false);
    }

    void HandleDroneInput()
    {
        // Drone Horizontal
        Messenger<float>.Broadcast(InputEvent.DRONE_HORIZONTAL_INPUT, Input.GetAxis("Vertical"));

        // Drone Vertical
        Messenger<float>.Broadcast(InputEvent.DRONE_VERTICAL_INPUT, Input.GetAxis("Horizontal"));

        // Drone Up
        bool up = Input.GetKey(KeyCode.Q);
        bool down = Input.GetKey(KeyCode.E);
        up = up && !down;
        down = down && !up;
        Messenger<float>.Broadcast(InputEvent.DRONE_UP_INPUT, up ? 1 : down ? -1 : 0);

        // Drone Attack
        Messenger<bool>.Broadcast(InputEvent.DRONE_ATTACK_INPUT, Input.GetKeyDown(KeyCode.F));

        // Drone Camera Horizontal
        Messenger<float>.Broadcast(InputEvent.DRONE_CAMERA_HORIZONTAL_INPUT, Input.GetAxis("Mouse X"));
    }

    void HandleVehicleInput()
    {
        // Vehicle Horizontal
        Messenger<float>.Broadcast(InputEvent.VEHICLE_HORIZONTAL_INPUT, Input.GetAxis("Horizontal"));

        // Vehicle Vertical
        Messenger<float>.Broadcast(InputEvent.VEHICLE_VERTICAL_INPUT, Input.GetAxis("Vertical"));

        // Vehicle Camera Horizontal
        Messenger<float>.Broadcast(InputEvent.VEHICLE_CAMERA_HORIZONTAL_INPUT, Input.GetAxis("Mouse X"));

        // Vehicle Camera Vertical
        Messenger<float>.Broadcast(InputEvent.VEHICLE_CAMERA_VERTICAL_INPUT, Input.GetAxis("Mouse Y"));

        // Player exit vehicle
        Messenger<bool>.Broadcast(InputEvent.VEHICLE_ENTER_EXIT_INPUT, Input.GetKeyDown(KeyCode.E));
    }


    void HandleGameInput()
    {
        // SKill Game Pause
        if (Input.GetKeyDown(KeyCode.P))
        {
            Messenger.Broadcast(InputEvent.GAME_PAUSE_INPUT);
        }

        // SKill Observer Mode
        if (Input.GetKeyDown(KeyCode.O))
        {
            Messenger.Broadcast(InputEvent.OBSERVER_MODE_INPUT);
        }

        if (IsLeftPrimaryButtonPressed())
        {
            GameManager.Instance.InGameMenu();
        }
    }
}