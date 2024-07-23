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
        if (capsuleCollider != null)
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

    [Header("Attack Info")] public int maxBullets = 6;
    public int bullets = 6;
    public GameObject bulletImpact;

    public bool CanAttack()
    {
        return bullets > 0;
    }

    private void AttackStart()
    {
        playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
    {
        if (operateNow && InputManager.Instance.operateEntityNow)
        {
            int crosshairSize = 12;
            int lineLength = 5;
            int lineWidth = 2;
            float posX = playerCamera.pixelWidth / 2 - crosshairSize / 4;
            float posY = playerCamera.pixelHeight / 2 - crosshairSize / 2;

            // Create a 1x1 texture for lines
            Texture2D lineTexture = new Texture2D(1, 1);
            lineTexture.SetPixel(0, 0, Color.white);
            lineTexture.Apply();

            // Horizontal line
            GUI.DrawTexture(
                new Rect(playerCamera.pixelWidth / 2 - lineLength / 2, playerCamera.pixelHeight / 2 - lineWidth / 2,
                    lineLength,
                    lineWidth), lineTexture);
            // Vertical line
            GUI.DrawTexture(
                new Rect(playerCamera.pixelWidth / 2 - lineWidth / 2, playerCamera.pixelHeight / 2 - lineLength / 2,
                    lineWidth,
                    lineLength), lineTexture);
            // Left line
            GUI.DrawTexture(
                new Rect(playerCamera.pixelWidth / 2 - lineLength - crosshairSize / 2,
                    playerCamera.pixelHeight / 2 - lineWidth / 2,
                    lineLength, lineWidth), lineTexture);
            // Right line
            GUI.DrawTexture(
                new Rect(playerCamera.pixelWidth / 2 + crosshairSize / 2, playerCamera.pixelHeight / 2 - lineWidth / 2,
                    lineLength,
                    lineWidth), lineTexture);
            // Top line
            GUI.DrawTexture(
                new Rect(playerCamera.pixelWidth / 2 - lineWidth / 2,
                    playerCamera.pixelHeight / 2 - lineLength - crosshairSize / 2,
                    lineWidth, lineLength), lineTexture);
            // Bottom line
            GUI.DrawTexture(
                new Rect(playerCamera.pixelWidth / 2 - lineWidth / 2, playerCamera.pixelHeight / 2 + crosshairSize / 2,
                    lineWidth,
                    lineLength), lineTexture);
        }
    }

    public void Attack()
    {
        soundSource.PlayOneShot(fireSound);
        Vector3 point = new Vector3(playerCamera.pixelWidth / 2, playerCamera.pixelHeight / 2, 0);
        Ray ray = playerCamera.ScreenPointToRay(point);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.transform.gameObject;
            ReactiveTarget target = hitObject.GetComponent<ReactiveTarget>();
            if (target != null)
            {
                target.ReactToHit();
            }
            else
            {
                if (hitObject.tag == "Ground")
                {
                    StartCoroutine(AttackBulletImpact(hit.point, hit.normal));
                }
            }
        }
    }

    private IEnumerator AttackBulletImpact(Vector3 pos, Vector3 normal)
    {
        // Calculate the rotation so that the prefab's Y-axis points in the direction of the normal
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

        // Instantiate the bullet impact prefab with the adjusted rotation
        GameObject impactEffect = Instantiate(bulletImpact, pos, rotation);

        // Optional: Adjust if the effect should disappear after some time
        yield return new WaitForSeconds(1);

        Destroy(impactEffect);
    }

    #endregion

    #region Control

    public void SetOperate(bool operate)
    {
        operateNow = operate;
        playerCamera.gameObject.SetActive(operate);
        transform.rotation = Quaternion.identity;
        if (!operate)
        {
            StateMachine.ChangeState(IdleState);
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
        bullets = 6;

        AttackStart();

        SetOperate(InputManager.Instance.operatePlayerNow);
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

    public void OnDrawGizmos()
    {
        CapsuleCollider capsuleCollider = Collider as CapsuleCollider;
        if (capsuleCollider == null) return;

        Vector3 capsuleBottom = new Vector3(transform.position.x, transform.position.y + capsuleCollider.radius,
            transform.position.z);
        Vector3 capsuleTop = new Vector3(transform.position.x,
            transform.position.y + capsuleCollider.height - capsuleCollider.radius, transform.position.z);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(capsuleTop, capsuleCollider.radius);
        Gizmos.DrawWireSphere(capsuleBottom, capsuleCollider.radius);
    }

    #region MouseLook

    [Header("Mouse Look Info")] public float sensitivityHor = 9.0f;
    public float sensitivityVert = 9.0f;

    public float minimumVert = -45.0f;
    public float maximumVert = 45.0f;

    public Transform targetX;
    public Transform targetY;

    private float verticalRot = 0;
    public Camera playerCamera { get; private set; }

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

    #region isBusy

    public bool IsBusy { get; private set; }

    public IEnumerator BusyFor(float seconds)
    {
        IsBusy = true;

        yield return new WaitForSeconds(seconds);

        IsBusy = false;
    }

    #endregion

    #region Audio

    [Header("Audio Info")] [SerializeField]
    public AudioSource soundSource;

    [SerializeField] public AudioClip reloadSound;
    [SerializeField] public AudioClip fireSound;
    [SerializeField] public AudioClip dashSound;

    #endregion

    #region Handle Input

    public float HorizontalInput { get; private set; }
    public float VerticalInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool DashInput { get; private set; }
    public bool CrouchInput { get; private set; }
    public bool AttackInput { get; private set; }
    public float CameraHorizontalInput { get; private set; }
    public float CameraVerticalInput { get; private set; }

    void OnEnable()
    {
        Messenger<float>.AddListener(InputEvent.PLAYER_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.PLAYER_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_JUMP_INPUT, (value) => { JumpInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_DASH_INPUT, (value) => { DashInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_CROUCH_INPUT, (value) => { CrouchInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_ATTACK_INPUT, (value) => { AttackInput = value; });
        Messenger<float>.AddListener(InputEvent.CAMERA_HORIZONTAL_INPUT,
            (value) => { CameraHorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.CAMERA_VERTICAL_INPUT, (value) => { CameraVerticalInput = value; });
    }

    void OnDisable()
    {
        Messenger<float>.RemoveListener(InputEvent.PLAYER_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.PLAYER_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_JUMP_INPUT, (value) => { JumpInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_DASH_INPUT, (value) => { DashInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_CROUCH_INPUT, (value) => { CrouchInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_ATTACK_INPUT, (value) => { AttackInput = value; });
        Messenger<float>.RemoveListener(InputEvent.CAMERA_HORIZONTAL_INPUT,
            (value) => { CameraHorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.CAMERA_VERTICAL_INPUT, (value) => { CameraVerticalInput = value; });
    }

    #endregion
}