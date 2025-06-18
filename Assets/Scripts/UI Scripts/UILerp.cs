using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UILerp : MonoBehaviour
{
    public bool lerpToStart = true; // If true, lerps to startPoint, otherwise to endPoint
    public bool disableOnEnd = false;
    public bool startAtEnd = false;
    [SerializeField] private RectTransform transformToLerp;
    [SerializeField] private float lerpMultiplier = 1f; // Multiplier to adjust the speed of the lerp
    [SerializeField] private RectTransform startPoint, endPoint;


    private void Awake()
    {

        if(transformToLerp == null) transformToLerp = GetComponent<RectTransform>();

        if (startAtEnd) transformToLerp.position = endPoint.position;

    }

    private void Update()
    {
        if (lerpToStart)
        {
            transformToLerp.position = Vector2.Lerp(transformToLerp.position, startPoint.position, Time.deltaTime * lerpMultiplier);
            if (Vector2.Distance(transformToLerp.position, startPoint.position) < 0.01f)
            {
                transformToLerp.position = startPoint.position;
            }
        }
        else
        {
            transformToLerp.position = Vector2.Lerp(transformToLerp.position, endPoint.position, Time.deltaTime * lerpMultiplier);
            if(Vector2.Distance(transformToLerp.position, endPoint.position) < 0.1f)
            {
                transformToLerp.position = endPoint.position;
            }
        }

        if(disableOnEnd && Vector2.Distance(transformToLerp.position, endPoint.position) < 0.1f)
        {
            transformToLerp.gameObject.SetActive(false);
        }
    }
}
