using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraEffectsHandler : MonoBehaviour
{
    private CinemachineVirtualCamera playerCamera;
    private float startingFOV;
    private float startingDutch;
    void Start()
    {
        PlayerCam playerCamScript = FindAnyObjectByType<PlayerCam>();

        if (playerCamScript != null)
        {
            playerCamera = playerCamScript.GetComponent<CinemachineVirtualCamera>();
            startingFOV = playerCamera.m_Lens.FieldOfView;
            startingDutch = playerCamera.m_Lens.Dutch;
            if (playerCamera == null)
            {
                Debug.Log("We have a problem with the camera");
            }
        }
    }

    public void ScreenShake()
    {

    }
}
