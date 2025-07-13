using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCameraManager : MonoBehaviour
{
    public static FaceCameraManager Instance;

    Transform player;
    Transform freeCam;

    public List<FaceCamera> allFaceCameras = new List<FaceCamera>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            print("Destroyed Copy");
            return;
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        if(!player) player = FindObjectOfType<PlayerCam>().transform;
        if(!freeCam) freeCam = FindObjectOfType<FreeCam>().transform;
        StartCoroutine("FacePlayer");
    }

    IEnumerator FacePlayer()
    {
        while(enabled)
        {
            for(int i = 0; i < allFaceCameras.Count; i++)
            {
                if(allFaceCameras[i] == null || !allFaceCameras[i].enabled)
                {
                    allFaceCameras.RemoveAt(i);
                    i--;
                    continue;
                }
                Vector3 fwd;
                if (FreeCam.activeFreeCam) { fwd = freeCam.forward; }
                else { fwd = player.forward; }

                fwd.y = 0; 
                if(allFaceCameras[i].invert) fwd = -fwd;
                if (fwd != Vector3.zero) allFaceCameras[i].transform.rotation = Quaternion.LookRotation(fwd);
            }
            yield return new WaitForSeconds(0.15f);
        }
    }
}
