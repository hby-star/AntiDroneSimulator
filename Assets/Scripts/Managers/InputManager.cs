using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Player player;
    public Drone drone;

    enum OperateTarget
    {
        Player,
        Drone
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        HandleOperateSwitch();


        if (player.operateNow)
        {
            HandlePlayerInput();
            HandleCameraInput();
        }

        if (drone.operateNow)
        {
            HandleDroneInput();
            HandleCameraInput();
        }
    }

    void HandleOperateSwitch()
    {
        // Tab 切换操作对象
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OperateTarget target = player.operateNow ? OperateTarget.Drone : OperateTarget.Player;
            switch (target)
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


        // Player Dash
        Messenger<bool>.Broadcast(InputEvent.PLAYER_DASH_INPUT, Input.GetMouseButtonDown(1));


        // Player Crouch
        Messenger<bool>.Broadcast(InputEvent.PLAYER_CROUCH_INPUT, Input.GetKeyDown(KeyCode.LeftControl));
    }

    void HandleDroneInput()
    {
        // Drone Horizontal
        Messenger<float>.Broadcast(InputEvent.DRONE_VERTICAL_INPUT, Input.GetAxis("Horizontal"));

        // Drone Vertical
        Messenger<float>.Broadcast(InputEvent.DRONE_HORIZONTAL_INPUT, Input.GetAxis("Vertical"));

        // Drone Up
        bool up = Input.GetKey(KeyCode.Q);
        bool down = Input.GetKey(KeyCode.E);
        up = up && !down;
        down = down && !up;
        Messenger<float>.Broadcast(InputEvent.DRONE_UP_INPUT, up ? 1 : down ? -1 : 0);

        // Drone Attack
        Messenger<bool>.Broadcast(InputEvent.DRONE_ATTACK_INPUT, Input.GetKeyDown(KeyCode.F));
    }

    void HandleCameraInput()
    {
        // Camera Horizontal
        Messenger<float>.Broadcast(InputEvent.CAMERA_HORIZONTAL_INPUT, Input.GetAxis("Mouse X"));

        // Camera Vertical
        Messenger<float>.Broadcast(InputEvent.CAMERA_VERTICAL_INPUT, Input.GetAxis("Mouse Y"));
    }
}