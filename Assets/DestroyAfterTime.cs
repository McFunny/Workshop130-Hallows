using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
   public bool destroy = false;
   public bool fadeChildMeshes = false;
    [SerializeField] int time;

    private void Update()
    {
        if (destroy)
        {
            StartCoroutine(DestroyObject(time));
        }
    }

    IEnumerator DestroyObject(int time)
    {
        yield return new WaitForSeconds(time);
        /*if(fadeChildMeshes)
        {
            x = 20;
            while(x > 0)
            {
                foreach(Transform child in damageParticlesObject.transform)
                {
                    //
                }
            }
        }*/
        Destroy(this.gameObject);
    }
   
}
