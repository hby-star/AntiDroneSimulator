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
    public GameObject dronePrefab;
    public int droneCount = 20;
    public float detectDroneRate = 0.3f;
    public List<Drone> detectDrones;
    public List<Drone> attackDrones;

    // 炸弹
    public GameObject bombPrefab;

    // 根据比例分配无人机
    void AssignDrones()
    {
        detectDrones = new List<Drone>();
        attackDrones = new List<Drone>();

        for (int i = 0; i < droneCount; i++)
        {
            // 使用预制体实例化无人机
            GameObject droneObj = Instantiate(dronePrefab);

            // 在蜂巢位置附近随机生成位置
            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            Vector3 randomOffset = new Vector3(Mathf.Sin(randomAngle), 0f, Mathf.Cos(randomAngle)) *
                                   UnityEngine.Random.Range(0f, hiveRadius);
            Vector3 spawnPosition = hivePosition + randomOffset;
            spawnPosition.y = hivePosition.y; // 确保在同一水平线上

            droneObj.transform.position = spawnPosition;

            Drone drone = droneObj.GetComponent<Drone>();
            if (i < Mathf.RoundToInt(droneCount * detectDroneRate))
            {
                detectDrones.Add(drone);
                drone.droneType = Drone.DroneType.Detect;
                drone.droneID = i;
                drone.DetectDroneInit();
            }
            else
            {
                attackDrones.Add(drone);
                drone.droneType = Drone.DroneType.Attack;
                drone.droneID = i;
                drone.AttackDroneInit();
            }

            drone.hivePosition = hivePosition;
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

    Vector3 GenerateRandomHoneyPosition()
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

    void Start()
    {
        SettingStart();

        hivePosition = transform.position;
        hivePosition.y += 5f;
        AssignDrones();
        RandomAssignHoney(detectDrones.Count);
        AssignHoneyToDetectDrones();
    }

    void Update()
    {
        SupplyBomb();

        if (AllDronesDestoryed())
        {
            Messenger.Broadcast(GameEvent.GAME_SUCCESS);
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
            Drone drone = collider.GetComponent<Drone>();
            if (drone && drone.droneType == Drone.DroneType.Attack && !drone.hasBomb)
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

    void OnEnable()
    {
        Messenger<Vector3>.AddListener(SwarmEvent.DETECT_DRONE_FOUND_PLAYER, OnDetectDroneFoundPlayer);
        Messenger<int>.AddListener(SwarmEvent.DETECT_DRONE_ASK_FOR_NEW_HONEY, OnDetectDroneAskForNewHoney);

        Messenger.AddListener(SwarmEvent.ATTACK_DRONE_PLAYER_DIED, OnAttackDronePlayerDied);
    }

    void OnDisable()
    {
        Messenger<Vector3>.RemoveListener(SwarmEvent.DETECT_DRONE_FOUND_PLAYER, OnDetectDroneFoundPlayer);
        Messenger<int>.RemoveListener(SwarmEvent.DETECT_DRONE_ASK_FOR_NEW_HONEY, OnDetectDroneAskForNewHoney);

        Messenger.RemoveListener(SwarmEvent.ATTACK_DRONE_PLAYER_DIED, OnAttackDronePlayerDied);
    }

    void OnDetectDroneFoundPlayer(Vector3 playerPosition)
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

    void OnDetectDroneAskForNewHoney(int droneID)
    {
        // 通知发送请求的无人机新的蜜源位置
        detectDrones[droneID].detectDroneTargetPosition = GenerateRandomHoneyPosition();
    }

    void OnAttackDronePlayerDied()
    {
        // 通知所有无人机返回蜂巢
        Messenger.Broadcast(SwarmEvent.SWARM_BACK_TO_HIVE);
    }

    # endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hivePosition, hiveRadius);

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(hivePosition, honeyRadius);
    }
}