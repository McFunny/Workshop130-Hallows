using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillboard : MonoBehaviour
{
	Transform transformCamera;

	Quaternion originalRotation;

    void Start()
    {
        transformCamera = GameObject.Find("Main Camera").transform;
        originalRotation = transform.rotation;
    }

    void Update()
    {
     	transform.rotation = transformCamera.rotation * originalRotation;   
    }
}
