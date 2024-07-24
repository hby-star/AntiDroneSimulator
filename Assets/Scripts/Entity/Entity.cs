using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Collider Collider { get; private set; }

    public EntityStats EntityStats { get; private set; }

    protected bool operateNow = false;

    [Header("Move Info")] public float moveSpeed = 10f;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        EntityStats = GetComponent<EntityStats>();
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
    }

    protected virtual void Update()
    {
    }

    public void ZeroHorVelocity()
    {
        Rigidbody.velocity = new Vector3(0, Rigidbody.velocity.y, 0);
    }

    public void ZeroVelocity()
    {
        Rigidbody.velocity = Vector3.zero;
    }

    public bool IsOperateNow()
    {
        return operateNow;
    }
}