using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowInputs : MonoBehaviour
{
    public GameObject image;

    void Update()
    {
        if(ControlManager.isController) image.SetActive(true);
        else image.SetActive(false);
    }
}
