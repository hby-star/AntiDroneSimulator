using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : Entity
{
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

    #region Move

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

    [Header("Attack Info")]
    public List<Gun> guns;
    public GameObject shieldPrefab;
    private bool isShiledPlaced;
    private Equipment currentEquipment;
    private int currentGunIndex;

    private void AttackStart()
    {
        currentGunIndex = 0;
        SetGun(currentGunIndex);

        isShiledPlaced = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

    public void Reload()
    {
        if (currentEquipment is Gun gun)
        {
            gun.Reload();
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
                Vector3 shieldPosition = transform.position + transform.forward * 3;
                shieldPosition.y = transform.position.y + 0.75f;
                GameObject newShield = Instantiate(shieldPrefab, shieldPosition, Quaternion.identity);
                newShield.transform.forward = transform.forward;
                isShiledPlaced = true;
            }
            else
            {
                foreach (var collider in colliders)
                {
                    Shield shield = collider.GetComponent<Shield>();
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

        StateMachine = new EntityStateMachine();

        MoveState = new PlayerMoveState(StateMachine, this, "Move", this);
        IdleState = new PlayerIdleState(StateMachine, this, "Idle", this);
        AirState = new PlayerAirState(StateMachine, this, "Air", this);
        JumpState = new PlayerJumpState(StateMachine, this, "Jump", this);
        AttackState = new PlayerAttackState(StateMachine, this, "Attack", this);
        ReloadState = new PlayerReloadState(StateMachine, this, "Reload", this);
        DashState = new PlayerDashState(StateMachine, this, "Dash", this);
        CrouchState = new PlayerCrouchState(StateMachine, this, "Crouch", this);
    }

    protected override void Start()
    {
        base.Start();

        StateMachine.Initialize(IdleState);

        AttackStart();

        SetOperate(InputManager.Instance.currentEntity is Player);

        Rigidbody.freezeRotation = true;
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
            Vector3 point1 = new Vector3(transform.position.x, transform.position.y + capsuleCollider.height / 2,
                transform.position.z);
            Vector3 point2 = new Vector3(transform.position.x, transform.position.y - capsuleCollider.height / 2,
                transform.position.z);
            Gizmos.DrawWireSphere(point1, capsuleCollider.radius);
            Gizmos.DrawWireSphere(point2, capsuleCollider.radius);
            Gizmos.DrawLine(point1 + Vector3.right * capsuleCollider.radius, point2 + Vector3.right * capsuleCollider.radius);
            Gizmos.DrawLine(point1 - Vector3.right * capsuleCollider.radius, point2 - Vector3.right * capsuleCollider.radius);
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
        if (operateNow)
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

    public float HorizontalInput { get; private set; }
    public float VerticalInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool DashInput { get; private set; }
    public bool CrouchInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool ReloadInput { get; private set; }
    public bool ChangeGunInput { get; private set; }
    public float CameraHorizontalInput { get; private set; }
    public float CameraVerticalInput { get; private set; }

    // Interact with vehicle
    public bool PlayerEnterVehicleInput { get; private set; }
    public bool PlayerUseVehicleEmpInput { get; private set; }
    public bool PlayerUseVehicleRadarInput { get; private set; }
    public bool PlayerUseVehicleElectromagneticInterferenceInput { get; private set; }
    public bool PlayerPlacePickupShieldInput { get; private set; }

    void OnEnable()
    {
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
        Messenger<bool>.RemoveListener(InputEvent.VECHILE_EMP_USE_INPUT, (value) => { PlayerUseVehicleEmpInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.VECHILE_RADAR_SWITCH_INPUT,
            (value) => { PlayerUseVehicleRadarInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.VECHILE_ELERTIC_INTERFERENCE_INPUT ,
            (value) => { PlayerUseVehicleElectromagneticInterferenceInput = value; });

        // Interact with shield
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_PLACE_PICKUP_SHIELD_INPUT,
            (value) => { PlayerPlacePickupShieldInput = value; });
    }

    #endregion
}