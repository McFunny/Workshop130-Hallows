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

    const float contScalar = 5;

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
        else if (!PlayerMovement.accessingInventory && !PauseScript.isPaused)
        {
            CursorLock();
        }

        if (PlayerMovement.restrictMovementTokens > 0 || PlayerMovement.isCodexOpen) return;
        if (PauseScript.isPaused) return;
        Vector2 look = controlManager.look.action.ReadValue<Vector2>();
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