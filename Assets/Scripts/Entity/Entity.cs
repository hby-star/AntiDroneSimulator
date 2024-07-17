using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public CharacterController CharacterController { get; private set; }

    public EntityStats EntityStats { get; private set; }

    [Header("Move Info")]
    public float moveSpeed = 10f;
    public float rotationSpeed = 150f;
    public float gravity = 9.8f;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        EntityStats = GetComponent<EntityStats>();
        CharacterController = GetComponent<CharacterController>();
    }

    protected virtual void Update()
    {
    }

    public virtual void Move(float moveSpeed, float rotationSpeed, float gravity)
    {
        // Capture input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate movement
        Vector3 moveDirection = transform.forward * verticalInput * moveSpeed;
        moveDirection.y -= gravity * Time.deltaTime; // Apply gravity

        // Move the character
        CharacterController.Move(moveDirection * Time.deltaTime);

        // Calculate rotation
        Vector3 rotation = new Vector3(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);

        // Rotate the character
        transform.Rotate(rotation);
    }
}