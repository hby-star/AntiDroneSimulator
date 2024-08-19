using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Drone : Entity
{
    public enum DroneType
    {
        None,
        Detect,
        Attack,
        Operate,
    }

    [Header("Swarm Info")] public DroneType droneType;
    public int droneID;
    public Vector3 hivePosition;
    public float hiveRadius = 20f;


    protected override void Awake()
    {
        base.Awake();

        droneType = DroneType.None;
    }

    protected override void Start()
    {
        base.Start();

        CameraStart();
    }


    protected override void Update()
    {
        base.Update();
        CameraUpdate();
        AudioUpdate();

        AvoidObstacleUpdate();

        switch (droneType)
        {
            case DroneType.Detect:
                DetectDroneUpdate();
                break;
            case DroneType.Attack:
                AttackDroneUpdate();
                break;
            default:
                // do nothing
                return;
        }


        Move();
    }

    #region Detect Drone Update

    [Header("Detect Drone Info")] public float detectDroneSpeed = 10;
    [NonSerialized] public Vector3 detectDroneTargetPosition;
    [NonSerialized] public bool FoundPlayer = false;
    [NonSerialized] public bool LockPlayer = false;

    public void DetectDroneInit()
    {
        moveSpeed = detectDroneSpeed;

        hasBomb = false;
        bomb = null;
    }


    void DetectDroneUpdate()
    {
        if (!FoundPlayer)
        {
            moveSpeed = detectDroneSpeed;

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
            moveSpeed = detectDroneSpeed / 2;

            // 向蜂群广播玩家位置
            Messenger<Vector3>.Broadcast(SwarmEvent.DETECT_DRONE_FOUND_PLAYER, targetPlayer.transform.position);
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
        if (distanceToTarget < detectDroneSpeed / 2)
        {
            taskForce = Vector3.zero;
            Messenger<int>.Broadcast(SwarmEvent.DETECT_DRONE_ASK_FOR_NEW_HONEY, droneID);
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
        directionToPlayer.y = 0;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 如果玩家距离侦查无人机较近，则延缓追踪
        if (distanceToPlayer < detectDroneSpeed * 1.5f)
        {
            taskForce = Vector3.zero;
        }
        // 如果玩家距离侦查无人机较远，则追踪
        else
        {
            Vector3 trackForce = targetPlayer.transform.position - transform.position;
            taskForce = trackForce.normalized;
        }
    }

    #endregion

    #region Attack Drone Update

    [Header("Attack Drone Info")] public float attackDroneSpeed = 5;
    public float throwBombRadius = 5;
    [NonSerialized] public Vector3 attackDroneTargetPosition;
    [NonSerialized] public bool attackDroneHasTarget = false;
    [NonSerialized] public bool hasBomb = false;
    [NonSerialized] public GameObject bomb;
    public float bombBelowLength = 0.1f;

    public void AttackDroneInit()
    {
        moveSpeed = attackDroneSpeed;

        hasBomb = false;
        bomb = null;
    }

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
            Vector3 directionToHive = hivePosition - transform.position;
            if (directionToHive.magnitude > hiveRadius)
            {
                taskForce = (hivePosition - transform.position).normalized;
            }
            else
            {
                taskForce = Vector3.zero;
            }
        }
    }

    void AttackDroneMoveToTarget()
    {
        // 攻击无人机到当前目标的距离
        float distanceToTarget = (attackDroneTargetPosition - transform.position).magnitude;

        // 到达目标，但是没有找到玩家
        if (distanceToTarget < attackDroneSpeed)
        {
            // 返回蜂巢
            attackDroneTargetPosition = hivePosition;
            taskForce = Vector3.zero;
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
            attackDroneTargetPosition = hivePosition;
            taskForce = Vector3.up * 3f;
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
        if (hasBomb)
        {
            bomb.transform.parent = null;
            bomb.AddComponent<Rigidbody>();
            Rigidbody bombRigidbody = bomb.GetComponent<Rigidbody>();
            bombRigidbody.velocity = Rigidbody.velocity;
            hasBomb = false;
        }
    }

    #endregion

    # region Handle Swarm Message

    private void OnEnable()
    {
        Messenger.AddListener(SwarmEvent.SWARM_BACK_TO_HIVE, OnSwarmBackToHive);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener(SwarmEvent.SWARM_BACK_TO_HIVE, OnSwarmBackToHive);
    }

    private void OnSwarmBackToHive()
    {
        attackDroneTargetPosition = hivePosition;
        detectDroneTargetPosition = hivePosition;
    }

    # endregion


    # region 摄像机检测和跟随玩家

    [Header("Camera Info")] private Player targetPlayer;
    public float minDetectScaleInCamera = 20f;
    private Renderer[] targetRenderers;
    private Rect targetRect;
    public float cameraSmooth = 1;
    public float rotateSmooth = 1;
    private bool isPlayerDetectedInCamera = false;

    private void CameraStart()
    {
        Camera.enabled = true;

        UIBoxInit();
    }

    public void CameraUpdate()
    {
        if (!targetPlayer)
        {
            targetPlayer = FindObjectOfType<Player>();
            targetRenderers = targetPlayer.GetComponentsInChildren<Renderer>();
        }

        if (LockPlayer)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(targetPlayer.transform.position - Camera.transform.position);
            Camera.transform.rotation =
                Quaternion.Slerp(Camera.transform.rotation, targetRotation, Time.deltaTime * cameraSmooth);
        }
        else
        {
            Vector3 lookAtPosition = transform.position + transform.forward;
            lookAtPosition.y -= transform.forward.magnitude * 0.2f;
            Quaternion targetRotation = Quaternion.LookRotation(lookAtPosition - Camera.transform.position);
            Camera.transform.rotation =
                Quaternion.Slerp(Camera.transform.rotation, targetRotation, Time.deltaTime * rotateSmooth);
        }


        // 获取物体的边界
        Bounds[] bounds = new Bounds[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            bounds[i] = targetRenderers[i].bounds;
        }

        // 找到屏幕空间中的边界
        Vector3 min = Camera.WorldToScreenPoint(bounds[0].min);
        Vector3 max = Camera.WorldToScreenPoint(bounds[0].max);

        for (int i = 1; i < bounds.Length; i++)
        {
            min = Vector3.Min(min, Camera.WorldToScreenPoint(bounds[i].min));
            min = Vector3.Min(min, Camera.WorldToScreenPoint(bounds[i].max));
            max = Vector3.Max(max, Camera.WorldToScreenPoint(bounds[i].min));
            max = Vector3.Max(max, Camera.WorldToScreenPoint(bounds[i].max));
        }

        // 将物体的屏幕坐标转换到摄像机的视口空间
        Rect cameraRect = Camera.rect;
        float minX = min.x / Screen.width;
        float maxX = max.x / Screen.width;
        float minY = min.y / Screen.height;
        float maxY = max.y / Screen.height;

        // 检查物体是否在当前摄像机的视口内
        bool isInCameraView = (minX > cameraRect.x && maxX < cameraRect.x + cameraRect.width &&
                               minY > cameraRect.y && maxY < cameraRect.y + cameraRect.height);

        // 检查物体是否被遮挡
        bool isVisible = true;
        RaycastHit hit;
        if (Physics.Linecast(Camera.transform.position,
                targetPlayer.transform.position + targetPlayer.standColliderHeight / 2 * Vector3.up,
                out hit))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                isVisible = false;
            }
        }


        // 计算边界框的位置和大小
        if (isVisible && isInCameraView && maxY - minY > cameraRect.height / minDetectScaleInCamera)
        {
            targetRect = new Rect(min.x, Screen.height - max.y, max.x - min.x, max.y - min.y);
            isPlayerDetectedInCamera = true;
        }
        else
        {
            targetRect = new Rect(0, 0, 0, 0);
            isPlayerDetectedInCamera = false;
        }

        UIBoxUpdate();
    }

    public RectTransform uiBoxPrefab;
    private RectTransform uiBox;

    void UIBoxInit()
    {
        uiBox = Instantiate(uiBoxPrefab, CameraManager.Instance.display2.transform);
        uiBox.transform.SetParent(CameraManager.Instance.display2.transform);
        uiBox.gameObject.SetActive(false);
    }

    void UIBoxUpdate()
    {
        if (targetRect.width > 0)
        {
            uiBox.anchoredPosition = new Vector2(targetRect.x - Screen.width / 2 + targetRect.width / 2,
                -targetRect.y + Screen.height / 2 - targetRect.height / 2);
            uiBox.sizeDelta = new Vector2(targetRect.width, targetRect.height);
            uiBox.gameObject.SetActive(true);
        }
        else
        {
            uiBox.gameObject.SetActive(false);
        }
    }

    # endregion

    # region 避障

    [Header("Avoid Obstacle Info")] public float detectObstacleDistance = 5; // 避障距离

    void AvoidObstacleUpdate()
    {
        Vector3 obstaclePosition = GetObstacleRelativePosition();

        if (obstaclePosition.magnitude < detectObstacleDistance - 1)
        {
            avoidObstacleForce = -obstaclePosition.normalized * (detectObstacleDistance - obstaclePosition.magnitude) /
                                 detectObstacleDistance;
        }
        else
        {
            avoidObstacleForce = Vector3.zero;
        }
    }

    public Vector3 GetObstacleRelativePosition()
    {
        Vector3 obstaclePosition = new Vector3(detectObstacleDistance, detectObstacleDistance, detectObstacleDistance);

        // 检测前后左右上下以及8个拐角方向的障碍物
        Vector3[] directions = new Vector3[]
        {
            transform.forward, -transform.forward, transform.right, -transform.right, transform.up, -transform.up,
            transform.forward + transform.right + transform.up, transform.forward + transform.right - transform.up,
            transform.forward - transform.right + transform.up, transform.forward - transform.right - transform.up,
            -transform.forward + transform.right + transform.up, -transform.forward + transform.right - transform.up,
            -transform.forward - transform.right + transform.up, -transform.forward - transform.right - transform.up
        };

        foreach (var direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, detectObstacleDistance))
            {
                // 忽略玩家和自身和炸弹
                if (!hit.collider.CompareTag("Player") &&
                    !hit.collider.gameObject.CompareTag("Bomb"))
                {
                    if (hit.distance < obstaclePosition.magnitude)
                    {
                        obstaclePosition = hit.point - transform.position;
                    }
                }
            }
        }

        return obstaclePosition;
    }

    # endregion

    # region 移动

    [NonSerialized] public Vector3 avoidObstacleForce;
    [NonSerialized] public Vector3 taskForce;
    public float avoidObstacleWeight = 5f;

    // 根据MoveForce移动
    public void Move()
    {
        Vector3 moveForce = avoidObstacleForce * avoidObstacleWeight + taskForce;

        if (moveForce == Vector3.zero)
        {
            Rigidbody.velocity = Vector3.zero;
            return;
        }

        moveForce *= moveSpeed;

        // 先水平转向到目标方向
        Vector3 horMoveDirection = moveForce;
        horMoveDirection.y = 0;

        if (horMoveDirection != Vector3.zero)
        {
            // 平滑转向
            Quaternion targetRotation = Quaternion.LookRotation(horMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSmooth);

            // 再向前移动
            Rigidbody.velocity = transform.forward * horMoveDirection.magnitude;
        }

        // 再垂直移动
        Rigidbody.velocity += transform.up * moveForce.y;
    }

    # endregion

    #region 受击

    public enum HitType
    {
        NormalBullet,
        EmpBullet,
        NetBullet,
        ElectricInterference,
    }

    public void ReactToHit(HitType hitType)
    {
        if (hitType == HitType.ElectricInterference)
        {
            StartCoroutine(BusyFor(5));
        }
        else
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        this.transform.Rotate(-75, 0, 0);

        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }

    #endregion

    #region 音效

    [Header("Audio Info")] public AudioSource soundSource;
    public AudioClip flySound;

    void AudioUpdate()
    {
        if (!soundSource.isPlaying)
        {
            soundSource.clip = flySound;
            soundSource.loop = true;
            soundSource.Play();
        }
    }

    #endregion
}