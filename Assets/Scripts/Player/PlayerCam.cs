using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    ControlManager controlManager;

    void Awake()
    {
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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
        else if (!PlayerMovement.accessingInventory)
        {
            CursorLock();
        }

        if (PlayerMovement.restrictMovementTokens > 0 || PlayerMovement.isCodexOpen) return;

        Vector2 look = controlManager.look.action.ReadValue<Vector2>();
        float lookX = look.x * sensX;
        float lookY = look.y * sensY;
        // Scaling sensitivity to match old input system;
        lookX *= 0.5f;
        lookX *= 0.1f;
        lookY *= 0.5f;
        lookY *= 0.1f;


        yRotation += lookX;
        xRotation -= lookY;

        /* if (ControlManager.isController)
        {
            float controllerX = Input.GetAxis("RightJoyX") * sensX;
            float controllerY = Input.GetAxis("RightJoyY") * sensX * -1;
            yRotation += controllerX;
            xRotation -= controllerY;
        }
        else
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;
            yRotation += mouseX;
            xRotation -= mouseY;
        } */
        
        

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            sensX += 1;
            sensY += 1;
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            sensX -= 1;
            sensY -= 1;
        }


    }
}