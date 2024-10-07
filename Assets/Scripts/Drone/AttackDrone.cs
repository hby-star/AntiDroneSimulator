using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDrone : Drone
{
    protected override void Update()
    {
        base.Update();

        AvoidObstacleUpdate();
        AttackDroneUpdate();
        Move();
    }

    #region Attack Drone Update

    [Header("Attack Drone Info")] public Swarm swarm;
    public float throwBombRadius = 5;
    [NonSerialized] public Vector3 attackDroneTargetPosition;
    [NonSerialized] public bool attackDroneHasTarget = false;
    [NonSerialized] public bool hasBomb = false;
    [NonSerialized] public GameObject bomb = null;
    public float bombBelowLength = 0.1f;

    void AttackDroneUpdate()
    {
        if (hasBomb)
        {
            if (!attackDroneHasTarget)
            {
                taskForce = Vector3.zero;
                return;
            }

            if (!FoundPlayer)
            {
                // 朝指定目标前进
                AttackDroneMoveToTarget();

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
                // 追踪并攻击玩家
                AttackDroneHitPlayer();

                if (!isPlayerDetectedInCamera)
                {
                    FoundPlayer = false;
                    LockPlayer = false;
                }
            }
        }
        else
        {
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
        float distanceToPlayer = (targetPlayer.transform.position - transform.position).magnitude;

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
        if (distanceToTarget < moveSpeed)
        {
            // 返回蜂巢
            attackDroneTargetPosition = swarm.hivePosition;
            attackDroneHasTarget = true;
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
        attackDroneTargetPosition = targetPlayer.transform.position;

        // 侦查无人机到玩家的距离
        float distanceToPlayer = (targetPlayer.transform.position - transform.position).magnitude;

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
            Vector3 trackForce = targetPlayer.transform.position - transform.position;
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
            Bomb bombScript = bomb.GetComponent<Bomb>();
            bombScript.canExplode = true;
            hasBomb = false;
        }
    }

    #endregion
}