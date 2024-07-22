using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : Entity
{
    # region Player States

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

    public float jumpForce = 10f;
    public int maxBullets = 6;
    public int bullets = 6;
    public float dashSpeed = 10f;
    public float standColliderHeight = 1.75f;
    public float crouchColliderHeight = 1.0f;

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

        RayShooterStart();

        SetOperate(false);
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();
    }

    public void Jump()
    {
        Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void AnimationFinished()
    {
        StateMachine.CurrentState.AnimationFinished();
    }

    public bool CanAttack()
    {
        return bullets > 0;
    }

    public void TakeDamage(float damage)
    {
        EntityStats.TakeDamage(damage);
    }

    public void SetOperate(bool operate)
    {
        operateNow = operate;
        _camera.gameObject.SetActive(operate);
        mouseLookX.enabled = operate;
        mouseLookY.enabled = operate;
        transform.rotation = Quaternion.identity;
        if (!operate)
        {
            StateMachine.ChangeState(IdleState);
        }
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

    [SerializeField] public AudioSource soundSource;
    [SerializeField] public AudioClip reloadSound;
    [SerializeField] public AudioClip fireSound;
    [SerializeField] public AudioClip dashSound;

    #endregion

    #region RayShooter

    [Header("Ray Shooter")] [SerializeField]
    GameObject bulletImpact;

    private Camera _camera;

    private void RayShooterStart()
    {
        _camera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
    {
        if (operateNow)
        {
            int crosshairSize = 12;
            int lineLength = 5;
            int lineWidth = 2;
            float posX = _camera.pixelWidth / 2 - crosshairSize / 4;
            float posY = _camera.pixelHeight / 2 - crosshairSize / 2;

            // Create a 1x1 texture for lines
            Texture2D lineTexture = new Texture2D(1, 1);
            lineTexture.SetPixel(0, 0, Color.white);
            lineTexture.Apply();

            // Horizontal line
            GUI.DrawTexture(
                new Rect(_camera.pixelWidth / 2 - lineLength / 2, _camera.pixelHeight / 2 - lineWidth / 2, lineLength,
                    lineWidth), lineTexture);
            // Vertical line
            GUI.DrawTexture(
                new Rect(_camera.pixelWidth / 2 - lineWidth / 2, _camera.pixelHeight / 2 - lineLength / 2, lineWidth,
                    lineLength), lineTexture);
            // Left line
            GUI.DrawTexture(
                new Rect(_camera.pixelWidth / 2 - lineLength - crosshairSize / 2,
                    _camera.pixelHeight / 2 - lineWidth / 2,
                    lineLength, lineWidth), lineTexture);
            // Right line
            GUI.DrawTexture(
                new Rect(_camera.pixelWidth / 2 + crosshairSize / 2, _camera.pixelHeight / 2 - lineWidth / 2,
                    lineLength,
                    lineWidth), lineTexture);
            // Top line
            GUI.DrawTexture(
                new Rect(_camera.pixelWidth / 2 - lineWidth / 2,
                    _camera.pixelHeight / 2 - lineLength - crosshairSize / 2,
                    lineWidth, lineLength), lineTexture);
            // Bottom line
            GUI.DrawTexture(
                new Rect(_camera.pixelWidth / 2 - lineWidth / 2, _camera.pixelHeight / 2 + crosshairSize / 2, lineWidth,
                    lineLength), lineTexture);
        }
    }

    public void RayShooterFire()
    {
        soundSource.PlayOneShot(fireSound);
        Vector3 point = new Vector3(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 0);
        Ray ray = _camera.ScreenPointToRay(point);
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
                    StartCoroutine(RayShooterBulletImpact(hit.point, hit.normal));
                }
            }
        }
    }

    private IEnumerator RayShooterBulletImpact(Vector3 pos, Vector3 normal)
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

    #region Handle Input

    public float HorizontalInput;
    public float VerticalInput;
    public bool JumpInput;
    public bool DashInput;
    public bool CrouchInput;
    public bool AttackInput;

    public MouseLook mouseLookX;
    public MouseLook mouseLookY;

    void OnEnable()
    {
        Messenger<float>.AddListener(InputEvent.PLAYER_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.AddListener(InputEvent.PLAYER_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_JUMP_INPUT, (value) => { JumpInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_DASH_INPUT, (value) => { DashInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_CROUCH_INPUT, (value) => { CrouchInput = value; });
        Messenger<bool>.AddListener(InputEvent.PLAYER_ATTACK_INPUT, (value) => { AttackInput = value; });
    }

    void OnDisable()
    {
        Messenger<float>.RemoveListener(InputEvent.PLAYER_HORIZONTAL_INPUT, (value) => { HorizontalInput = value; });
        Messenger<float>.RemoveListener(InputEvent.PLAYER_VERTICAL_INPUT, (value) => { VerticalInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_JUMP_INPUT, (value) => { JumpInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_DASH_INPUT, (value) => { DashInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_CROUCH_INPUT, (value) => { CrouchInput = value; });
        Messenger<bool>.RemoveListener(InputEvent.PLAYER_ATTACK_INPUT, (value) => { AttackInput = value; });
    }
    #endregion

}