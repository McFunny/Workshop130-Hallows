using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillboard : MonoBehaviour
{
	Transform transformCamera;

	Quaternion originalRotation;

    void Start()
    {
        transformCamera = FindObjectOfType<PlayerCam>().transform;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (FreeCam.activeFreeCam) transformCamera = FindObjectOfType<FreeCam>().transform;
        else transformCamera = FindObjectOfType<PlayerCam>().transform;
        transform.rotation = transformCamera.rotation * originalRotation;   
    }
}
