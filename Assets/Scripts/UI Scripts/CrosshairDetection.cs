using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairDetection : MonoBehaviour
{
    public Image r;

    public Color ogColor, highlightedColor;

    Transform castPoint;

    public LayerMask interactionLayers;

    // Start is called before the first frame update
    void Start()
    {
        castPoint = PlayerInteraction.Instance.mainCam.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 fwd = castPoint.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(castPoint.position, fwd, out hit, 5.5f, interactionLayers))
        {
            r.color = highlightedColor;
        }
        else r.color = ogColor;
    }
}
