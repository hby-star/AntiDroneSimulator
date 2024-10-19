using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Drone : Entity
{
    [NonSerialized] public int DroneID;

    [Header("Move Info")] public float moveSpeed = 10f;

    void SettingsAwake()
    {
        if (UIManager.Instance)
        {
            flySoundSource.volume *= UIManager.Instance.settingsPopUp.GetComponent<Settings>().volumeSlider.value;
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

        CameraUpdate();
        AudioUpdate();
    }

    void HandleStuck()
    {
        if (FoundPlayer && Rigidbody.velocity.magnitude < 0.01f)
        {
            // 无法移动时，尝试随机水平移动
            Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0,
                UnityEngine.Random.Range(-1f, 1f));
            Rigidbody.velocity = Vector3.Lerp(Rigidbody.velocity, randomDirection, Time.deltaTime);
            StartCoroutine(BusyFor(0.5f));
        }
    }


    # region 摄像机检测和跟随玩家

    [Header("Camera Info")] protected Player targetPlayer;
    [NonSerialized] public bool FoundPlayer = false;
    public float minDetectSizeInCamera = 0.0005f;
    protected Rect targetRect;
    protected bool isPlayerDetectedInCamera = false;
    public float initialCameraFov;

    // To Optimize
    float displayWidth;
    float displayHeight;
    float halfDisplayWidth;
    float halfDisplayHeight;
    float cameraRate;
    float minSizeTimesCameraRate;
    List<Vector3[]> playerRenderersBounds = new List<Vector3[]>();
    List<Collider> playerRenderers = new List<Collider>();
    Rect cameraRect;
    // Optimize End

    protected void CameraStart()
    {
        Camera.enabled = true;
        initialCameraFov = Camera.fieldOfView;

        UIBoxInit();


        // To optimize
        if (!targetPlayer)
        {
            targetPlayer = FindObjectOfType<Player>();
        }

        displayWidth = Screen.width;
        displayHeight = Screen.height;
        if (Display.displays.Length > 1)
        {
            var display = Display.displays[1];
            displayWidth = display.systemWidth;
            displayHeight = display.systemHeight;
        }

        halfDisplayWidth = displayWidth / 2;
        halfDisplayHeight = displayHeight / 2;

        float divide =
            Mathf.Ceil(Mathf.Sqrt(UIManager.Instance.settingsPopUp.GetComponent<Settings>().droneNumSlider.value + 1));
        float droneViewHeight = Screen.height / divide;
        cameraRate = droneViewHeight / displayHeight;
        minSizeTimesCameraRate = minDetectSizeInCamera * cameraRate;
        cameraRect = Camera.rect;
        // optimize end
    }

    float lastCameraUpdateTime = 0;
    float cameraUpdateInterval = 0.05f;

    public void CameraUpdate()
    {
        if (playerRenderers.Count == 0)
        {
            playerRenderers = targetPlayer.playerRenderers;
            playerRenderersBounds = targetPlayer.playerRenderersBounds;
        }

        Vector3 targetPosition = targetPlayer.transform.position;
        float targetPlayerHeight = targetPlayer.standColliderHeight;

        if (FoundPlayer)
        {
            var targetRotation =
                Quaternion.LookRotation(targetPosition - Camera.transform.position);
            Camera.transform.rotation =
                Quaternion.Slerp(Camera.transform.rotation, targetRotation, Time.deltaTime * 5);
        }
        else
        {
            var moveDirection = Rigidbody.velocity;
            moveDirection.y = -moveDirection.magnitude * 0.5f;
            if (moveDirection != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(moveDirection);
                Camera.transform.rotation =
                    Quaternion.Slerp(Camera.transform.rotation, targetRotation, Time.deltaTime);
            }
        }

        if (Time.time - lastCameraUpdateTime < cameraUpdateInterval)
        {
            return;
        }

        lastCameraUpdateTime = Time.time;
        cameraUpdateInterval = UnityEngine.Random.Range(0.02f, 0.06f);

        // 初始化 min 和 max 为极大值和极小值
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);

        for (int i = 0; i < playerRenderers.Count; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Vector3 screenPoint = Camera.WorldToScreenPoint(playerRenderersBounds[i][j] + playerRenderers[i].bounds.center);
                min.x = Mathf.Min(min.x, screenPoint.x);
                min.y = Mathf.Min(min.y, screenPoint.y);
                max.x = Mathf.Max(max.x, screenPoint.x);
                max.y = Mathf.Max(max.y, screenPoint.y);
            }
        }

        // 将物体的屏幕坐标转换到摄像机的视口空间
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
        if (Physics.Linecast(Camera.transform.position,
                targetPosition +
                targetPlayerHeight / 2 * Vector3.up,
                out RaycastHit hit))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                isVisible = false;
            }
        }

        // 计算边界框的位置和大小
        if (isVisible && isInCameraView && (maxY - minY > minSizeTimesCameraRate || FoundPlayer))
        {
            targetRect = new Rect(min.x, displayHeight - max.y, max.x - min.x, max.y - min.y);
            isPlayerDetectedInCamera = true;

            // 调整Camera的fov
            float targetHeightScale = (max.y - min.y) / CameraManager.Instance.droneViewHeight;
            float fovLimit = 200 * Mathf.Abs(targetHeightScale - 0.4f);
            if (targetHeightScale < 0.4f)
            {
                // 平滑减小Camera的fov
                Camera.fieldOfView =
                    Mathf.Lerp(Camera.fieldOfView, initialCameraFov - fovLimit, Time.deltaTime);
            }

            if (targetHeightScale > 0.5f)
            {
                // 平滑增大Camera的fov
                Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, initialCameraFov + fovLimit, Time.deltaTime);
            }
        }
        else
        {
            targetRect = new Rect(0, 0, 0, 0);
            isPlayerDetectedInCamera = false;

            // 平滑恢复Camera的fov
            Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, initialCameraFov, Time.deltaTime / 4f);
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
            uiBox.anchoredPosition = new Vector2(targetRect.x - halfDisplayWidth + targetRect.width / 2,
                -targetRect.y + halfDisplayHeight - targetRect.height / 2);
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

    // To Optimize
    private Vector3 obstaclePosition;
    private float obstacleDistance;

    Vector3[] directions =
    {
        Vector3.forward, -Vector3.forward, Vector3.right, -Vector3.right, Vector3.up, -Vector3.up,
        Vector3.forward + Vector3.right + Vector3.up, Vector3.forward + Vector3.right - Vector3.up,
        Vector3.forward - Vector3.right + Vector3.up, Vector3.forward - Vector3.right - Vector3.up,
        -Vector3.forward + Vector3.right + Vector3.up, -Vector3.forward + Vector3.right - Vector3.up,
        -Vector3.forward - Vector3.right + Vector3.up, -Vector3.forward - Vector3.right - Vector3.up,
        Vector3.forward + Vector3.right, Vector3.forward - Vector3.right,
        -Vector3.forward + Vector3.right, -Vector3.forward - Vector3.right,
        Vector3.forward + Vector3.up, Vector3.forward - Vector3.up,
        -Vector3.forward + Vector3.up, -Vector3.forward - Vector3.up,
        Vector3.right + Vector3.up, Vector3.right - Vector3.up,
        -Vector3.right + Vector3.up, -Vector3.right - Vector3.up
    };
    // Optimize End

    float lastAvoidObstacleUpdateTime = 0;
    float avoidObstacleUpdateInterval = 0.5f;

    protected void AvoidObstacleUpdate()
    {
        if (Time.time - lastAvoidObstacleUpdateTime < avoidObstacleUpdateInterval)
        {
            return;
        }

        lastAvoidObstacleUpdateTime = Time.time;
        avoidObstacleUpdateInterval = UnityEngine.Random.Range(0.2f, 0.8f);


        GetObstacleRelativePosition();
        if (obstacleDistance < detectObstacleDistance)
        {
            avoidObstacleForce = -obstaclePosition.normalized;
            avoidObstacleForce.y += UnityEngine.Random.Range(0.2f, 0.5f);
        }
        else
        {
            // Collider[] colliders = Physics.OverlapBox(transform.position,
            //     Collider.bounds.size);
            // foreach (var collider in colliders)
            // {
            //     if (Collider.CompareTag("Ground"))
            //     {
            //         avoidObstacleForce = (transform.position - collider.transform.position).normalized;
            //         avoidObstacleForce.y += 1f;
            //         return;
            //     }
            // }

            avoidObstacleForce = Vector3.zero;
        }
    }

    public void GetObstacleRelativePosition()
    {
        obstacleDistance = detectObstacleDistance + 1f;

        // 检测前后左右上下以及更多方向的障碍物
        foreach (var direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, detectObstacleDistance))
            {
                // 忽略玩家和自身和炸弹
                if (hit.collider.CompareTag("Drone") || hit.collider.CompareTag("Ground"))
                {
                    if (hit.distance < obstacleDistance)
                    {
                        obstacleDistance = hit.distance;
                        obstaclePosition = hit.point - transform.position;
                    }
                }
            }
        }
    }

    # endregion

    # region 移动

    private Vector3 avoidObstacleForce;
    protected Vector3 taskForce;
    public float avoidObstacleWeight = 5f;

    // 根据MoveForce移动
    protected void Move()
    {
        Vector3 moveForce = avoidObstacleForce * avoidObstacleWeight + taskForce;

        if (moveForce == Vector3.zero)
        {
            Rigidbody.velocity = Vector3.Lerp(Rigidbody.velocity, Vector3.zero, Time.deltaTime);
            return;
        }

        moveForce *= moveSpeed;

        // 平滑移动
        Rigidbody.velocity = Vector3.Lerp(Rigidbody.velocity, moveForce, Time.deltaTime);

        // 直接移动
        //Rigidbody.velocity = moveForce;
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
    public AudioClip explosionSound;

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
                AudioSource.PlayClipAtPoint(explosionSound, transform.position);
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
        flySoundSource.Stop();
        Destroy(gameObject);
    }

    #endregion

    #region 音效

    [Header("Audio Info")] public AudioSource flySoundSource;
    public AudioClip flySound;

    void AudioUpdate()
    {
        if (!flySoundSource.isPlaying)
        {
            flySoundSource.clip = flySound;
            flySoundSource.loop = true;
            flySoundSource.Play();
        }
    }

    #endregion
}