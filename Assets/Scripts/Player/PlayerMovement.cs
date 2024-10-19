using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private float savedMoveSpeed;
    public float sprintSpeed;

    public float groundDrag;


    public Transform orientation;
  

    public static bool isStalled;
    public static bool accessingInventory;
    public static int restrictMovementTokens = 0; //if 0, player can move, else, they cant. This keeps track if multiple sources are stopping player movement

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;


    private void Start()
    {
        savedMoveSpeed = moveSpeed;
        accessingInventory = false;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

    }

    private void Update()
    {
        MyInput();
        if (isStalled)
            return;

        MyMovementInput();
        SpeedControl();
        rb.drag = groundDrag;

    }



    private void FixedUpdate()
    {
        if (isStalled)
            return;
        MovePlayer();
    }

    private void MyInput()
    {

        if (accessingInventory || restrictMovementTokens > 0)
        {
            isStalled = true;
        }
        else
        {
            isStalled = false;
        }
    }

    private void MyMovementInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");


        if (Input.GetKeyDown(KeyCode.LeftShift) && !accessingInventory)
        {
            moveSpeed = sprintSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && !accessingInventory)
        {
            moveSpeed = savedMoveSpeed;
        }

    }


    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

}