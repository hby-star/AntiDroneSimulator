using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public Rigidbody Rigidbody { get; private set; }

    public EntityStats EntityStats { get; private set; }

    [Header("Move Info")] public float moveSpeed = 10f;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        EntityStats = GetComponent<EntityStats>();
        Rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
    }

    public virtual void Move(float moveSpeed)
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

    public virtual bool IsGrounded()
    {
        Vector3 origin = transform.position;
        origin.y += 0.1f;
        return Physics.Raycast(origin, Vector3.down, 0.2f);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 origin = transform.position;
        origin.y += 0.1f;
        Gizmos.DrawRay(origin, Vector3.down * 0.2f);
    }

    public void ZeroVelocity()
    {
        Rigidbody.velocity = Vector3.zero;
    }
}