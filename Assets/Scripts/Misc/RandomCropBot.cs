using System.Collections;
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
            if (selectedCrop.ichorIntake > 0)
            {
                StartCoroutine(FindNewCrop());
            }
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
        allCrops = _database.GetAllCrops();
        int r = Random.Range(0, allCrops.Count);
        selectedCrop = allCrops[r].cropData;
        if (selectedCrop.ichorIntake > 0)
        {
            StartCoroutine(FindNewCrop());
        }
        if (selectedCrop)
        {
            r = Random.Range(0, selectedCrop.cropSprites.Length);
            selectedSprite = selectedCrop.cropSprites[r];
            spriteRenderer.sprite = selectedSprite;
        }
        else spriteRenderer.sprite = null;
    }

    IEnumerator FindNewCrop()
    {
        int attempts = 0;
        do
        {
            allCrops = _database.GetAllCrops();
            int r = Random.Range(0, allCrops.Count);
            selectedCrop = allCrops[r].cropData;
            attempts++;
            
        } while (selectedCrop.ichorIntake > 0 && attempts < 10);

        if (attempts >= 10) { selectedCrop = null;  yield return null; }
        yield return new WaitForSeconds(0.1f);
    }
}
