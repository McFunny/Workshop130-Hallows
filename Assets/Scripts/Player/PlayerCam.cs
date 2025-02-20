using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    public static PlayerCam Instance;
    //bool allowCameraInfluence = false;

    Vector3 posOfInterest = new Vector3(0,0,0); //What the camera pans to
    float interestRotSpeed = 3;
    bool focusOnInterest = false;

    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    ControlManager controlManager;

    const float contScalar = 5;

    private bool isSprinting;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        controlManager = FindFirstObjectByType<ControlManager>();
    }
    private void Start()
    {
        CursorLock();
    }

    private static void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (PlayerMovement.accessingInventory || PlayerMovement.isCodexOpen)
        {
            if(!ControlManager.isController)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            
            return;
        }
        else if (!PlayerMovement.accessingInventory && !PauseScript.isPaused)
        {
            CursorLock();
        }

        if ((PlayerMovement.restrictMovementTokens > 0) || PlayerMovement.isCodexOpen || PauseScript.isPaused)
        {
            //
        }
        else
        {
            Vector2 look = controlManager.look.action.ReadValue<Vector2>() * PlayerPrefs.GetFloat("Sensitivity", 1.0f);
            float lookX = look.x * sensX;
            float lookY = look.y * sensY;
            // Scaling sensitivity to match old input system;
            lookX *= 0.5f;
            lookX *= 0.1f;
            lookY *= 0.5f;
            lookY *= 0.1f;

            if(ControlManager.isGamepad)
            {
                lookX = lookX * contScalar;
                lookY = lookY * contScalar;
            }
            else
            {
                lookX = lookX * 1;
                lookY = lookY * 1;
            }


            yRotation += lookX;
            xRotation -= lookY;

            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
        

        if(focusOnInterest)
        {
            if(PlayerMovement.restrictMovementTokens == 0)
            {
                print("Clearing cuz player can move");
                ClearObjectOfInterest();
                return;
            }

            Vector3 dir = posOfInterest - transform.position;

            Quaternion rot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), interestRotSpeed * Time.deltaTime);
            transform.rotation = rot;
            orientation.rotation = Quaternion.Euler(0, rot.y, 0);

            xRotation = rot.eulerAngles.x;
            yRotation = rot.eulerAngles.y;
        }

        /*if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            sensX += 1;
            sensY += 1;
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            sensX -= 1;
            sensY -= 1;
        } */


    }

    public void NewObjectOfInterest(Vector3 newInterest)
    {
        posOfInterest = newInterest;
        focusOnInterest = true;
        //allowCameraInfluence = allowInfluence;
    }

    public void ClearObjectOfInterest()
    {
        focusOnInterest = false;
        //posOfInterest = new Vector3(0,0,0);
        //allowCameraInfluence = false;
    }
}