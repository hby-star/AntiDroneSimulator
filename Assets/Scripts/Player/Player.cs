using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    # endregion

    public float jumpForce = 10f;
    public int maxBullets = 6;
    public int bullets = 6;


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
    }

    protected override void Start()
    {
        base.Start();

        StateMachine.Initialize(IdleState);
        bullets = 6;

        RayShooterStart();
    }

    protected override void Update()
    {
        base.Update();

        StateMachine.CurrentState.Update();
    }

    public void Move(float moveSpeed)
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X");

        // Calculate movement based on vertical input & horizontal input
        Vector3 moveDirectionX = transform.forward * (verticalInput * moveSpeed);
        Vector3 moveDirectionZ = transform.right * (horizontalInput * moveSpeed);
        Vector3 moveDirection = moveDirectionX + moveDirectionZ;
        moveDirection = Vector3.ClampMagnitude(moveDirection, moveSpeed);
        Rigidbody.velocity = new Vector3(moveDirection.x, Rigidbody.velocity.y, moveDirection.z);
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

    #region isBusy

    public bool IsBusy { get; private set; }

    public IEnumerator BusyFor(float seconds)
    {
        IsBusy = true;

        yield return new WaitForSeconds(seconds);

        IsBusy = false;
    }

    #endregion

    #region RayShooter

    [Header("Ray Shooter")] [SerializeField]
    AudioSource soundSource;

    [SerializeField] AudioClip fireSound;
    [SerializeField] GameObject bulletImpact;
    private Camera _camera;

    private void RayShooterStart()
    {
        _camera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
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
            new Rect(_camera.pixelWidth / 2 - lineLength - crosshairSize / 2, _camera.pixelHeight / 2 - lineWidth / 2,
                lineLength, lineWidth), lineTexture);
        // Right line
        GUI.DrawTexture(
            new Rect(_camera.pixelWidth / 2 + crosshairSize / 2, _camera.pixelHeight / 2 - lineWidth / 2, lineLength,
                lineWidth), lineTexture);
        // Top line
        GUI.DrawTexture(
            new Rect(_camera.pixelWidth / 2 - lineWidth / 2, _camera.pixelHeight / 2 - lineLength - crosshairSize / 2,
                lineWidth, lineLength), lineTexture);
        // Bottom line
        GUI.DrawTexture(
            new Rect(_camera.pixelWidth / 2 - lineWidth / 2, _camera.pixelHeight / 2 + crosshairSize / 2, lineWidth,
                lineLength), lineTexture);
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
}