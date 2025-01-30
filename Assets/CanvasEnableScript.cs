using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasEnableScript : MonoBehaviour
{
    public GameObject UI;
    void Start()
    {
        if (UI != null)
        {
            UI.SetActive(true);
        }
    }

   
}
