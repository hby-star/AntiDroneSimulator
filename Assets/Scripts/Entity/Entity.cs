using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator Animator;
    public Rigidbody Rigidbody { get; private set; }
    public Collider Collider { get; private set; }
    public Camera Camera;

    protected bool operateNow = false;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();

        Rigidbody.freezeRotation = true;
    }

    protected virtual void Update()
    {
    }

    public virtual void SetOperate(bool operateNow)
    {
        this.operateNow = operateNow;
        Camera.enabled = operateNow;
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

    [NonSerialized] public bool IsBusy;

    public IEnumerator BusyFor(float seconds)
    {
        IsBusy = true;

        yield return new WaitForSeconds(seconds);

        IsBusy = false;
    }

    #endregion
}