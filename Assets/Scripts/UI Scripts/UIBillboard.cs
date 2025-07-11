using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillboard : MonoBehaviour
{
	Transform transformCamera;
    Transform freeCam;

	Quaternion originalRotation;

    [HideInInspector] public bool enabled = false;

    void OnEnable()
    {
        freeCam = FindObjectOfType<FreeCam>().transform;
        transformCamera = FindObjectOfType<PlayerCam>().transform;
        originalRotation = transform.rotation;

        //StartCoroutine("FaceCamera");
    }

    void OnDisable()
    {
        enabled = false;
    }

    void Update()
    {
        if (FreeCam.activeFreeCam) transform.rotation = freeCam.rotation * originalRotation;
        else transform.rotation = transformCamera.rotation * originalRotation;
    }

    IEnumerator FaceCamera() //causes issues
    {
        while(enabled)
        {
            if (FreeCam.activeFreeCam) transform.rotation = freeCam.rotation * originalRotation;
            else transform.rotation = transformCamera.rotation * originalRotation;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
