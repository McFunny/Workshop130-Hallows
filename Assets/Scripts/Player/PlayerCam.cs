using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

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

    public bool overrideCamera = false;
    float cameraRecoilX, cameraRecoilY;

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

    private void LateUpdate()
    {
        if (PlayerMovement.accessingInventory || PlayerMovement.isCodexOpen || DebugUI.isDebugMenuOpen || CraftingSystem.isCraftingMenuOpen)
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

        if ((PlayerMovement.restrictMovementTokens > 0) || PlayerMovement.isCodexOpen || PauseScript.isPaused || overrideCamera)
        {
            //
        }
        else
        {
            if (controlManager == null) return;
            Vector2 look = controlManager.look.action.ReadValue<Vector2>() * PlayerPrefs.GetFloat("Sensitivity", 1.0f);
            float lookX = look.x * sensX;
            float lookY = look.y * sensY;

            if(cameraRecoilX != 0 || cameraRecoilY != 0)
            {
                if(cameraRecoilX > 0)
                {
                    lookX += 5;
                    cameraRecoilX -= 5;
                }
                else
                {
                    lookX -= 5;
                    cameraRecoilX += 5;
                }
                if(cameraRecoilX <= 5 && cameraRecoilX >= -5) cameraRecoilX = 0;

                if(cameraRecoilY > 0)
                {
                    lookY += 5;
                    cameraRecoilY -= 5;
                }
                else
                {
                    lookY -= 5;
                    cameraRecoilY += 5;
                }
                if(cameraRecoilY <= 5 && cameraRecoilY >= -5) cameraRecoilY = 0;
            }
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

            if(xRotation > 90) 
            {
                print("Passed! Old num was " + xRotation);
                if(xRotation < 300) xRotation = 90; //To fix focus issue (causes some irregularities)
                else xRotation = xRotation - 360;
            }

            //print(xRotation);

            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
        

        if(focusOnInterest)
        {
            if(PlayerMovement.restrictMovementTokens == 0 && !overrideCamera)
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
            //xRotation = Mathf.Clamp(xRotation, -90f, 90f);
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

    public void ForceChangeRotation(float rotY)
    {
        yRotation = rotY;
    }

    public void AddCameraRecoil(float x, float y)
    {
        cameraRecoilX += x;
        cameraRecoilY += y;
    }
}