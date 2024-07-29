using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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
            DontDestroyOnLoad(gameObject);
            operateEntityNow = true;
            operateEntityIndex = 0;
            currentEntity = operateEntities[operateEntityIndex];
            currentCamera = currentEntity.Camera;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    public List<Entity> operateEntities;
    [NonSerialized] public bool operateEntityNow;
    public int operateEntityIndex { get; private set; }
    public Entity currentEntity { get; private set; }
    public Camera currentCamera { get; private set; }


    void Start()
    {
    }

    void Update()
    {
        HandleGameInput();

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
            else if (currentEntity is Drone drone && GameObject.FindWithTag("Drone"))
            {
                currentCamera = drone.Camera;
                HandleDroneInput();
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

    public void ChangeOperateEntity(Entity newEntity)
    {
        if(!newEntity)
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
        // Player Horizontal
        Messenger<float>.Broadcast(InputEvent.PLAYER_HORIZONTAL_INPUT, Input.GetAxis("Horizontal"));

        // Player Vertical
        Messenger<float>.Broadcast(InputEvent.PLAYER_VERTICAL_INPUT, Input.GetAxis("Vertical"));

        // Player Camera Horizontal
        Messenger<float>.Broadcast(InputEvent.PLAYER_CAMERA_HORIZONTAL_INPUT, Input.GetAxis("Mouse X"));

        // Player Camera Vertical
        Messenger<float>.Broadcast(InputEvent.PLAYER_CAMERA_VERTICAL_INPUT, Input.GetAxis("Mouse Y"));

        // Player Jump
        Messenger<bool>.Broadcast(InputEvent.PLAYER_JUMP_INPUT, Input.GetKeyDown(KeyCode.Space));

        // Player Attack
        Messenger<bool>.Broadcast(InputEvent.PLAYER_ATTACK_INPUT, Input.GetMouseButtonDown(0));

        // Player Reload
        Messenger<bool>.Broadcast(InputEvent.PLAYER_RELOAD_INPUT, Input.GetMouseButtonDown(2));

        // Player Dash
        Messenger<bool>.Broadcast(InputEvent.PLAYER_DASH_INPUT, Input.GetMouseButtonDown(1));

        // Player Crouch
        Messenger<bool>.Broadcast(InputEvent.PLAYER_CROUCH_INPUT, Input.GetKeyDown(KeyCode.LeftControl));

        // Player Change Gun
        Messenger<bool>.Broadcast(InputEvent.PLAYER_CHANGE_GUN_INPUT, Input.GetKeyDown(KeyCode.LeftShift));

        // Interact with vehicle
        // Player enter vehicle
        Messenger<bool>.Broadcast(InputEvent.VEHICLE_ENTER_EXIT_INPUT, Input.GetKeyDown(KeyCode.E));

        // Player use radar
        Messenger<bool>.Broadcast(InputEvent.VECHILE_RADAR_SWITCH_INPUT, Input.GetKeyDown(KeyCode.Alpha1));

        // Player use electric interference
        Messenger<bool>.Broadcast(InputEvent.VECHILE_ELERTIC_INTERFERENCE_INPUT, Input.GetKeyDown(KeyCode.Alpha2));

        // Player use emp
        Messenger<bool>.Broadcast(InputEvent.VECHILE_EMP_USE_INPUT, Input.GetKeyDown(KeyCode.Alpha3));

        // Interact with shield
        // Player place & pickup shield
        Messenger<bool>.Broadcast(InputEvent.PLAYER_PLACE_PICKUP_SHIELD_INPUT, Input.GetKeyDown(KeyCode.Q));
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
        // Game Pause
        if (Input.GetKeyDown(KeyCode.P))
        {
            Messenger.Broadcast(InputEvent.GAME_PAUSE_INPUT);
        }

        // Observer Mode
        if (Input.GetKeyDown(KeyCode.O))
        {
            Messenger.Broadcast(InputEvent.OBSERVER_MODE_INPUT);
        }
    }
}