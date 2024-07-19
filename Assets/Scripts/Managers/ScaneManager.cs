using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaneManager : MonoBehaviour
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
        // Tab 键切换操作对象
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
}
