using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableMesh : MonoBehaviour
{
    private MeshRenderer playerRenderer;
    void Start()
    {
        playerRenderer = GetComponent<MeshRenderer>();
        playerRenderer.enabled = false;
    }
}
