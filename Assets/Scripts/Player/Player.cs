using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : Entity
{
    // To Optimize
    public List<Vector3[]> playerRenderersBounds = new List<Vector3[]>();
    // Optimize End

    # region States

    public EntityStateMachine StateMachine;
    public PlayerMoveState MoveState;
    public PlayerIdleState IdleState;
    public PlayerAirState AirState;
    public PlayerJumpState JumpState;
    public PlayerAttackState AttackState;
    public PlayerReloadState ReloadState;
    public PlayerDashState DashState;
    public PlayerCrouchState CrouchState;

    # endregion

    #region Stats

    [Header("Stats Info")] public EntityStats playerStats;
    public List<Collider> playerRenderers;

    public void TakeDamage(float damage)
    {
        if (playerStats.currentHeath > damage)
        {
            playerStats.TakeDamage(damage);
        }
        else
        {
            if (isLeader)
            {
                GameManager.Instance.GameFail();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    #endregion

    #region Move

    [Header("Move Info")] public float moveSpeed = 10f;
    public float jumpForce = 10f;
    public float dashSpeed = 10f;
    public float standColliderHeight = 1.75f;
    public float crouchColliderHeight = 1.0f;

    public void Jump()
    {
        Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void SetColliderHeight(float height)
    {
        CapsuleCollider capsuleCollider = Collider as CapsuleCollider;
        if (capsuleCollider)
        {
            float originalHeight = capsuleCollider.height;
            Vector3 center = capsuleCollider.center;
            capsuleCollider.height = height;
            capsuleCollider.center = new Vector3(center.x, (height - 0.05f) / 2, center.z);
        }
    }

    public bool IsGrounded()
    {
        CapsuleCollider capsuleCollider = Collider as CapsuleCollider;
        if (capsuleCollider == null) return false;

        Vector3 capsuleBottom = new Vector3(transform.position.x, transform.position.y + capsuleCollider.radius,
            transform.position.z);
        Vector3 capsuleTop = new Vector3(transform.position.x,
            transform.position.y + capsuleCollider.height - capsuleCollider.radius, transform.position.z);

        float distanceToGround = capsuleCollider.height / 2 - capsuleCollider.radius + 0.1f;
        return Physics.CapsuleCast(capsuleTop, capsuleBottom, capsuleCollider.radius, Vector3.down, distanceToGround);
    }

    #endregion

    #region Attack

    [Header("Attack Info")] public List<Gun> guns;
    public Action onGunChanged;
    public GameObject shieldPrefab;
    private bool isShiledPlaced;
    public Equipment currentEquipment { get; private set; }
    private int currentGunIndex;

    private void AttackStart()
    {
        currentGunIndex = 0;
        SetGun(currentGunIndex);

        isShiledPlaced = false;

        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    private void SetGun(int index)
    {
        for (int i = 0; i < guns.Count; i++)
        {
            guns[i].playerCamera = Camera;
            guns[i].gameObject.SetActive(i == index);
        }

        currentEquipment = guns[index];
    }

    public void ChangeGun()
    {
        currentGunIndex++;
        if (currentGunIndex >= guns.Count)
        {
            currentGunIndex = 0;
        }

        SetGun(currentGunIndex);
        onGunChanged?.Invoke();
    }

    public bool CanAttack()
    {
        if (currentEquipment is Gun gun)
        {
            return gun.CanFire();
        }

        return false;
    }

    public void Attack()
    {
        if (currentEquipment is Gun gun)
        {
            gun.Fire();
        }
    }

    public bool CanReload()
    {
        if (currentEquipment is Gun gun)
        {
            return gun.CanReload();
        }

        return false;
    }

    public void ReloadStart()
    {
        if (currentEquipment is Gun gun)
        {
            gun.ReloadStart();
        }
    }

    public void ReloadEnd()
    {
        if (currentEquipment is Gun gun)
        {
            gun.ReloadEnd();
        }
    }

    #endregion

    #region Control

    [Header("Control Info")] public float interactRange = 1.5f;

    public override void SetOperate(bool operate)
    {
        base.SetOperate(operate);

        if (!operate)
        {
            StateMachine.ChangeState(IdleState);
        }
    }

    public override void InteractUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactRange);

        if (PlayerEnterVehicleInput)
        {
            foreach (var collider in colliders)
            {
                Vehicle vehicle = collider.GetComponent<Vehicle>();
                if (vehicle)
                {
                    transform.parent = vehicle.transform;
                    gameObject.SetActive(false);
                    vehicle.currentPlayer = this;
                    vehicle.StartCoroutine(vehicle.BusyFor(1f));
                    InputManager.Instance.ChangeOperateEntity(vehicle);
                    break;
                }
            }
        }

        if (PlayerUseVehicleEmpInput)
        {
            foreach (var collider in colliders)
            {
                Vehicle vehicle = collider.GetComponent<Vehicle>();
                if (vehicle)
                {
                    vehicle.EmpAttack();
                    break;
                }
            }
        }

        if (PlayerUseVehicleRadarInput)
        {
            foreach (var collider in colliders)
            {
                Vehicle vehicle = collider.GetComponent<Vehicle>();
                if (vehicle)
                {
                    List<Vector3> positions = new List<Vector3>();
                    positions = vehicle.RadarDetection();
                    foreach (var position in positions)
                    {
                        Debug.Log("Radar Detection: " + position);
                    }

                    break;
                }
            }
        }

        if (PlayerUseVehicleElectromagneticInterferenceInput)
        {
            foreach (var collider in colliders)
            {
                Vehicle vehicle = collider.GetComponent<Vehicle>();
                if (vehicle)
                {
                    vehicle.ElectromagneticInterferenceAttack();
                    break;
                }
            }
        }

        if (PlayerPlacePickupShieldInput)
        {
            if (!isShiledPlaced)
            {
                Vector3 shieldPosition = transform.position + transform.forward * 2;
                shieldPosition.y = transform.position.y;
                GameObject newShield = Instantiate(shieldPrefab, shieldPosition, Quaternion.identity);
                newShield.transform.forward = transform.forward;
                isShiledPlaced = true;
            }
            else
            {
                foreach (var collider in colliders)
                {
                    Shield shield = collider.GetComponentInParent<Shield>();
                    if (shield)
                    {
                        Destroy(shield.gameObject);
                        isShiledPlaced = false;
                        break;
                    }
                }
            }
        }
    }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        SettingsAwake();

        StateMachine = new EntityStateMachine();

        MoveState = new PlayerMoveState(StateMachine, this, "Move", this);
        IdleState = new PlayerIdleState(StateMachine, this, "Idle", this);
        AirState = new PlayerAirState(StateMachine, this, "Air", this);
        JumpState = new PlayerJumpState(StateMachine, this, "Jump", this);
        AttackState = new PlayerAttackState(StateMachine, this, "Attack", this);
        ReloadState = new PlayerReloadState(StateMachine, this, "Reload", this);
        DashState = new PlayerDashState(StateMachine, this, "Dash", this);
        CrouchState = new PlayerCrouchState(StateMachine, this, "Crouch", this);

        // To Optimize
        foreach (var renderer in playerRenderers)
        {
            Vector3[] corners = new Vector3[8];
            Bounds bounds = renderer.bounds;
            corners[0] = new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
            corners[1] = new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
            corners[2] = new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);
            corners[3] = new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            corners[4] = new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
            corners[5] = new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
            corners[6] = new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);
            corners[7] = new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
            playerRenderersBounds.Add(corners);
        }
        // Optimize End
    }

    private void SettingsAwake()
    {
        if (UIManager.Instance)
        {
            sensitivityHor = sensitivityVert =
                SettingsManager.Instance.settings.GetComponent<Settings>().sensitivitySlider.value;
            soundSource.volume *= SettingsManager.Instance.settings.GetComponent<Settings>().volumeSlider.value;
            playerStats.maxHeath = SettingsManager.Instance.settings.GetComponent<Settings>().playerHeathSlider.value;
            playerStats.currentHeath = playerStats.maxHeath;
            moveSpeed = SettingsManager.Instance.settings.GetComponent<Settings>().playerMoveSpeedSlider.value;
        }
    }

    protected override void Start()
    {
        base.Start();

        StateMachine.Initialize(IdleState);

        AttackStart();

        SetOperate(InputManager.Instance.currentEntity is Player && isLeader);
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();

        MouseLookUpdate();
    }


    public void AnimationFinished()
    {
        StateMachine.CurrentState.AnimationFinished();
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the capsule collider
        CapsuleCollider capsuleCollider = Collider as CapsuleCollider;
        if (capsuleCollider)
        {
            Gizmos.color = Color.red;
            Vector3 point1 = new Vector3(transform.position.x, transform.position.y + capsuleCollider.radius,
                transform.position.z);
            Vector3 point2 = new Vector3(transform.position.x,
                transform.position.y + capsuleCollider.height - capsuleCollider.radius,
                transform.position.z);
            Gizmos.DrawWireSphere(point1, capsuleCollider.radius);
            Gizmos.DrawWireSphere(point2, capsuleCollider.radius);
            Gizmos.DrawLine(point1 + Vector3.right * capsuleCollider.radius,
                point2 + Vector3.right * capsuleCollider.radius);
            Gizmos.DrawLine(point2 - Vector3.right * capsuleCollider.radius,
                point2 - Vector3.right * capsuleCollider.radius);
        }

        // Draw the interaction sphere
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    #region MouseLook

    [Header("Mouse Look Info")] public float sensitivityHor = 9.0f;
    public float sensitivityVert = 9.0f;

    public float minimumVert = -45.0f;
    public float maximumVert = 45.0f;

    public Transform targetX;
    public Transform targetY;

    private float verticalRot = 0;

    void MouseLookUpdate()
    {
        if (operateNow && !GameManager.Instance.IsGamePaused)
        {
            MouseXLookUpdate();
            MouseYLookUpdate();
        }
    }

    void MouseXLookUpdate()
    {
        float rotationY = CameraHorizontalInput * sensitivityHor;
        targetX.Rotate(0, rotationY, 0);
    }

    void MouseYLookUpdate()
    {
        verticalRot -= CameraVerticalInput * sensitivityVert;
        verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);
        targetY.localEulerAngles = new Vector3(verticalRot, targetY.localEulerAngles.y, 0);
    }

    #endregion

    #region Audio

    [Header("Audio Info")] [SerializeField]
    public AudioSource soundSource;

    [SerializeField] public AudioClip dashSound;

    #endregion

    #region Handle Input

    [Header("Input Info")] public bool isLeader;

    [NonSerialized] public float HorizontalInput;
    [NonSerialized] public float VerticalInput;
    [NonSerialized] public bool JumpInput;
    [NonSerialized] public bool DashInput;
    [NonSerialized] public bool CrouchInput;
    [NonSerialized] public bool AttackInput;
    [NonSerialized] public bool ReloadInput;
    [NonSerialized] public bool ChangeGunInput;
    [NonSerialized] public float CameraHorizontalInput;
    [NonSerialized] public float CameraVerticalInput;

    // Interact with vehicle
    [NonSerialized] public bool PlayerEnterVehicleInput;
    [NonSerialized] public bool PlayerUseVehicleEmpInput;
    [NonSerialized] public bool PlayerUseVehicleRadarInput;
    [NonSerialized] public bool PlayerUseVehicleElectromagneticInterferenceInput;
    [NonSerialized] public bool PlayerPlacePickupShieldInput;

    public void ZeroInput()
    {
        HorizontalInput = 0;
        VerticalInput = 0;
        JumpInput = false;
        DashInput = false;
        CrouchInput = false;
        AttackInput = false;
        ReloadInput = false;
        ChangeGunInput = false;
        CameraHorizontalInput = 0;
        PlayerEnterVehicleInput = false;
        PlayerUseVehicleEmpInput = false;
        PlayerUseVehicleRadarInput = false;
        PlayerUseVehicleElectromagneticInterferenceInput = false;
        PlayerPlacePickupShieldInput = false;
    }

    void OnEnable()
    {
        if (!isLeader)
        {
            return;
        }

        Messenger<float>.AddListener(InputEvent.PLAYER_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.PLAYER_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_JUMP_INPUT, (value) => { JumpInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_DASH_INPUT, (value) => { DashInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_CROUCH_INPUT, (value) => { CrouchInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_ATTACK_INPUT, (value) => { AttackInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_RELOAD_INPUT, (value) => { ReloadInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_CHANGE_GUN_INPUT, (value) => { ChangeGunInput = value; });
        Messenger<float>.AddListener(InputEvent.PLAYER_CAMERA_HORIZONTAL_INPUT,
            (value) => { CameraHorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.PLAYER_CAMERA_VERTICAL_INPUT,
            (value) => { CameraVerticalInput = value; });

        // Interact with vehicle
        Messenger<bool>.AddListener(InputEvent.VEHICLE_ENTER_EXIT_INPUT,
            (value) => { PlayerEnterVehicleInput = value; });
        Messenger<bool>.AddListener(InputEvent.VECHILE_EMP_USE_INPUT, (value) => { PlayerUseVehicleEmpInput = value; });
        Messenger<bool>.AddListener(InputEvent.VECHILE_RADAR_SWITCH_INPUT,
            (value) => { PlayerUseVehicleRadarInput = value; });
        Messenger<bool>.AddListener(InputEvent.VECHILE_ELERTIC_INTERFERENCE_INPUT,
            (value) => { PlayerUseVehicleElectromagneticInterferenceInput = value; });

        // Interact with shield
        Messenger<bool>.AddListener(InputEvent.PLAYER_PLACE_PICKUP_SHIELD_INPUT,
            (value) => { PlayerPlacePickupShieldInput = value; });
    }

    void OnDisable()
    {
        if (!isLeader)
        {
            return;
        }

        Messenger<float>.RemoveListener(InputEvent.PLAYER_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.PLAYER_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_JUMP_INPUT, (value) => { JumpInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_DASH_INPUT, (value) => { DashInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_CROUCH_INPUT, (value) => { CrouchInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_ATTACK_INPUT, (value) => { AttackInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_RELOAD_INPUT, (value) => { ReloadInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_CHANGE_GUN_INPUT, (value) => { ChangeGunInput = value; });
        Messenger<float>.RemoveListener(InputEvent.PLAYER_CAMERA_HORIZONTAL_INPUT,
            (value) => { CameraHorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.PLAYER_CAMERA_VERTICAL_INPUT,
            (value) => { CameraVerticalInput = value; });

        // Interact with vehicle
        Messenger<bool>.RemoveListener(InputEvent.VEHICLE_ENTER_EXIT_INPUT,
            (value) => { PlayerEnterVehicleInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.VECHILE_EMP_USE_INPUT,
            (value) => { PlayerUseVehicleEmpInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.VECHILE_RADAR_SWITCH_INPUT,
            (value) => { PlayerUseVehicleRadarInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.VECHILE_ELERTIC_INTERFERENCE_INPUT,
            (value) => { PlayerUseVehicleElectromagneticInterferenceInput = value; });

        // Interact with shield
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_PLACE_PICKUP_SHIELD_INPUT,
            (value) => { PlayerPlacePickupShieldInput = value; });
    }

    #endregion
}