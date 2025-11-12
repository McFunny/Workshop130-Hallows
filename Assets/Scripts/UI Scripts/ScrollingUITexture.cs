using UnityEngine;
using UnityEngine.UI;

public class UIScrollingUITexture : MonoBehaviour
{
    public float scrollSpeedX = 0.0f;
    public float scrollSpeedY = 0.0f;

    private RawImage uiImage;

    void Start()
    {
        uiImage = GetComponent<RawImage>();
    }

    private void OnEnable()
    {
        if (uiImage == null) return;
        uiImage.uvRect = new Rect(0, 0, 1, 1);
    }

    void Update()
    {
        if (uiImage == null) return;
        Rect offset = new Rect(uiImage.uvRect.x, uiImage.uvRect.y, uiImage.uvRect.width, uiImage.uvRect.height);
        offset.x += scrollSpeedX * Time.deltaTime;
        offset.y += scrollSpeedY * Time.deltaTime;
        offset.x %= 1.0f;
        offset.y %= 1.0f;
        uiImage.uvRect = offset;
    }
}