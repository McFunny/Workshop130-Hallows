using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FaceCamera : MonoBehaviour
{
    Transform player;
    Transform freeCam;

    public bool invert;
    public bool npcOverride; //if true, object looks at player and their rotation does not try to match with camera
    [HideInInspector] public bool objectActive = false;

    void OnEnable()
    {
        if(!player) player = FindObjectOfType<PlayerCam>().transform;
        if(!freeCam) freeCam = FindObjectOfType<FreeCam>().transform;
        objectActive = true;
        if(FaceCameraManager.Instance && !FaceCameraManager.Instance.allFaceCameras.Contains(this) && !npcOverride) FaceCameraManager.Instance.allFaceCameras.Add(this);
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

            if(npcOverride) fwd = (player.position - transform.position).normalized;

            fwd.y = 0; 
            if(invert) fwd = -fwd;

            //if(npcOverride) transform.LookAt(fwd);
            if (fwd != Vector3.zero) transform.rotation = Quaternion.LookRotation(fwd);
            yield return new WaitForSeconds(0.1f);
        }
    }


}
