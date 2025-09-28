using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectEnableTrigger : MonoBehaviour
{
    public List<GameObject> objects;

    public bool enableObjects;
    void OnTriggerEnter(Collider other)
    {
        foreach(GameObject thing in objects)
        {
            if(enableObjects) thing.SetActive(true);
            else thing.SetActive(false);
        }
    }
}
