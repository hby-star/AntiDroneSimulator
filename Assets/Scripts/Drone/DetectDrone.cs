using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectDrone : Drone
{
    protected override void Update()
    {
        base.Update();

        AvoidObstacleUpdate();
        DetectDroneUpdate();
        Move();
    }

    #region Detect Drone Update

    [Header("Detect Drone Info")] public Swarm swarm;
    public Vector3 detectDroneTargetPosition;


    void DetectDroneUpdate()
    {
        if (!FoundPlayer)
        {
            // 寻找玩家
            DetectDroneMoveToTarget();

            if (isPlayerDetectedInCamera)
            {
                FoundPlayer = true;
                LockPlayer = true;
            }
        }
        else
        {
            // 向蜂群广播玩家位置
            swarm.OnDetectDroneFoundPlayer(targetPlayer.transform.position);
            // 持续追踪玩家
            DetectDroneTrackPlayer();

            if (!isPlayerDetectedInCamera)
            {
                FoundPlayer = false;
                LockPlayer = false;
            }
        }
    }

    void DetectDroneMoveToTarget()
    {
        // 侦查无人机到当前目标的距离
        float distanceToTarget = (detectDroneTargetPosition - transform.position).magnitude;

        // 到达目标，但是没有找到玩家
        if (distanceToTarget < moveSpeed)
        {
            detectDroneTargetPosition = swarm.GenerateRandomHoneyPosition();
        }
        // 没有到达目标，继续前进
        else
        {
            taskForce = (detectDroneTargetPosition - transform.position).normalized;
        }
    }

    void DetectDroneTrackPlayer()
    {
        // 更新目标位置
        detectDroneTargetPosition = targetPlayer.transform.position;

        // 侦查无人机到玩家的距离
        Vector3 directionToPlayer = targetPlayer.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        directionToPlayer.y = 0;

        // 如果玩家距离侦查无人机较近，则随机移动
        float random_size = 0.3f;
        if (distanceToPlayer < moveSpeed * 2f)
        {
            // 随机移动
            taskForce = new Vector3(Random.Range(-random_size, random_size), Random.Range(0, random_size),
                Random.Range(-random_size, random_size));
        }
        // 如果玩家距离侦查无人机较远，则追踪
        else
        {
            taskForce = directionToPlayer.normalized;
        }
    }

    #endregion
}