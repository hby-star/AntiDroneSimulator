using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Drone : Entity
{
    [NonSerialized] public int DroneID;

    [Header("Move Info")] public float moveSpeed = 10f;

    void SettingsAwake()
    {
        flySoundSource.volume *= SettingsManager.Instance.settings.GetComponent<Settings>().volumeSlider.value;
        electricInterferenceTime =
            SettingsManager.Instance.settings.GetComponent<Settings>().empBulletDurationSlider.value;
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
    protected List<Player> otherPlayers = new List<Player>();
    [NonSerialized] public bool FoundPlayer = false;
    public float minDetectSizeInCamera = 0.0005f;
    protected Rect targetRect;
    protected List<Rect> otherTargetRects = new List<Rect>();
    protected bool isPlayerDetectedInCamera = false;
    public float initialCameraFov;

    // To Optimize
    float displayWidth;
    float displayHeight;
    float halfDisplayWidth;
    float halfDisplayHeight;
    float cameraRate;

    float minSizeTimesCameraRate;

    // List<Vector3[]> playerRenderersBounds = new List<Vector3[]>();
    List<Collider> playerRenderers = new List<Collider>();
    List<List<Collider>> otherPlayerRenderers = new List<List<Collider>>();
    Rect cameraRect;
    // Optimize End

    protected void CameraStart()
    {
        Camera.enabled = true;
        initialCameraFov = Camera.fieldOfView;

        // To optimize
        if (!targetPlayer)
        {
            Player[] players = GameObject.FindObjectsOfType<Player>();
            foreach (var player in players)
            {
                if (player.isLeader)
                {
                    targetPlayer = player;
                }
                else
                {
                    otherPlayers.Add(player);
                }
            }
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
            Mathf.Ceil(Mathf.Sqrt(SettingsManager.Instance.settings.GetComponent<Settings>().droneNumSlider.value + 1));
        float droneViewHeight = Screen.height / divide;
        cameraRate = droneViewHeight / displayHeight;
        minSizeTimesCameraRate = minDetectSizeInCamera * cameraRate;
        cameraRect = Camera.rect;
        // optimize end

        UIBoxInit();
    }
    // float lastCameraUpdateTime = 0;
    // float cameraUpdateInterval = 0.1f;

    public void CameraUpdate()
    {
        if (playerRenderers.Count == 0)
        {
            playerRenderers = targetPlayer.playerRenderers;
            for (int i = 0; i < otherPlayers.Count; i++)
            {
                otherPlayerRenderers.Add(otherPlayers[i].playerRenderers);
                otherTargetRects.Add(new Rect(0, 0, 0, 0));
            }
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

        // if (Time.time - lastCameraUpdateTime < cameraUpdateInterval)
        // {
        //     return;
        // }
        //
        // lastCameraUpdateTime = Time.time;
        // cameraUpdateInterval = UnityEngine.Random.Range(0.08f, 0.12f);

        // 初始化 min 和 max 为极大值和极小值
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);

        for (int i = 0; i < playerRenderers.Count; i++)
        {
            Bounds playerBounds = playerRenderers[i].bounds;
            Vector3[] corners = new Vector3[8];
            corners[0] = playerBounds.center +
                         new Vector3(-playerBounds.extents.x, playerBounds.extents.y, -playerBounds.extents.z);
            corners[1] = playerBounds.center +
                         new Vector3(playerBounds.extents.x, playerBounds.extents.y, -playerBounds.extents.z);
            corners[2] = playerBounds.center +
                         new Vector3(-playerBounds.extents.x, playerBounds.extents.y, playerBounds.extents.z);
            corners[3] = playerBounds.center +
                         new Vector3(playerBounds.extents.x, playerBounds.extents.y, playerBounds.extents.z);
            corners[4] = playerBounds.center +
                         new Vector3(-playerBounds.extents.x, -playerBounds.extents.y, -playerBounds.extents.z);
            corners[5] = playerBounds.center +
                         new Vector3(playerBounds.extents.x, -playerBounds.extents.y, -playerBounds.extents.z);
            corners[6] = playerBounds.center +
                         new Vector3(-playerBounds.extents.x, -playerBounds.extents.y, playerBounds.extents.z);
            corners[7] = playerBounds.center +
                         new Vector3(playerBounds.extents.x, -playerBounds.extents.y, playerBounds.extents.z);


            for (int j = 0; j < 8; j++)
            {
                Vector3 screenPoint = Camera.WorldToScreenPoint(corners[j]);
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
        int ignoreRaycastLayer = 1 << LayerMask.NameToLayer("Ignore Raycast");
        ignoreRaycastLayer = ~ignoreRaycastLayer;

        if (Physics.Linecast(Camera.transform.position,
                targetPosition +
                (targetPlayerHeight / 2) * Vector3.up,
                out RaycastHit hit, ignoreRaycastLayer))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                isVisible = false;
            }
        }

        // 计算边界框的位置和大小
        if (isVisible && isInCameraView && (maxY - minY > minSizeTimesCameraRate))
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

        // 计算其他玩家的边界框
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            if (!otherPlayers[i])
            {
                continue;
            }

            // 初始化 min 和 max 为极大值和极小值
            min = new Vector3(float.MaxValue, float.MaxValue, 0);
            max = new Vector3(float.MinValue, float.MinValue, 0);

            for (int j = 0; j < otherPlayerRenderers[i].Count; j++)
            {
                Bounds playerBounds = otherPlayerRenderers[i][j].bounds;
                Vector3[] corners = new Vector3[8];
                corners[0] = playerBounds.center +
                             new Vector3(-playerBounds.extents.x, playerBounds.extents.y, -playerBounds.extents.z);
                corners[1] = playerBounds.center +
                             new Vector3(playerBounds.extents.x, playerBounds.extents.y, -playerBounds.extents.z);
                corners[2] = playerBounds.center +
                             new Vector3(-playerBounds.extents.x, playerBounds.extents.y, playerBounds.extents.z);
                corners[3] = playerBounds.center +
                             new Vector3(playerBounds.extents.x, playerBounds.extents.y, playerBounds.extents.z);
                corners[4] = playerBounds.center +
                             new Vector3(-playerBounds.extents.x, -playerBounds.extents.y, -playerBounds.extents.z);
                corners[5] = playerBounds.center +
                             new Vector3(playerBounds.extents.x, -playerBounds.extents.y, -playerBounds.extents.z);
                corners[6] = playerBounds.center +
                             new Vector3(-playerBounds.extents.x, -playerBounds.extents.y, playerBounds.extents.z);
                corners[7] = playerBounds.center +
                             new Vector3(playerBounds.extents.x, -playerBounds.extents.y, playerBounds.extents.z);

                for (int k = 0; k < 8; k++)
                {
                    Vector3 screenPoint = Camera.WorldToScreenPoint(corners[k]);
                    min.x = Mathf.Min(min.x, screenPoint.x);
                    min.y = Mathf.Min(min.y, screenPoint.y);
                    max.x = Mathf.Max(max.x, screenPoint.x);
                    max.y = Mathf.Max(max.y, screenPoint.y);
                }
            }

            // 将物体的屏幕坐标转换到摄像机的视口空间
            minX = min.x / displayWidth;
            maxX = max.x / displayWidth;
            minY = min.y / displayHeight;
            maxY = max.y / displayHeight;
            centerX = (minX + maxX) / 2;
            centerY = (minY + maxY) / 2;

            // 检查物体是否在当前摄像机的视口内
            isInCameraView = (centerX > cameraRect.x && centerX < cameraRect.x + cameraRect.width &&
                              centerY > cameraRect.y && centerY < cameraRect.y + cameraRect.height);
            // 检查物体是否被遮挡
            isVisible = true;
            if (Physics.Linecast(Camera.transform.position,
                    otherPlayers[i].transform.position +
                    otherPlayers[i].standColliderHeight / 2 * Vector3.up,
                    out RaycastHit _hit))
            {
                if (!_hit.collider.CompareTag("Player"))
                {
                    isVisible = false;
                }
            }

            if (isVisible && isInCameraView && (maxY - minY > minSizeTimesCameraRate || FoundPlayer))
            {
                otherTargetRects[i] = new Rect(min.x, displayHeight - max.y, max.x - min.x, max.y - min.y);
            }
            else
            {
                otherTargetRects[i] = new Rect(0, 0, 0, 0);
            }
        }

        UIBoxUpdate();
    }

    public RectTransform uiBoxPrefab;
    public RectTransform agentUiBoxPrefab;
    private RectTransform uiBox;
    private List<RectTransform> otherUiBoxes = new List<RectTransform>();

    void UIBoxInit()
    {
        uiBox = Instantiate(uiBoxPrefab, CameraManager.Instance.display2.transform);
        uiBox.transform.SetParent(CameraManager.Instance.display2.transform);
        uiBox.gameObject.SetActive(false);
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            RectTransform otherUiBox = Instantiate(agentUiBoxPrefab, CameraManager.Instance.display2.transform);
            otherUiBox.transform.SetParent(CameraManager.Instance.display2.transform);
            otherUiBox.gameObject.SetActive(false);
            otherUiBoxes.Add(otherUiBox);
        }
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

        for (int i = 0; i < otherTargetRects.Count; i++)
        {
            if (otherTargetRects[i].width > 0)
            {
                otherUiBoxes[i].anchoredPosition = new Vector2(
                    otherTargetRects[i].x - halfDisplayWidth + otherTargetRects[i].width / 2,
                    -otherTargetRects[i].y + halfDisplayHeight - otherTargetRects[i].height / 2);
                otherUiBoxes[i].sizeDelta = new Vector2(otherTargetRects[i].width, otherTargetRects[i].height);
                otherUiBoxes[i].gameObject.SetActive(true);
            }
            else
            {
                otherUiBoxes[i].gameObject.SetActive(false);
            }
        }
    }

    # endregion

    # region 避障

    [Header("Avoid Obstacle Info")] private Vector3 obstaclePosition;
    private float obstacleDistance;
    public float avoid_1_distance = 1.5f;
    public float avoid_1_ground_distance = 1f;
    public float avoid_2_distance = 2f;

    public float droneColliderHeight = 1f;
    public float droneColliderSize = 3f;

    private void OnDrawGizmos()
    {
        //避障检测-1
        // Gizmos.color = Color.red;
        // foreach (var direction in directions)
        // {
        //     Gizmos.DrawRay(transform.position, direction.normalized * avoid_1_distance);
        // }

        //避障检测-1-ground
        if (this is AttackDrone)
        {
            Gizmos.color = Color.blue;
        }
        else
        {
            Gizmos.color = Color.green;
        }

        //Gizmos.color = Color.blue;
        foreach (var direction in directions)
        {
            Vector3 modifiedDirection = direction;
            modifiedDirection *= avoid_1_ground_distance;
            Gizmos.DrawRay(transform.position, modifiedDirection);
        }

        // // 避障检测-2
        // Gizmos.color = Color.green;
        // Vector3 boundCenter = transform.position;
        // Vector3 boundSize = new Vector3(droneColliderSize, droneColliderHeight, droneColliderSize) *
        //                     transform.localScale.x;
        // Vector3[] rayPositions =
        // {
        //     boundCenter + new Vector3(boundSize.x / 2, boundSize.y / 2, boundSize.z / 2),
        //     boundCenter + new Vector3(-boundSize.x / 2, boundSize.y / 2, boundSize.z / 2),
        //     boundCenter + new Vector3(boundSize.x / 2, boundSize.y / 2, -boundSize.z / 2),
        //     boundCenter + new Vector3(-boundSize.x / 2, boundSize.y / 2, -boundSize.z / 2),
        //     boundCenter + new Vector3(boundSize.x / 2, -boundSize.y / 2, boundSize.z / 2),
        //     boundCenter + new Vector3(-boundSize.x / 2, -boundSize.y / 2, boundSize.z / 2),
        //     boundCenter + new Vector3(boundSize.x / 2, -boundSize.y / 2, -boundSize.z / 2),
        //     boundCenter + new Vector3(-boundSize.x / 2, -boundSize.y / 2, -boundSize.z / 2),
        // };
        // Vector3 frontDirection = transform.forward.normalized;
        //
        // foreach (var direction in rayDirectionsEuler)
        // {
        //     foreach (var rayPosition in rayPositions)
        //     {
        //         Gizmos.DrawRay(rayPosition, direction * frontDirection * avoid_2_distance);
        //     }
        // }
    }

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

    Quaternion[] rayDirectionsEuler =
    {
        // 正前方
        Quaternion.Euler(0, 0, 0),
        // 左前方-1
        Quaternion.Euler(0, -15, 0),
        // 右前方-1
        Quaternion.Euler(0, 15, 0),
        // 上前方-1
        Quaternion.Euler(-15, 0, 0),
        // 下前方-1
        Quaternion.Euler(15, 0, 0),
        // 左上前方-1
        Quaternion.Euler(-15, -15, 0),
        // 右上前方-1
        Quaternion.Euler(-15, 15, 0),
        // 左下前方-1
        Quaternion.Euler(15, -15, 0),
        // 右下前方-1
        Quaternion.Euler(15, 15, 0),
        // 左前方-2
        Quaternion.Euler(0, -30, 0),
        // 右前方-2
        Quaternion.Euler(0, 30, 0),
        // 上前方-2
        Quaternion.Euler(-30, 0, 0),
        // 下前方-2
        Quaternion.Euler(30, 0, 0),
        // 左上前方-2
        Quaternion.Euler(-30, -30, 0),
        // 右上前方-2
        Quaternion.Euler(-30, 30, 0),
        // 左下前方-2
        Quaternion.Euler(30, -30, 0),
        // 右下前方-2
        Quaternion.Euler(30, 30, 0),
        // 左前方-3
        Quaternion.Euler(0, -45, 0),
        // 右前方-3
        Quaternion.Euler(0, 45, 0),
        // 上前方-3
        Quaternion.Euler(-45, 0, 0),
        // 下前方-3
        Quaternion.Euler(45, 0, 0),
        // 左上前方-3
        Quaternion.Euler(-45, -45, 0),
        // 右上前方-3
        Quaternion.Euler(-45, 45, 0),
        // 左下前方-3
        Quaternion.Euler(45, -45, 0),
        // 右下前方-3
        Quaternion.Euler(45, 45, 0),
        // 左前方-4
        Quaternion.Euler(0, -60, 0),
        // 右前方-4
        Quaternion.Euler(0, 60, 0),
        // 上前方-4
        Quaternion.Euler(-60, 0, 0),
        // 下前方-4
        Quaternion.Euler(60, 0, 0),
        // 左上前方-4
        Quaternion.Euler(-60, -60, 0),
        // 右上前方-4
        Quaternion.Euler(-60, 60, 0),
        // 左下前方-4
        Quaternion.Euler(60, -60, 0),
        // 右下前方-4
        Quaternion.Euler(60, 60, 0),
        // 左前方-5
        Quaternion.Euler(0, -75, 0),
        // 右前方-5
        Quaternion.Euler(0, 75, 0),
        // 上前方-5
        Quaternion.Euler(-75, 0, 0),
        // 下前方-5
        Quaternion.Euler(75, 0, 0),
        // 左上前方-5
        Quaternion.Euler(-75, -75, 0),
        // 右上前方-5
        Quaternion.Euler(-75, 75, 0),
        // 左下前方-5
        Quaternion.Euler(75, -75, 0),
        // 右下前方-5
        Quaternion.Euler(75, 75, 0),
        // 左方
        Quaternion.Euler(0, -90, 0),
        // 右方
        Quaternion.Euler(0, 90, 0),
        // 上方
        Quaternion.Euler(-90, 0, 0),
        // 下方
        Quaternion.Euler(90, 0, 0),
        // 左后方
        Quaternion.Euler(0, -135, 0),
        // 右后方
        Quaternion.Euler(0, 135, 0),
        // 上后方
        Quaternion.Euler(-135, 0, 0),
        // 下后方
        Quaternion.Euler(135, 0, 0),
    };
    // Optimize End

    protected void AvoidObstacleUpdate()
    {
        #region 避障检测-1-无人机

        avoidObstacleForce = Vector3.zero;
        foreach (var direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction.normalized, out hit, avoid_1_distance))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.CompareTag("Bomb"))
                {
                    continue;
                }

                if (hit.collider.CompareTag("Drone"))
                {
                    avoidObstacleForce += (transform.position - hit.point).normalized;
                }
            }
        }

        #endregion

        #region 避障检测-1-地面

        foreach (var direction in directions)
        {
            RaycastHit hit;
            Vector3 modifiedDirection = direction;
            modifiedDirection *= avoid_1_ground_distance;
            if (Physics.Raycast(transform.position, modifiedDirection, out hit, modifiedDirection.magnitude))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.CompareTag("Bomb"))
                {
                    continue;
                }

                if (hit.collider.CompareTag("Ground"))
                {
                    avoidObstacleForce += (transform.position - hit.point).normalized;
                }
            }
        }

        #endregion

        #region 避障检测-2-地面

        if (taskForce == Vector3.zero)
        {
            return;
        }

        Vector3[] rayPositions =
        {
            Collider.bounds.center + new Vector3(Collider.bounds.size.x / 2, Collider.bounds.size.y / 2,
                Collider.bounds.size.z / 2),
            Collider.bounds.center + new Vector3(-Collider.bounds.size.x / 2, Collider.bounds.size.y / 2,
                Collider.bounds.size.z / 2),
            Collider.bounds.center + new Vector3(Collider.bounds.size.x / 2, Collider.bounds.size.y / 2,
                -Collider.bounds.size.z / 2),
            Collider.bounds.center + new Vector3(-Collider.bounds.size.x / 2, Collider.bounds.size.y / 2,
                -Collider.bounds.size.z / 2),
            Collider.bounds.center + new Vector3(Collider.bounds.size.x / 2, -Collider.bounds.size.y / 2,
                Collider.bounds.size.z / 2),
            Collider.bounds.center + new Vector3(-Collider.bounds.size.x / 2, -Collider.bounds.size.y / 2,
                Collider.bounds.size.z / 2),
            Collider.bounds.center + new Vector3(Collider.bounds.size.x / 2, -Collider.bounds.size.y / 2,
                -Collider.bounds.size.z / 2),
            Collider.bounds.center + new Vector3(-Collider.bounds.size.x / 2, -Collider.bounds.size.y / 2,
                -Collider.bounds.size.z / 2),
        };
        Vector3 frontDirection = taskForce.normalized;

        foreach (var direction in rayDirectionsEuler)
        {
            bool canMove = true;
            foreach (var rayPosition in rayPositions)
            {
                RaycastHit hit;
                if (Physics.Raycast(rayPosition, direction * frontDirection, out hit, avoid_2_distance))
                {
                    if (hit.collider.CompareTag("Ground"))
                    {
                        canMove = false;
                    }
                }
            }

            if (canMove)
            {
                taskForce = direction * frontDirection;
                return;
            }
        }

        taskForce = Quaternion.Euler(0, 180, 0) * frontDirection;

        #endregion
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
        for (int i = 0; i < otherUiBoxes.Count; i++)
        {
            otherUiBoxes[i].gameObject.SetActive(false);
        }

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