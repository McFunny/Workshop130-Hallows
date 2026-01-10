using Cinemachine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    [Header("Movement")]
    public float moveSpeed; //Current Move Speed
    private float savedMoveSpeed;
    public float sprintSpeed;

    public List<MovementSpeedModifiers> speedMods = new List<MovementSpeedModifiers>();

    public float groundDrag;

    public Transform orientation;

    public CinemachineVirtualCamera playerCamera;
    public Camera toolCamera, effectsCamera, uiCamera;

    public static bool isStalled, isCodexOpen;
    public static bool accessingInventory;
    public static int restrictMovementTokens = 0; //if 0, player can move, else, they cant. This keeps track if multiple sources are stopping player movement
    public static bool limitMaxVelocity = true;
    public static bool ignoreMovementInputs = false; //if true, player can still look around but not move, which is different from the restrict movement tokens

    float horizontalInput;
    float verticalInput;

    private PhysicMaterial noFriction;
    private CapsuleCollider capsuleCollider;

    Vector3 moveDirection;

    Rigidbody rb;

    ControlManager controlManager;
    HeadBobController headBobController;
    public bool isSprinting;
    bool isGrounded;

    private Coroutine fovCoroutine;

    bool playerCanMove = true;

    [HideInInspector]
    public float velocity;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

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
        capsuleCollider = GetComponent<CapsuleCollider>();
        noFriction = capsuleCollider.material;
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

        capsuleCollider.material = DialogueController.Instance.IsTalking() ? null : noFriction;

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
        if (isStalled || isCodexOpen || ignoreMovementInputs)
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
            if(PlayerPrefs.GetInt("ToggleSprint") == 1 && isSprinting == true)
            {
                CancelSprintManually();
                return;
            }
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

        if(PlayerPrefs.GetInt("ToggleSprint") == 1) return; //1 is when toggle sprint is active

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

    public void ApplyForceToPlayer(float force, Vector3 dir)
    {
        //rb.velocity = Vector3.zero;
        dir = new Vector3(dir.x, 0, dir.z);
        rb.AddForce(dir.normalized * force, ForceMode.Force);
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

    public void ApplySpeedMod(MovementSpeedModifiers newMod)
    {
        for(int i = 0; i < speedMods.Count; i++)
        {
            if(speedMods[i].source == newMod.source) return;
        }

        speedMods.Add(newMod);
    }

    public void RemoveSpeedMod(GameObject source)
    {
        for(int i = 0; i < speedMods.Count; i++)
        {
            if(speedMods[i].source == source)
            {
                speedMods.RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveSpeedMod(string source)
    {
        for(int i = 0; i < speedMods.Count; i++)
        {
            if(speedMods[i].tag == source)
            {
                speedMods.RemoveAt(i);
                return;
            }
        }
    }



    private void SpeedControl()
    {
        //The better system but one I really dont feel like working on
        float movementMult = 1;

        List<string> appliedTags = new List<string>();

        for(int i = 0; i < speedMods.Count; i++)
        {
            if(speedMods[i].source == null)
            {
                speedMods.RemoveAt(i);
                i--;
            }
            else if(speedMods[i].stack == true || !appliedTags.Contains(speedMods[i].tag))
            {
                movementMult *= speedMods[i].modifier;
                appliedTags.Add(speedMods[i].tag);
            }
        }


        float walkMod = 0;
        float sprintMod = 0;

        if(StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.Dare))
        {
            AchievementManager.Instance.NotifyDareConsumed();
            walkMod += 3f;
            sprintMod += 4.5f;
        }
        if(StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.Frost))
        {
            walkMod -= 5f;
            sprintMod -= 6f;
        }
        if (isSprinting)
        {
            moveSpeed = (sprintSpeed + sprintMod) * movementMult;
        }
        else
        {
            moveSpeed = (savedMoveSpeed + walkMod) * movementMult;

            Vector2 moveInput = controlManager.movement.action.ReadValue<Vector2>();

            if(moveInput.y < 0f) moveSpeed -= 3; // Walking backwards is slower
        }

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit velocity if needed
        if (flatVel.magnitude > moveSpeed && limitMaxVelocity)
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
            isGrounded = true;
        }
        else
        {
            //print("Player fast falling");
            isGrounded = false;
            rb.AddForce(-Vector3.up * 120, ForceMode.Force);
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
            if(uiCamera) uiCamera.fieldOfView = playerCamera.m_Lens.FieldOfView;
            yield return null;
        }

        playerCamera.m_Lens.FieldOfView = targetFoV;
        toolCamera.fieldOfView = targetFoV;
        effectsCamera.fieldOfView = targetFoV;
        if(uiCamera) uiCamera.fieldOfView = targetFoV;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

}

public class MovementSpeedModifiers
{
    public GameObject source;
    public float modifier = 1f;
    public string tag = "Default"; //identifier of the speed mod
    public bool stack = true; //if false, speed mod will ignore mods with the same tag

    public MovementSpeedModifiers(GameObject _source, float _modifier, string _tag, bool _stack)
    {
        source = _source;
        modifier = _modifier;
        tag = _tag;
        stack = _stack;
    }
}
