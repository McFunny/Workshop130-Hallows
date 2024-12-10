using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private float savedMoveSpeed;
    public float sprintSpeed;

    public float groundDrag;


    public Transform orientation;
  

    public static bool isStalled, isCodexOpen;
    public static bool accessingInventory;
    public static int restrictMovementTokens = 0; //if 0, player can move, else, they cant. This keeps track if multiple sources are stopping player movement

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    ControlManager controlManager;
    private bool isSprinting;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }
    private void Start()
    {  
        isSprinting = false;
        savedMoveSpeed = moveSpeed;
        accessingInventory = false;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        controlManager.sprint.action.started += Sprint;
        //controlManager.sprint.action.canceled += Sprint;
    }
    private void OnDisable()
    {
        controlManager.sprint.action.started -= Sprint;
        //controlManager.sprint.action.canceled -= Sprint;
    }

    private void Update()
    {
        MyInput();
        if (isStalled || isCodexOpen)
            return;

        //MyMovementInput();
        SpeedControl();
        rb.drag = groundDrag;
        GroundedCheck();

    }

    private void FixedUpdate()
    {
        if (isStalled || isCodexOpen)
            return;
        MovePlayer();
    }

    private void Sprint(InputAction.CallbackContext obj)
    {
        isSprinting = !isSprinting;
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


    private void MovePlayer()
    {
        // calculate movement direction
        Vector2 move = controlManager.movement.action.ReadValue<Vector2>();
        moveDirection = orientation.forward * move.y + orientation.right * move.x;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

    }

    private void SpeedControl()
    {
        if(isSprinting){moveSpeed = sprintSpeed;}
        else{moveSpeed = savedMoveSpeed;}

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void GroundedCheck()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 2f))
        { 
            //grounded
        }
        else
        {
            rb.AddForce(-Vector3.up * 30, ForceMode.Force);
        }
    }

}