using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    public enum OperateTarget
    {
        Player,
        Drone
    }

    public Player player;
    public Drone drone;
    public bool operateEntityNow;
    public OperateTarget operateTarget;
    public Camera operateCamera;


    void Start()
    {
        operateEntityNow = true;
    }

    void Update()
    {
        HandleGameInput();

        if (operateEntityNow)
        {
            OperateTarget target = player.IsOperateNow() ? OperateTarget.Player : OperateTarget.Drone;

            if (player != null && drone != null)
            {
                HandleOperateSwitch();
            }

            if (player && operateTarget == OperateTarget.Player)
            {
                operateCamera = player.playerCamera;
                HandlePlayerInput();
            }

            if (drone && operateTarget == OperateTarget.Drone)
            {
                operateCamera = drone.droneCamera;
                HandleDroneInput();
            }
        }
    }

    void HandleOperateSwitch()
    {
        // Tab 切换操作对象
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            operateTarget = operateTarget == OperateTarget.Player ? OperateTarget.Drone : OperateTarget.Player;
            switch (operateTarget)
            {
                case OperateTarget.Player:
                    player.SetOperate(true);
                    drone.SetOperate(false);
                    break;
                case OperateTarget.Drone:
                    player.SetOperate(false);
                    drone.SetOperate(true);
                    break;
            }
        }
    }

    void HandlePlayerInput()
    {
        // Player Horizontal
        Messenger<float>.Broadcast(InputEvent.PLAYER_HORIZONTAL_INPUT, Input.GetAxis("Horizontal"));

        // Player Vertical
        Messenger<float>.Broadcast(InputEvent.PLAYER_VERTICAL_INPUT, Input.GetAxis("Vertical"));

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

        // Player Camera Horizontal
        Messenger<float>.Broadcast(InputEvent.PLAYER_CAMERA_HORIZONTAL_INPUT, Input.GetAxis("Mouse X"));

        // Player Camera Vertical
        Messenger<float>.Broadcast(InputEvent.PLAYER_CAMERA_VERTICAL_INPUT, Input.GetAxis("Mouse Y"));
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