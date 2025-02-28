using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private float savedMoveSpeed;
    public float sprintSpeed;

    public float groundDrag;

    public Transform orientation;

    public CinemachineVirtualCamera playerCamera;
    public Camera toolCamera, effectsCamera;

    public static bool isStalled, isCodexOpen;
    public static bool accessingInventory;
    public static int restrictMovementTokens = 0; //if 0, player can move, else, they cant. This keeps track if multiple sources are stopping player movement

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    ControlManager controlManager;
    HeadBobController headBobController;
    public bool isSprinting;

    private Coroutine fovCoroutine;

    bool playerCanMove = true;

    [HideInInspector]
    public float velocity;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        headBobController = FindFirstObjectByType<HeadBobController>();
        restrictMovementTokens = 0;
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
        controlManager.sprint.action.canceled += CancelSprint;
    }

    private void OnDisable()
    {
        controlManager.sprint.action.started -= Sprint;
        controlManager.sprint.action.canceled -= CancelSprint;
    }

    private void Update()
    {
        if (playerCanMove && restrictMovementTokens > 0)
        {
            playerCanMove = false;
            CancelSprintManually(); // Cancel sprinting and reset FOV when movement is restricted
            print("Player cannot move");
        }
        if (!playerCanMove && restrictMovementTokens == 0)
        {
            playerCanMove = true;
            print("Player is able to move");
        }

        MyInput();
        if (isStalled || isCodexOpen)
            return;

        HandleSprintCheck();
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
        if (PlayerInteraction.Instance.stamina <= 50) return;
        Vector2 moveInput = controlManager.movement.action.ReadValue<Vector2>();

        // Allow sprinting if moving forward (positive y) and tolerate slight sideways movement
        if (moveInput.y > 0.1f && !isStalled) // Adjust threshold to detect forward movement
        {
            isSprinting = true;

            if (fovCoroutine != null)
                StopCoroutine(fovCoroutine);

            float targetFoV = 70f;
            fovCoroutine = StartCoroutine(LerpFieldOfView(targetFoV, 0.5f));
        }
    }



    private void CancelSprint(InputAction.CallbackContext obj)
    {
        // Return early if sprinting has already been cancelled
        if (!isSprinting) return;

        CancelSprintManually();
    }

    private void CancelSprintManually()
    {
        
        isSprinting = false;

        if (fovCoroutine != null)
            StopCoroutine(fovCoroutine);

        float targetFoV = 60f;
        fovCoroutine = StartCoroutine(LerpFieldOfView(targetFoV, 0.5f));
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
        Vector2 move = controlManager.movement.action.ReadValue<Vector2>();
        moveDirection = orientation.forward * move.y + orientation.right * move.x;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void HandleSprintCheck()
    {
        if (isSprinting && !isStalled)
        {
            Vector2 moveInput = controlManager.movement.action.ReadValue<Vector2>();

           
            if (moveInput.y <= 0.1f) 
            {
                isSprinting = false;

                if (fovCoroutine != null)
                    StopCoroutine(fovCoroutine);

                fovCoroutine = StartCoroutine(LerpFieldOfView(60f, 0.5f));
            }
        }
    }



    private void SpeedControl()
    {
        if (isSprinting)
        {
            moveSpeed = sprintSpeed;
        }
        else
        {
            moveSpeed = savedMoveSpeed;
        }

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void GroundedCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 2f))
        {
            // Grounded
        }
        else
        {
            rb.AddForce(-Vector3.up * 30, ForceMode.Force);
        }
    }

    private IEnumerator LerpFieldOfView(float targetFoV, float duration)
    {
        float startFoV = playerCamera.m_Lens.FieldOfView;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            playerCamera.m_Lens.FieldOfView = Mathf.Lerp(startFoV, targetFoV, elapsedTime / duration);
            toolCamera.fieldOfView = playerCamera.m_Lens.FieldOfView;
            effectsCamera.fieldOfView = playerCamera.m_Lens.FieldOfView;
            yield return null;
        }

        playerCamera.m_Lens.FieldOfView = targetFoV;
        toolCamera.fieldOfView = targetFoV;
        effectsCamera.fieldOfView = targetFoV;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }
}
