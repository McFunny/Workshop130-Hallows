using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenAnimation : MonoBehaviour
{
    public GameObject loadingScreenCanvas, animationObject;
    void Update()
    {
        animationObject.SetActive(loadingScreenCanvas.activeSelf);
    }
}
