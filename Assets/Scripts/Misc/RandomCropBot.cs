using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomCropBot : MonoBehaviour
{
    [SerializeField] private List<CropData> allCrops = new List<CropData>();
    private CropData selectedCrop;
    private Sprite selectedSprite;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        RandomCropOnStart();
        TimeManager.OnHourlyUpdate += RandomCrop;
    }

    private void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= RandomCrop;
    }


    void RandomCrop()
    {
        if (TimeManager.Instance.currentHour == 8)
        {
            int r = Random.Range(0, allCrops.Count);
            selectedCrop = allCrops[r];
            if (selectedCrop)
            {
                r = Random.Range(0, selectedCrop.cropSprites.Length);
                selectedSprite = selectedCrop.cropSprites[r];
                spriteRenderer.sprite = selectedSprite;
            }
            else spriteRenderer.sprite = null;
        }
    }

    void RandomCropOnStart()
    {
        int r = Random.Range(0, allCrops.Count);
        selectedCrop = allCrops[r];
        if (selectedCrop)
        {
            r = Random.Range(0, selectedCrop.cropSprites.Length);
            selectedSprite = selectedCrop.cropSprites[r];
            spriteRenderer.sprite = selectedSprite;
        }
        else spriteRenderer.sprite = null;
    }
}
