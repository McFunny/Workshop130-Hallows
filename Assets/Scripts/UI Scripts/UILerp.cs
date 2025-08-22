using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UILerp : MonoBehaviour
{
    public bool lerpToStart = true; // If true, lerps to startPoint, otherwise to endPoint
    public bool disableOnEnd = false;
    public bool startAtEnd = false;
    public bool ignoreTimeScale = true; // If true, lerp will ignore Time.timeScale
    public bool isAtEnd;
    public bool doLerpX = true;
    public bool doLerpY = true;
    private bool isOneAxis = false;
    private float timeScale;
    [SerializeField] private RectTransform transformToLerp;
    [SerializeField] private float lerpMultiplier = 1f; // Multiplier to adjust the speed of the lerp
    [SerializeField] private RectTransform startPoint, endPoint;
    private Vector2 modifiedStartPos, modifiedEndPos;


    public void Awake()
    {

        if (transformToLerp == null) transformToLerp = GetComponent<RectTransform>();

        if (startAtEnd) transformToLerp.position = endPoint.position;

        if (ignoreTimeScale) timeScale = Time.fixedDeltaTime;
        else timeScale = Time.deltaTime;

    }

    public void Update()
    {
        if (doLerpX && doLerpY)
        {
            modifiedStartPos = startPoint.position;
            modifiedEndPos = endPoint.position;
            isOneAxis = false;
        }
        else if (!doLerpX && doLerpY)
        {
            modifiedStartPos = new Vector2(transformToLerp.position.x, startPoint.position.y);
            modifiedEndPos = new Vector2(transformToLerp.position.x, endPoint.position.y);
            isOneAxis = true;
        }
        else if (doLerpX && !doLerpY)
        {
            modifiedStartPos = new Vector2(startPoint.position.x, transformToLerp.position.y);
            modifiedEndPos = new Vector2(endPoint.position.x, transformToLerp.position.y);
            isOneAxis = true;
        }
        else
        {
            Debug.LogError("Both doLerpX and doLerpY are false. Please enable at least one axis for lerping. Disabling Component.");
            enabled = false;
            return;
        }

        PerformLerp(lerpToStart);

        if (disableOnEnd && Vector2.Distance(transformToLerp.position, endPoint.position) < 0.1f)
        {
            transformToLerp.gameObject.SetActive(false);
            isAtEnd = true;
        }
    }

    public void PerformLerp(bool whereToLerp)
    {
        if (whereToLerp)
        {
            transformToLerp.position = Vector2.Lerp(transformToLerp.position, modifiedStartPos, timeScale * lerpMultiplier);
        
            if (Vector2.Distance(transformToLerp.position, startPoint.position) < 0.01f)
            {
                transformToLerp.position = startPoint.position;
            }
            else isAtEnd = false;
        }
        else
        {
            transformToLerp.position = Vector2.Lerp(transformToLerp.position, modifiedEndPos, timeScale * lerpMultiplier);

            if (Vector2.Distance(transformToLerp.position, endPoint.position) < 0.1f)
            {
                transformToLerp.position = endPoint.position;
                isAtEnd = true;
            }
            else isAtEnd = false;
        }
    }
}
