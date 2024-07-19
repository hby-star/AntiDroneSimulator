using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public Rigidbody Rigidbody { get; private set; }

    public EntityStats EntityStats { get; private set; }

    public bool operateNow;

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

    public virtual bool IsGrounded()
    {
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider == null) return false;

        Vector3 capsuleBottom = new Vector3(transform.position.x, transform.position.y + capsuleCollider.radius, transform.position.z);
        Vector3 capsuleTop = new Vector3(transform.position.x, transform.position.y + capsuleCollider.height - capsuleCollider.radius, transform.position.z);

        float distanceToGround = capsuleCollider.height / 2 - capsuleCollider.radius + 0.1f;
        return Physics.CapsuleCast(capsuleTop, capsuleBottom, capsuleCollider.radius, Vector3.down, distanceToGround);
    }

    public void OnDrawGizmos()
    {
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider == null) return;

        Vector3 capsuleBottom = new Vector3(transform.position.x, transform.position.y + capsuleCollider.radius, transform.position.z);
        Vector3 capsuleTop = new Vector3(transform.position.x, transform.position.y + capsuleCollider.height - capsuleCollider.radius, transform.position.z);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(capsuleTop, capsuleCollider.radius);
        Gizmos.DrawWireSphere(capsuleBottom, capsuleCollider.radius);
    }

    public void ZeroVelocity()
    {
        Rigidbody.velocity = Vector3.zero;
    }
}