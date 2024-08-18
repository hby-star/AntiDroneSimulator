using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : Entity
{
    public enum DroneType
    {
        Detect,
        Attack,
        Operate,
    }

    public DroneType droneType;

    # region 摄像机检测和跟随玩家

    public Player targetPlayer;
    private Renderer[] targetRenderers;
    private Rect targetRect;
    private float cameraSmooth = 5;
    [NonSerialized] public bool FoundPlayer = false;
    [NonSerialized] public bool LockPlayer = false;

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
            Vector3 lookAtPosition = transform.position + transform.forward - transform.up;
            Quaternion targetRotation = Quaternion.LookRotation(lookAtPosition - Camera.transform.position);
            Camera.transform.rotation =
                Quaternion.Slerp(Camera.transform.rotation, targetRotation, Time.deltaTime * cameraSmooth);
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
        if (isVisible && isInCameraView)
        {
            targetRect = new Rect(min.x, Screen.height - max.y, max.x - min.x, max.y - min.y);
            LockPlayer = true;
        }
        else
        {
            targetRect = new Rect(0, 0, 0, 0);
            LockPlayer = false;
        }
    }

    void OnGUI()
    {
        if (Camera.enabled)
        {
            if (targetRect.width > 0)
            {
                GUI.Box(targetRect, "");
            }
        }
    }

    # endregion

    # region 检测周围障碍物

    public float detectObstacleDistance = 5; // 避障距离

    public Vector3 GetObstaclePosition()
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
                // 忽略玩家和无人机
                if (!hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Drone"))
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

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        CameraUpdate();
        AudioUpdate();
    }

    # region Move

    // 根据MoveForce移动
    public void Move(Vector3 moveForce)
    {
        // 先水平转向到目标方向
        Vector3 horMoveDirection = moveForce;
        horMoveDirection.y = 0;

        // 平滑转向
        Quaternion targetRotation = Quaternion.LookRotation(horMoveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * cameraSmooth);

        // 再向前移动
        Rigidbody.velocity = transform.forward * horMoveDirection.magnitude;

        // 再垂直移动
        Rigidbody.velocity += transform.up * moveForce.y;
    }

    # endregion

    #region React To Hit

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

    #region Audio

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