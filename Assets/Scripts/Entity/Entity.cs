using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public Rigidbody Rigidbody { get; private set; }

    public EntityStats EntityStats { get; private set; }

    [Header("Move Info")]
    public float moveSpeed = 10f;
    public float rotationSpeed = 150f;

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

    public virtual void Move(float moveSpeed, float rotationSpeed)
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate rotation based on horizontal input
        Vector3 rotation = new Vector3(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);
        transform.Rotate(rotation);

        // Calculate forward movement based on vertical input
        Vector3 moveDirection = transform.forward * (verticalInput * moveSpeed);
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
}