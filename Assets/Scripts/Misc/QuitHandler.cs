using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitHandler : MonoBehaviour
{
    [SerializeField] private string linkToOpen;
    private void OnApplicationQuit()
    {
        #if !UNITY_EDITOR
        Application.OpenURL(linkToOpen);
        #endif
    }
}
