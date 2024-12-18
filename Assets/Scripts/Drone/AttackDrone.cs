using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AttackDrone : Drone
{
    public enum AttackDroneState
    {
        Idle,
        Patrol,
        TrackPlayer,
        ReturnToHive
    }

    public AttackDroneState attackDroneState = AttackDroneState.Idle;
    public Action<int, AttackDroneState> OnAttackDroneStateChange;

    void SettingsAwake()
    {
        moveSpeed = SettingsManager.Instance.settings.GetComponent<Settings>().attackDroneSpeedSlider.value;
    }

    protected override void Awake()
    {
        base.Awake();

        SettingsAwake();
    }

    protected override void Update()
    {
        if (IsBusy)
        {
            return;
        }

        AttackDroneUpdate();
        AvoidObstacleUpdate();

        Move();

        base.Update();
    }

    #region Attack Drone Update

    [Header("Attack Drone Info")] public Swarm swarm;
    public float throwBombRadius = 5;
    [NonSerialized] public Vector3 attackDroneTargetPosition;
    [NonSerialized] public bool attackDroneHasTarget = false;
    [NonSerialized] public bool hasBomb = false;
    [NonSerialized] public GameObject bomb = null;
    public float bombBelowLength = 0.1f;

    // float lastUpdateTime = 0;
    // float updateInterval = 0.5f;

    void AttackDroneUpdate()
    {
        // if (Time.time - lastUpdateTime < updateInterval)
        // {
        //     return;
        // }
        //
        // updateInterval = Random.Range(0.3f, 0.7f);
        // lastUpdateTime = Time.time;


        if (hasBomb)
        {
            if (!attackDroneHasTarget)
            {
                // Idle状态
                if (attackDroneState != AttackDroneState.Idle)
                {
                    attackDroneState = AttackDroneState.Idle;
                    OnAttackDroneStateChange?.Invoke(DroneID, attackDroneState);
                }

                taskForce = Vector3.zero;
                return;
            }

            // 调整炸弹朝向
            Vector3 bombDirection = attackDroneTargetPosition - transform.position;
            bombDirection.y = 0;
            if (bombDirection != Vector3.zero)
            {
                bomb.transform.rotation = Quaternion.LookRotation(bombDirection);
            }

            if (!FoundPlayer)
            {
                // 朝指定目标前进
                AttackDroneMoveToTarget();

                if (isPlayerDetectedInCamera)
                {
                    FoundPlayer = true;
                    // 向蜂群广播玩家位置
                    swarm.OnDetectDroneFoundPlayer(targetPlayer.transform.position + Vector3.up * 2);
                }
            }
            else
            {
                // Patrol 状态
                if (!isPlayerDetectedInCamera && attackDroneState != AttackDroneState.Patrol)
                {
                    attackDroneState = AttackDroneState.Patrol;
                    OnAttackDroneStateChange?.Invoke(DroneID, attackDroneState);
                }

                // TrackPlayer 状态
                if (isPlayerDetectedInCamera && attackDroneState != AttackDroneState.TrackPlayer)
                {
                    attackDroneState = AttackDroneState.TrackPlayer;
                    OnAttackDroneStateChange?.Invoke(DroneID, attackDroneState);
                }

                // 追踪并攻击玩家
                AttackDroneHitPlayer();

                if (isPlayerDetectedInCamera)
                {
                    // 向蜂群广播玩家位置
                    swarm.OnDetectDroneFoundPlayer(targetPlayer.transform.position + Vector3.up * 2);
                }
                else
                {
                    FoundPlayer = false;
                }
            }
        }
        else
        {
            // ReturnToHive 状态
            if (attackDroneState != AttackDroneState.ReturnToHive)
            {
                attackDroneState = AttackDroneState.ReturnToHive;
                OnAttackDroneStateChange?.Invoke(DroneID, attackDroneState);
            }

            // 返回蜂巢
            Vector3 directionToHive = swarm.hivePosition - transform.position;
            if (directionToHive.magnitude > swarm.hiveRadius)
            {
                taskForce = (swarm.hivePosition - transform.position).normalized;
            }
            else
            {
                taskForce = Vector3.zero;
            }
        }
    }

    void AttackDroneMoveToTarget()
    {
        // 无人机到玩家的距离
        float distanceToPlayer = (targetPlayer.transform.position + Vector3.up * 2 - transform.position).magnitude;

        // 如果玩家距离侦查无人机较近，则投放炸弹，然后返回蜂巢
        if (distanceToPlayer < throwBombRadius)
        {
            ThrowBomb();
            attackDroneTargetPosition = swarm.hivePosition;
            taskForce = Vector3.up * 5f;
        }

        // 攻击无人机到当前目标的距离
        float distanceToTarget = (attackDroneTargetPosition - transform.position).magnitude;

        // 到达目标，但是没有找到玩家
        if (distanceToTarget < 5f)
        {
            if (InHive())
            {
                // Idle状态
                attackDroneHasTarget = false;
            }
            else
            {
                // 返回蜂巢
                attackDroneTargetPosition = swarm.hivePosition;
                attackDroneHasTarget = true;
            }
        }
        // 没有到达目标，继续前进
        else
        {
            taskForce = (attackDroneTargetPosition - transform.position).normalized;
        }
    }

    void AttackDroneHitPlayer()
    {
        // 更新目标位置
        attackDroneTargetPosition = targetPlayer.transform.position + Vector3.up * 2;

        // 侦查无人机到玩家的距离
        float distanceToPlayer = (targetPlayer.transform.position + Vector3.up * 2 - transform.position).magnitude;

        // 如果玩家距离侦查无人机较近，则投放炸弹，然后返回蜂巢
        if (distanceToPlayer < throwBombRadius)
        {
            ThrowBomb();
            attackDroneTargetPosition = swarm.hivePosition;
            Rigidbody.velocity = Vector3.up * 10f;
        }
        // 如果玩家距离侦查无人机较远，则继续追踪
        else
        {
            Vector3 trackForce = targetPlayer.transform.position + Vector3.up * 2 - transform.position;
            taskForce = trackForce.normalized;
        }
    }

    public void ThrowBomb()
    {
        if (hasBomb && bomb)
        {
            bomb.transform.parent = null;
            bomb.AddComponent<Rigidbody>();
            Rigidbody bombRigidbody = bomb.GetComponent<Rigidbody>();
            bombRigidbody.velocity = Rigidbody.velocity;
            bombRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Bomb bombScript = bomb.GetComponent<Bomb>();
            bombScript.canExplode = true;
            hasBomb = false;
        }
    }

    #endregion

    bool InHive()
    {
        return (transform.position - swarm.hivePosition).magnitude < swarm.hiveRadius;
    }
}