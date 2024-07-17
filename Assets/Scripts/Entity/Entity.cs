using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public CharacterController CharacterController { get; private set; }

    public EntityStats EntityStats { get; private set; }

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        Animator = GetComponent<Animator>();
        EntityStats = GetComponent<EntityStats>();
        CharacterController = GetComponent<CharacterController>();
    }

    protected virtual void Update()
    {
    }

    public virtual void Move(float speed, float gravity)
    {
        float deltaX = Input.GetAxis("Horizontal") * speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = Vector3.ClampMagnitude(movement, speed);

        movement.y = gravity;

        movement *= Time.deltaTime;
        movement = transform.TransformDirection(movement);
        CharacterController.Move(movement);
    }
}