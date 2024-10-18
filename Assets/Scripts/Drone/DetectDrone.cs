using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DetectDrone : Drone
{
    public enum DetectDroneState
    {
        Patrol,
        TrackPlayer
    }

    public DetectDroneState detectDroneState = DetectDroneState.Patrol;
    public Action<int, DetectDroneState> OnDetectDroneStateChange;

    void SettingsAwake()
    {
        if (UIManager.Instance)
        {
            moveSpeed = UIManager.Instance.settingsPopUp.GetComponent<Settings>().detectDroneSpeedSlider.value;
        }
    }

    protected override void Awake()
    {
        base.Start();

        SettingsAwake();
    }

    protected override void Update()
    {
        if (IsBusy)
        {
            return;
        }

        DetectDroneUpdate();
        AvoidObstacleUpdate();
        Move();

        base.Update();
    }

    #region Detect Drone Update

    [Header("Detect Drone Info")] public Swarm swarm;
    public Vector3 detectDroneTargetPosition;


    void DetectDroneUpdate()
    {
        if (!FoundPlayer)
        {
            // Patrol 状态
            if (detectDroneState != DetectDroneState.Patrol)
            {
                detectDroneState = DetectDroneState.Patrol;
                OnDetectDroneStateChange?.Invoke(DroneID, detectDroneState);
            }

            // 寻找玩家
            DetectDroneMoveToTarget();

            if (isPlayerDetectedInCamera)
            {
                FoundPlayer = true;
                // 向蜂群广播玩家位置
                swarm.OnDetectDroneFoundPlayer(targetPlayer.transform.position);
            }
        }
        else
        {
            // 持续追踪玩家
            DetectDroneTrackPlayer();

            // TrackPlayer 状态
            if (isPlayerDetectedInCamera && detectDroneState != DetectDroneState.TrackPlayer)
            {
                detectDroneState = DetectDroneState.TrackPlayer;
                OnDetectDroneStateChange?.Invoke(DroneID, detectDroneState);
            }

            if (!isPlayerDetectedInCamera && detectDroneState != DetectDroneState.Patrol)
            {
                detectDroneState = DetectDroneState.Patrol;
                OnDetectDroneStateChange?.Invoke(DroneID, detectDroneState);
            }

            if (isPlayerDetectedInCamera)
            {
                // 向蜂群广播玩家位置
                swarm.OnDetectDroneFoundPlayer(targetPlayer.transform.position);
            }
            else
            {
                FoundPlayer = false;
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
        float horDistanceToPlayer = new Vector3(directionToPlayer.x, 0, directionToPlayer.z).magnitude;
        float verDistanceToPlayer = directionToPlayer.y;

        // 如果玩家距离侦查无人机较近，则随机移动
        float random_size = 1f;
        if (horDistanceToPlayer < moveSpeed * 2.5f)
        {
            if (horDistanceToPlayer < moveSpeed * 2f)
            {
                taskForce = -directionToPlayer.normalized;
                taskForce.y = 0;
            }
            else
            {
                // 随机移动
                if (!isRandomMove)
                {
                    taskForce = new Vector3(Random.Range(-random_size, random_size), 0,
                        Random.Range(-random_size, random_size));
                    StartCoroutine(IsRandomMoveFor(1f));
                }
            }

            if (verDistanceToPlayer > 8f)
            {
                taskForce.y -= 2f;
            }
        }
        // 如果玩家距离侦查无人机较远，则追踪
        else
        {
            taskForce = directionToPlayer.normalized;
        }
    }

    public bool isRandomMove = false;

    public IEnumerator IsRandomMoveFor(float seconds)
    {
        isRandomMove = true;
        yield return new WaitForSeconds(seconds);
        isRandomMove = false;
    }

    #endregion
}