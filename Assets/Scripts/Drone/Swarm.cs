using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Swarm : MonoBehaviour
{
    // 蜂群的位置和范围
    public Vector3 hivePosition;
    public float hiveRadius = 20f;

    // 蜜源的位置和随机探测范围
    public List<Vector3> honeyPositions;
    public float honeyRadius = 100f;

    // 无人机蜂群
    public GameObject detectDronePrefab;
    public GameObject attackDronePrefab;
    public int droneCount = 20;
    public float detectDroneRate = 0.3f;
    public List<DetectDrone> detectDrones;
    public List<AttackDrone> attackDrones;

    // 炸弹
    public GameObject bombPrefab;

    // 根据比例分配无人机
    void AssignDrones()
    {
        detectDrones = new List<DetectDrone>();
        attackDrones = new List<AttackDrone>();

        for (int i = 0; i < droneCount; i++)
        {
            // 在蜂巢位置附近随机生成位置
            float randomAngle = Random.Range(0f, 360f);
            Vector3 randomOffset = new Vector3(Mathf.Sin(randomAngle), 0f, Mathf.Cos(randomAngle)) *
                                   Random.Range(0f, hiveRadius);
            Vector3 spawnPosition = hivePosition + randomOffset;
            spawnPosition.y = hivePosition.y;

            if (i < Mathf.RoundToInt(droneCount * detectDroneRate))
            {
                GameObject droneObj = Instantiate(detectDronePrefab, spawnPosition, Quaternion.identity);
                DetectDrone drone = droneObj.GetComponent<DetectDrone>();
                drone.swarm = this;
                detectDrones.Add(drone);
            }
            else
            {
                GameObject droneObj = Instantiate(attackDronePrefab, spawnPosition, Quaternion.identity);
                AttackDrone drone = droneObj.GetComponent<AttackDrone>();
                drone.swarm = this;
                attackDrones.Add(drone);
            }
        }
    }

    // 随机分配蜜源
    void RandomAssignHoney(int honeyCount)
    {
        honeyPositions = new List<Vector3>();

        for (int i = 0; i < honeyCount - 1; i++)
        {
            honeyPositions.Add(GenerateRandomHoneyPosition());
        }

        // 最后一个蜜源位置为玩家位置
        Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        honeyPositions.Add(playerPosition);
    }

    public Vector3 GenerateRandomHoneyPosition()
    {
        Vector3 randomPosition = hivePosition + Random.insideUnitSphere * honeyRadius;
        randomPosition.y = hivePosition.y + 5f;
        return randomPosition;
    }

    // 将蜜源位置传递给所有侦查无人机
    void AssignHoneyToDetectDrones()
    {
        for (int i = 0; i < detectDrones.Count; i++)
        {
            detectDrones[i].detectDroneTargetPosition = honeyPositions[i];
        }

        // 攻击无人机暂时待命
        for (int i = 0; i < attackDrones.Count; i++)
        {
            attackDrones[i].attackDroneTargetPosition = hivePosition;
            attackDrones[i].attackDroneHasTarget = false;
        }
    }

    private void SettingStart()
    {
        droneCount = (int)UIManager.Instance.settingsPopUp.GetComponent<Settings>().droneNumSlider.value;
    }

    private void Awake()
    {
        SettingStart();

        hivePosition = transform.position;
        hivePosition.y += 5f;

        AssignDrones();
    }

    void Start()
    {
        RandomAssignHoney(detectDrones.Count);
        AssignHoneyToDetectDrones();
    }

    void Update()
    {
        SupplyBomb();

        if (AllDronesDestoryed())
        {
            GameManager.Instance.GameSuccess();
        }
    }

    bool AllDronesDestoryed()
    {
        foreach (Drone drone in detectDrones)
        {
            if (drone != null && drone.gameObject != null)
            {
                return false;
            }
        }

        foreach (Drone drone in attackDrones)
        {
            if (drone != null && drone.gameObject != null)
            {
                return false;
            }
        }

        return true;
    }

    // 为攻击无人机补充炸弹
    public void SupplyBomb()
    {
        Collider[] colliders = Physics.OverlapSphere(hivePosition, hiveRadius);

        foreach (Collider collider in colliders)
        {
            AttackDrone drone = collider.GetComponent<AttackDrone>();
            if (drone && !drone.hasBomb)
            {
                drone.hasBomb = true;
                Vector3 bombPosition = drone.transform.position;
                bombPosition.y -= drone.bombBelowLength;
                drone.bomb = Instantiate(bombPrefab, bombPosition, drone.transform.rotation);
                drone.bomb.transform.parent = drone.transform;
            }
        }
    }

    # region Handle Message

    public void OnDetectDroneFoundPlayer(Vector3 playerPosition)
    {
        // 通知所有无人机，侦查无人机追踪玩家，攻击无人机攻击玩家
        for (int i = 0; i < detectDrones.Count; i++)
        {
            detectDrones[i].detectDroneTargetPosition = playerPosition;
        }

        for (int i = 0; i < attackDrones.Count; i++)
        {
            attackDrones[i].attackDroneTargetPosition = playerPosition;
            attackDrones[i].attackDroneHasTarget = true;
        }

        honeyPositions = new List<Vector3>();
        honeyPositions.Add(playerPosition);
    }

    public void OnAttackDronePlayerDied()
    {
        // 通知所有无人机返回蜂巢
        for (int i = 0; i < detectDrones.Count; i++)
        {
            detectDrones[i].detectDroneTargetPosition = hivePosition;
        }

        for (int i = 0; i < attackDrones.Count; i++)
        {
            attackDrones[i].attackDroneTargetPosition = hivePosition;
            attackDrones[i].attackDroneHasTarget = false;
        }
    }

    # endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hiveRadius);

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position, honeyRadius);
    }
}