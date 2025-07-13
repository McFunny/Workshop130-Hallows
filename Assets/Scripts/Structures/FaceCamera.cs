using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FaceCamera : MonoBehaviour
{
    Transform player;
    Transform freeCam;

    public bool invert;
    [HideInInspector] public bool objectActive = false;

    void OnEnable()
    {
        if(!player) player = FindObjectOfType<PlayerCam>().transform;
        if(!freeCam) freeCam = FindObjectOfType<FreeCam>().transform;
        objectActive = true;
        if(FaceCameraManager.Instance && !FaceCameraManager.Instance.allFaceCameras.Contains(this)) FaceCameraManager.Instance.allFaceCameras.Add(this);
        else StartCoroutine("FacePlayer");
    }

    void OnDisable()
    {
        objectActive = false;
        //if(FaceCameraManager.Instance) FaceCameraManager.Instance.allFaceCameras.Remove(this);
    }

    IEnumerator FacePlayer()
    {
        while(objectActive)
        {
            Vector3 fwd;
            if (FreeCam.activeFreeCam) { fwd = freeCam.forward; }
            else { fwd = player.forward; }

            fwd.y = 0; 
            if(invert) fwd = -fwd;
            if (fwd != Vector3.zero) transform.rotation = Quaternion.LookRotation(fwd);
            yield return new WaitForSeconds(0.15f);
        }
    }


}
