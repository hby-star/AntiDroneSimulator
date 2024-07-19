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