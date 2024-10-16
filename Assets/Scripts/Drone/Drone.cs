using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Drone : Entity
{
    [Header("Move Info")] public float moveSpeed = 10f;

    void SettingsAwake()
    {
        if (UIManager.Instance)
        {
            soundSource.volume = UIManager.Instance.settingsPopUp.GetComponent<Settings>().volumeSlider.value;
            electricInterferenceTime =
                UIManager.Instance.settingsPopUp.GetComponent<Settings>().empBulletDurationSlider.value;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        SettingsAwake();
    }

    protected override void Start()
    {
        base.Start();

        CameraStart();
    }


    protected override void Update()
    {
        if (IsBusy)
        {
            return;
        }

        base.Update();
        CameraUpdate();
        AudioUpdate();

        HandleStuck();
    }

    void HandleStuck()
    {
        if (FoundPlayer && Rigidbody.velocity.magnitude < 0.01f)
        {
            // 无法移动时，尝试随机水平移动
            Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0,
                UnityEngine.Random.Range(-1f, 1f));
            Rigidbody.velocity = randomDirection.normalized * moveSpeed;
            StartCoroutine(BusyFor(1f));
        }
    }


    # region 摄像机检测和跟随玩家

    [Header("Camera Info")] protected Player targetPlayer;
    [NonSerialized] public bool FoundPlayer = false;
    public float minDetectSizeInCamera = 0.0005f;
    protected Rect targetRect;
    public float cameraSmooth = 1;
    public float rotateSmooth = 1;
    public float rotateThreshold = 10;
    protected bool isPlayerDetectedInCamera = false;
    public float initialCameraFov;

    protected void CameraStart()
    {
        Camera.enabled = true;
        initialCameraFov = Camera.fieldOfView;

        UIBoxInit();
    }

    public void CameraUpdate()
    {
        if (!targetPlayer)
        {
            targetPlayer = FindObjectOfType<Player>();
        }

        if (FoundPlayer)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(targetPlayer.transform.position - Camera.transform.position);
            // 为避免镜头晃动，若角度差大于rotateThreshold再旋转
            // if (Quaternion.Angle(Camera.transform.rotation, targetRotation) > rotateThreshold)
            // {
            Camera.transform.rotation =
                Quaternion.Slerp(Camera.transform.rotation, targetRotation, Time.deltaTime * 5);
            // }
        }
        else
        {
            Vector3 moveDirection = Rigidbody.velocity;
            moveDirection.y = -moveDirection.magnitude * 0.2f;
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                // 为避免镜头晃动，若角度差大于rotateThreshold再旋转
                // if (Quaternion.Angle(Camera.transform.rotation, targetRotation) > rotateThreshold)
                // {
                Camera.transform.rotation =
                    Quaternion.Slerp(Camera.transform.rotation, targetRotation, Time.deltaTime * 5);
                // }
            }
        }

        List<Renderer> targetRenderers = targetPlayer.playerRenderers;

        // 初始化 min 和 max 为极大值和极小值
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer.isVisible)
            {
                Bounds bounds = renderer.bounds;
                Vector3[] corners = new Vector3[8];
                corners[0] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
                corners[1] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
                corners[2] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);
                corners[3] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                corners[4] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
                corners[5] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
                corners[6] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);
                corners[7] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);

                for (int i = 0; i < corners.Length; i++)
                {
                    Vector3 screenPoint = Camera.WorldToScreenPoint(corners[i]);
                    min.x = Mathf.Min(min.x, screenPoint.x);
                    min.y = Mathf.Min(min.y, screenPoint.y);
                    max.x = Mathf.Max(max.x, screenPoint.x);
                    max.y = Mathf.Max(max.y, screenPoint.y);
                }
            }
        }


        // 获取display2的分辨率
        float displayWidth = Screen.width;
        float displayHeight = Screen.height;
        if (Display.displays.Length > 1)
        {
            var display = Display.displays[1];
            displayWidth = display.systemWidth;
            displayHeight = display.systemHeight;
        }

        // 将物体的屏幕坐标转换到摄像机的视口空间
        Rect cameraRect = Camera.rect;
        float minX = min.x / displayWidth;
        float maxX = max.x / displayWidth;
        float minY = min.y / displayHeight;
        float maxY = max.y / displayHeight;
        float centerX = (minX + maxX) / 2;
        float centerY = (minY + maxY) / 2;

        // 检查物体是否在当前摄像机的视口内
        bool isInCameraView = (centerX > cameraRect.x && centerX < cameraRect.x + cameraRect.width &&
                               centerY > cameraRect.y && centerY < cameraRect.y + cameraRect.height);

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
        if (isVisible && isInCameraView && (maxY - minY > minDetectSizeInCamera || FoundPlayer))
        {
            targetRect = new Rect(min.x, displayHeight - max.y, max.x - min.x, max.y - min.y);
            isPlayerDetectedInCamera = true;

            // 调整Camera的fov
            float targetHeightScale = (max.y - min.y) / CameraManager.Instance.droneViewHeight;
            if (targetHeightScale < 0.4f)
            {
                // 平滑减小Camera的fov
                Camera.fieldOfView =
                    Mathf.Lerp(Camera.fieldOfView, initialCameraFov - 200 * Mathf.Abs(targetHeightScale - 0.4f),
                        Time.deltaTime);
            }

            if (targetHeightScale > 0.5f)
            {
                // 平滑增大Camera的fov
                Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView,
                    initialCameraFov + 200 * Mathf.Abs(targetHeightScale - 0.4f),
                    Time.deltaTime);
            }
        }
        else
        {
            targetRect = new Rect(0, 0, 0, 0);
            isPlayerDetectedInCamera = false;

            // 平滑恢复Camera的fov
            Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, initialCameraFov, Time.deltaTime);
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
        // 获取display2的分辨率
        float displayWidth = Screen.width;
        float displayHeight = Screen.height;
        if (Display.displays.Length > 1)
        {
            var display = Display.displays[1];
            displayWidth = display.systemWidth;
            displayHeight = display.systemHeight;
        }

        if (targetRect.width > 0)
        {
            uiBox.anchoredPosition = new Vector2(targetRect.x - displayWidth / 2 + targetRect.width / 2,
                -targetRect.y + displayHeight / 2 - targetRect.height / 2);
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

    protected void AvoidObstacleUpdate()
    {
        Vector3 obstaclePosition = GetObstacleRelativePosition();

        if (obstaclePosition.magnitude < detectObstacleDistance)
        {
            avoidObstacleForce = -obstaclePosition.normalized;
            avoidObstacleForce.y += 1f;
        }
        else
        {
            Collider[] colliders = Physics.OverlapBox(transform.position,
                Collider.bounds.extents + new Vector3(0.1f, 0.1f, 0.1f));
            foreach (var collider in colliders)
            {
                if (Collider.CompareTag("Ground"))
                {
                    avoidObstacleForce = (transform.position - collider.transform.position).normalized;
                    avoidObstacleForce.y += 1f;
                    return;
                }
            }

            avoidObstacleForce = Vector3.zero;
        }
    }

    public Vector3 GetObstacleRelativePosition()
    {
        Vector3 obstaclePosition = new Vector3(detectObstacleDistance, detectObstacleDistance, detectObstacleDistance);

        // 检测前后左右上下以及更多方向的障碍物
        Vector3[] directions =
        {
            transform.forward, -transform.forward, transform.right, -transform.right, transform.up, -transform.up,
            transform.forward + transform.right + transform.up, transform.forward + transform.right - transform.up,
            transform.forward - transform.right + transform.up, transform.forward - transform.right - transform.up,
            -transform.forward + transform.right + transform.up, -transform.forward + transform.right - transform.up,
            -transform.forward - transform.right + transform.up, -transform.forward - transform.right - transform.up,
            transform.forward + transform.right, transform.forward - transform.right,
            -transform.forward + transform.right, -transform.forward - transform.right,
            transform.forward + transform.up, transform.forward - transform.up,
            -transform.forward + transform.up, -transform.forward - transform.up,
            transform.right + transform.up, transform.right - transform.up,
            -transform.right + transform.up, -transform.right - transform.up
        };

        foreach (var direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, detectObstacleDistance))
            {
                // 忽略玩家和自身和炸弹
                if (!hit.collider.CompareTag("Player") &&
                    !hit.collider.CompareTag("Bomb") &&
                    !hit.collider.CompareTag("Bullet"))
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
    protected void Move()
    {
        Vector3 moveForce = avoidObstacleForce * avoidObstacleWeight + taskForce;

        if (moveForce == Vector3.zero)
        {
            Rigidbody.velocity = Vector3.zero;
            return;
        }

        moveForce *= moveSpeed;

        // 直接移动
        Rigidbody.velocity = moveForce;

        // // 先水平转向到目标方向
        // Vector3 horMoveDirection = moveForce;
        // horMoveDirection.y = 0;
        //
        // if (horMoveDirection != Vector3.zero)
        // {
        //     // 平滑转向
        //     Quaternion targetRotation = Quaternion.LookRotation(horMoveDirection);
        //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSmooth);
        //
        //     // 再向前移动
        //     Rigidbody.velocity = transform.forward * horMoveDirection.magnitude;
        // }
        //
        // // 再垂直移动
        // Rigidbody.velocity += transform.up * moveForce.y;
    }

    # endregion

    #region 受击

    [Header("Hit Info")] public ParticleSystem normalBulletEffectPrefab;
    public ParticleSystem electricInterferenceEffectPrefab;

    public float electricInterferenceTime = 5f;

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
            ZeroVelocity();
            StartCoroutine(BusyForElectricInterference(electricInterferenceTime));
        }
        else
        {
            if (hitType == HitType.NormalBullet)
            {
                ParticleSystem normalBulletEffect = Instantiate(normalBulletEffectPrefab, transform.position,
                    transform.rotation);
                normalBulletEffect.Play();
            }

            Die();
        }
    }

    private IEnumerator BusyForElectricInterference(float seconds)
    {
        IsBusy = true;
        ParticleSystem electricInterferenceEffect = Instantiate(electricInterferenceEffectPrefab, transform.position,
            transform.rotation);
        electricInterferenceEffect.Play();
        yield return new WaitForSeconds(seconds);
        IsBusy = false;
    }

    private void Die()
    {
        uiBox.gameObject.SetActive(false);
        soundSource.Stop();
        gameObject.SetActive(false);
        Destroy(gameObject);
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