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
    public float minSpeed = 20f;
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
    private float fovLerpDuration = 3f;
    private float fovLerpTimer = 0f;


    private float pitch;
    private float yaw;

   

    public float positionSmoothTime = 0.1f;
    public float rotationSmoothTime = 0.1f;
    public float fovLerpSpeed = 5f;

    private Vector3 velocity = Vector3.zero;
    private Quaternion currentRotation;

    private Transform trackingTarget = null;

    void Start()
    {
        cam.Priority = 0;
        ResetFreeCamState();
    }

    void Update()
    {
        if (activeFreeCam)
        {
            // FOV scroll control
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetFOV -= scroll * 20f;
                targetFOV = Mathf.Clamp(targetFOV, 10f, 100f);
            }

            // FOV lerp
            if (cam != null)
                cam.m_Lens.FieldOfView = Mathf.Lerp(cam.m_Lens.FieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);

            // Speed controls
            if (Input.GetKeyDown(KeyCode.N))
            {
                baseMoveSpeed = Mathf.Max(minSpeed, baseMoveSpeed - 3);
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                baseMoveSpeed = Mathf.Min(maxSpeed, baseMoveSpeed + 3);
            }

            moveSpeed = baseMoveSpeed * (Input.GetKey(KeyCode.LeftShift) ? 2f : 1f);

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


            // Tracking
            if (Input.GetKeyDown(KeyCode.O))
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

            // If tracking a target look at it
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

            // Start FOV lerp to min
            if (Input.GetKeyDown(KeyCode.K))
            {
                fovLerpStart = cam.m_Lens.FieldOfView;
                fovLerpEnd = minFOV;
                fovLerpTimer = 0f;
                isFOVLerping = true;
            }

            // Start FOV lerp to max
            if (Input.GetKeyDown(KeyCode.L))
            {
                fovLerpStart = cam.m_Lens.FieldOfView;
                fovLerpEnd = maxFOV;
                fovLerpTimer = 0f;
                isFOVLerping = true;
            }
            // return to default no matter where
            if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                fovLerpStart = cam.m_Lens.FieldOfView;
                fovLerpEnd = defaultFOV;
                fovLerpTimer = 0f;
                isFOVLerping = true;
            }

            // Perform FOV lerp
            if (isFOVLerping)
            {
                fovLerpTimer += Time.deltaTime;
                float t = Mathf.Clamp01(fovLerpTimer / fovLerpDuration);
                cam.m_Lens.FieldOfView = Mathf.Lerp(fovLerpStart, fovLerpEnd, t);

               
                if (t >= 1f)
                {
                    isFOVLerping = false;
                }

               
                targetFOV = cam.m_Lens.FieldOfView;
            }

            // Sync FOV with other cameras
            if (toolCamera) toolCamera.fieldOfView = cam.m_Lens.FieldOfView;
            if (effectsCamera) effectsCamera.fieldOfView = cam.m_Lens.FieldOfView;
            if (uiCamera) uiCamera.fieldOfView = cam.m_Lens.FieldOfView;

           
            if (Input.GetKey(KeyCode.N) && Input.GetKeyDown(KeyCode.Comma))
            {
                activeFreeCam = false;
                cam.Priority = 0;
                PlayerMovement.restrictMovementTokens--;
                ResetFreeCamState();
            }

        }
        else
        {
          
            if (Input.GetKey(KeyCode.N) && Input.GetKeyDown(KeyCode.Comma))
            {
                activeFreeCam = true;
                cam.Priority = 20;
                PlayerMovement.restrictMovementTokens++;
                TeleportToPlayerCam();
                ResetFreeCamState();
            }
        }
    }

    private void TeleportToPlayerCam()
    {
        PlayerCam playerCam = FindAnyObjectByType<PlayerCam>();
        transform.position = playerCam.transform.position;
        Quaternion rot = playerCam.transform.rotation;
        transform.rotation = rot;

        Vector3 euler = rot.eulerAngles;
        yaw = euler.y;
        pitch = (euler.x > 180) ? euler.x - 360 : euler.x;
        currentRotation = rot;
    }

    private void ResetFreeCamState()
    {
        baseMoveSpeed = defaultMoveSpeed;
        moveSpeed = baseMoveSpeed;
        if (cam != null)
        {
            cam.m_Lens.FieldOfView = defaultFOV;
            targetFOV = defaultFOV;
        }
        trackingTarget = null;
    }
}
