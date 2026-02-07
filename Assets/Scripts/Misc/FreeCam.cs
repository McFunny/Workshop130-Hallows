using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FreeCam : MonoBehaviour
{
    public CinemachineVirtualCamera cam;
    public Camera toolCamera, effectsCamera, uiCamera;

    public static bool activeFreeCam = false;

    public float defaultMoveSpeed = 50f;
    public float minSpeed = 10f;
    public float maxSpeed = 100f;

    private float moveSpeed;
    private float baseMoveSpeed;

    public float lookSpeed = 2f;
    public float defaultFOV = 60f;
    private float targetFOV;
    public float minFOV = 20f;
    public float maxFOV = 100f;

    private bool isFOVLerping = false;
    private float fovLerpStart;
    private float fovLerpEnd;
    private float fovLerpTimer = 0f;

    private float pitch;
    private float yaw;

    public float positionSmoothTime = 0.1f;
    public float rotationSmoothTime = 0.1f;
    public float fovLerpSpeed = 5f;

    private float sharedLerpDuration = 3f;

    private Vector3 velocity = Vector3.zero;
    private Quaternion currentRotation;

    private Transform trackingTarget = null;

    private Vector3 savedPosA, savedPosB;
    private Quaternion savedRotA, savedRotB;

    private bool isPositionLerping = false;
    private float posLerpTimer = 0f;

    void Start()
    {
        cam.Priority = 0;

        targetFOV = defaultFOV;
        cam.m_Lens.FieldOfView = defaultFOV;

        ResetFreeCamState();
    }


    void Update()
    {
        if (activeFreeCam)
        {
            // -------------------- INPUTS --------------------

            // FOV scroll
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetFOV -= scroll * 20f;
                targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
            }

            // Lerp to FOV min/max/default
            if (Input.GetKeyDown(KeyCode.K)) BeginFOVLerp(minFOV);
            if (Input.GetKeyDown(KeyCode.L)) BeginFOVLerp(maxFOV);
            if (Input.GetKeyDown(KeyCode.Semicolon)) BeginFOVLerp(defaultFOV);

            // FOV lerping
            if (isFOVLerping)
            {
                fovLerpTimer += Time.deltaTime;
                float t = Mathf.Clamp01(fovLerpTimer / sharedLerpDuration);
                cam.m_Lens.FieldOfView = Mathf.Lerp(fovLerpStart, fovLerpEnd, t);
                if (t >= 1f) isFOVLerping = false;
                targetFOV = cam.m_Lens.FieldOfView;
            }

            // Speed control
            if (Input.GetKeyDown(KeyCode.N)) baseMoveSpeed = Mathf.Max(minSpeed, baseMoveSpeed - 3);
            if (Input.GetKeyDown(KeyCode.M)) baseMoveSpeed = Mathf.Min(maxSpeed, baseMoveSpeed + 3);
            moveSpeed = baseMoveSpeed * (Input.GetKey(KeyCode.LeftShift) ? 2f : 1f);

            // Save positions
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                savedPosA = transform.position;
                savedRotA = transform.rotation;
                Debug.Log("Saved Position A");
            }
            if (Input.GetKeyDown(KeyCode.Home))
            {
                savedPosB = transform.position;
                savedRotB = transform.rotation;
                Debug.Log("Saved Position B");
            }

            // Jump to saved
            if (Input.GetKeyDown(KeyCode.Delete)) TeleportTo(savedPosA, savedRotA);
            if (Input.GetKeyDown(KeyCode.End)) TeleportTo(savedPosB, savedRotB);

            // Start lerping between A & B
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                isPositionLerping = true;
                posLerpTimer = 0f;
            }

            if (isPositionLerping)
            {
                posLerpTimer += Time.deltaTime;
                float t = Mathf.Clamp01(posLerpTimer / sharedLerpDuration);
                transform.position = Vector3.Lerp(savedPosA, savedPosB, t);
                transform.rotation = Quaternion.Slerp(savedRotA, savedRotB, t);

                if (t >= 1f)
                {
                    isPositionLerping = false;
                    Vector3 euler = transform.rotation.eulerAngles;
                    yaw = euler.y;
                    pitch = (euler.x > 180) ? euler.x - 360 : euler.x;
                    currentRotation = transform.rotation;
                }
            }

            else
            {
                // Mouse look
                float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

                yaw += mouseX;
                pitch -= mouseY;
                pitch = Mathf.Clamp(pitch, -90f, 90f);
                Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0);
                currentRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSmoothTime);
                transform.rotation = currentRotation;

                // Movement
                Vector3 move = new Vector3(
                    Input.GetAxisRaw("Horizontal"),
                    (Input.GetKey(KeyCode.Space) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftControl) ? 1f : 0f),
                    Input.GetAxisRaw("Vertical")
                );

                if (move.magnitude < 0.01f) move = Vector3.zero;

                Vector3 targetPos = transform.position + transform.TransformDirection(move) * moveSpeed * Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * (1f / positionSmoothTime));
            }

            // Target tracking
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                if (trackingTarget == null)
                {
                    Ray ray = new Ray(transform.position, transform.forward);
                    if (Physics.Raycast(ray, out RaycastHit hit, 20f))
                    {
                        if (hit.collider.transform.gameObject.layer == 9 || hit.collider.GetComponent<NPCMovement>())
                        {
                            trackingTarget = hit.collider.transform;
                            Debug.Log("HitTargetForTracking");
                        }
                    }
                }
                else
                {
                    trackingTarget = null;
                }
            }

            if (trackingTarget != null)
            {
                Vector3 dir = trackingTarget.position - transform.position;
                Quaternion lookRot = Quaternion.LookRotation(dir);
                currentRotation = Quaternion.Slerp(currentRotation, lookRot, rotationSmoothTime);
                transform.rotation = currentRotation;

                Vector3 euler = transform.rotation.eulerAngles;
                yaw = euler.y;
                pitch = (euler.x > 180) ? euler.x - 360 : euler.x;
            }

            // Number keys 1-9 set lerp durations
            for (KeyCode key = KeyCode.Alpha1; key <= KeyCode.Alpha9; key++)
            {
                if (Input.GetKeyDown(key))
                {
                    int num = key - KeyCode.Alpha0;
                    sharedLerpDuration = num;
                    Debug.Log($"Set Lerp Duration: {sharedLerpDuration}s");
                }
            }

            // Sync FOV with other cameras
            float fov = cam.m_Lens.FieldOfView;
            if (toolCamera) toolCamera.fieldOfView = fov;
            if (effectsCamera) effectsCamera.fieldOfView = fov;
            if (uiCamera) uiCamera.fieldOfView = fov;

            // Toggle off
            if (Input.GetKey(KeyCode.N) && Input.GetKeyDown(KeyCode.Comma))
            {
                activeFreeCam = false;
                cam.Priority = 0;
                PlayerMovement.restrictMovementTokens--;
                TeleportToPlayerCam(); // keep cam where it is, but update direction
            }
        }
        else
        {
            // Toggle on
            if (Input.GetKey(KeyCode.N) && Input.GetKeyDown(KeyCode.Comma) && StructureManager.Instance.enableCheats)
            {
                activeFreeCam = true;
                cam.Priority = 20;
                PlayerMovement.restrictMovementTokens++;
                TeleportToPlayerCam();
            }
        }
    }

    private void BeginFOVLerp(float target)
    {
        fovLerpStart = cam.m_Lens.FieldOfView;
        fovLerpEnd = target;
        fovLerpTimer = 0f;
        isFOVLerping = true;
    }

    private void TeleportTo(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
        currentRotation = rot;

        Vector3 euler = rot.eulerAngles;
        yaw = euler.y;
        pitch = (euler.x > 180) ? euler.x - 360 : euler.x;
    }

    private void TeleportToPlayerCam()
    {
        PlayerCam playerCam = FindAnyObjectByType<PlayerCam>();
        TeleportTo(playerCam.transform.position, playerCam.transform.rotation);
    }

    private void ResetFreeCamState()
    {
        moveSpeed = baseMoveSpeed;
        if (cam != null)
        {
            cam.m_Lens.FieldOfView = targetFOV;
        }
        trackingTarget = null;
        isFOVLerping = false;
        isPositionLerping = false;
        fovLerpTimer = 0f;
        posLerpTimer = 0f;
    }

}
