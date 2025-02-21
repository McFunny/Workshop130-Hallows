using System.Collections.Generic;
using UnityEngine;

public class RandomCropBot : MonoBehaviour
{
    private List<CropItem> allCrops = new List<CropItem>();
    private CropData selectedCrop;
    private Sprite selectedSprite;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Database _database;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        RandomCropOnStart();
        TimeManager.OnHourlyUpdate += RandomCrop;
    }


    void RandomCrop()
    {
        if (TimeManager.Instance.currentHour == 8)
        {
            allCrops = _database.GetAllCrops();
            int r = Random.Range(0, allCrops.Count);
            selectedCrop = allCrops[r].cropData;
            r = Random.Range(0, selectedCrop.cropSprites.Length);
            selectedSprite = selectedCrop.cropSprites[r];
            spriteRenderer.sprite = selectedSprite;
        }
    }

    void RandomCropOnStart()
    {
        allCrops = _database.GetAllCrops();
        int r = Random.Range(0, allCrops.Count);
        selectedCrop = allCrops[r].cropData;
        r = Random.Range(0, selectedCrop.cropSprites.Length);
        selectedSprite = selectedCrop.cropSprites[r];
        spriteRenderer.sprite = selectedSprite;
    }
}
