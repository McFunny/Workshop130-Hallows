using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FaceCamera : MonoBehaviour
{
    Transform player;
    Transform freeCam;

    public bool invert;
    bool enabled = false;

    void OnEnable()
    {
        if(!player) player = FindObjectOfType<PlayerCam>().transform;
        if(!freeCam) freeCam = FindObjectOfType<FreeCam>().transform;
        enabled = true;
        StartCoroutine("FacePlayer");
    }

    void OnDisable()
    {
        enabled = false;
    }

    IEnumerator FacePlayer()
    {
        while(enabled)
        {
            Vector3 fwd;
            if (FreeCam.activeFreeCam) { fwd = freeCam.forward; }
            else { fwd = player.forward; }

            fwd.y = 0; 
            if(invert) fwd = -fwd;
            if (fwd != Vector3.zero) transform.rotation = Quaternion.LookRotation(fwd);
            yield return new WaitForSeconds(0.1f);
        }
    }


}
