using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Collider Collider { get; private set; }
    public Camera Camera { get; private set; }

    protected bool operateNow = false;

    [Header("Move Info")] public float moveSpeed = 10f;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
        Camera = GetComponentInChildren<Camera>();
    }

    protected virtual void Update()
    {
    }

    public virtual void SetOperate(bool operateNow)
    {
        this.operateNow = operateNow;
        Camera.gameObject.SetActive(operateNow);
        transform.rotation = Quaternion.identity;
    }

    public virtual void InteractUpdate()
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

    #region isBusy

    public bool IsBusy { get; private set; }

    public IEnumerator BusyFor(float seconds)
    {
        IsBusy = true;

        yield return new WaitForSeconds(seconds);

        IsBusy = false;
    }

    #endregion

}