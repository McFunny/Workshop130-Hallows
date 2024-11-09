using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
   public bool destroy = false;
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
        Destroy(this.gameObject);
    }
   
}
