using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropStatsRework : MonoBehaviour
{
    Camera mainCam;
    public Color c_default, c_rising, c_lowering, c_transparent;
    public GameObject cropStats, cropStatsDetailed;
    private FarmLand hitCrop;
    public Image cropSprite, cropSpriteD, gloamArrow, terraArrow, ichorArrow, waterArrow;
    private bool isActive;
    public float reach = 8;
    public TextMeshProUGUI cropNameText, cropNameTextD, growthStageNumber, growthStageNumberD, gloamIntake, terraIntake, ichorIntake, waterIntake, gloamValue, terraValue, ichorValue, waterValue;
    public Slider gloamFill, terraFill, ichorFill, waterFill, gloamFillD, terraFillD, ichorFillD, waterFillD;
    string growthString;
    ControlManager controlManager;
    //For Lerps
    public Transform lerpStart, lerpEnd, lerpEndD, cropUITransform, cropUITransformD;
    float timeSpendAnimating = 0;
    float moveProgress = 0;
    float maxMoveProgress = 0.5f;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }
    void Start()
    {
        cropUITransform.position = lerpStart.position;
        cropUITransformD.position = lerpStart.position;
        growthStageNumber.text = "";
        growthStageNumberD.text = "";
        mainCam = Camera.main;
        StartCoroutine(CheckTimer());
    }

    void Update()
    {
        if(controlManager.moreInfo.action.IsPressed())
        {
            cropStats.SetActive(false);
            cropStatsDetailed.SetActive(true);
        }
        else
        {
            cropStats.SetActive(true);
            cropStatsDetailed.SetActive(false);
        }

        if(isActive && moveProgress < maxMoveProgress)
        {
            moveProgress += Time.deltaTime;
            cropUITransform.position = Vector3.Lerp(lerpStart.position, lerpEnd.position, moveProgress/maxMoveProgress);
            cropUITransformD.position = Vector3.Lerp(lerpStart.position, lerpEndD.position, moveProgress/maxMoveProgress);
        }

        if(!isActive && moveProgress > 0)
        {
            moveProgress -= Time.deltaTime;
            cropUITransform.position = Vector3.Lerp(lerpStart.position, lerpEnd.position, moveProgress/maxMoveProgress);
            cropUITransformD.position = Vector3.Lerp(lerpStart.position, lerpEndD.position, moveProgress/maxMoveProgress);
        }
    }

    IEnumerator CheckTimer()
    {
        do
        {
            FarmlandCheck();
            yield return new WaitForSeconds(0.2f);
        }
        while(gameObject.activeSelf);
    }
    void FarmlandCheck()
    {
        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, fwd, out hit, reach, 1 << 6))
        {
            if (hit.collider.gameObject.tag == "FarmLand")
            {
                hitCrop = hit.collider.GetComponent<FarmLand>();

                if(hitCrop.growthStage < 0 || !hitCrop.crop)
                {
                    isActive = true;
                    cropNameText.text = "Farmland";
                    cropNameTextD.text = "Farmland";
                    
                }
                else
                {
                    isActive = true;
                    cropNameText.text = hitCrop.crop.name;
                    cropNameTextD.text = hitCrop.crop.name;
                }
                FarmlandStatUpdate(hitCrop);
            }
            else
            {
                isActive = false;
            }
        }
        else
        {
            isActive = false;
        }
    }

    void FarmlandStatUpdate(FarmLand tile)
    {
        NutrientStorage tileNutrients = tile.GetCropStats();
        if (tileNutrients == null) { return; }

        gloamFill.value = tileNutrients.gloamLevel / 10;
        terraFill.value = tileNutrients.terraLevel / 10;
        ichorFill.value = tileNutrients.ichorLevel / 10;
        waterFill.value = tileNutrients.waterLevel / 10;

        gloamFillD.value = tileNutrients.gloamLevel / 10;
        terraFillD.value = tileNutrients.terraLevel / 10;
        ichorFillD.value = tileNutrients.ichorLevel / 10;
        waterFillD.value = tileNutrients.waterLevel / 10;

        gloamValue.text = tileNutrients.gloamLevel + "/10";
        terraValue.text = tileNutrients.terraLevel + "/10";
        ichorValue.text = tileNutrients.ichorLevel + "/10";
        waterValue.text = tileNutrients.waterLevel + "/10";

        if(tile.crop != null)
        {
            if(tile.crop.gloamIntake > 0)
            {
                gloamIntake.text = (-1 * tile.crop.gloamIntake).ToString();
                gloamArrow.gameObject.transform.rotation = Quaternion.Euler(0f,0f,0f);
                gloamArrow.color = c_lowering;
                gloamArrow.gameObject.SetActive(true);
            }
            else if(tile.crop.gloamIntake < 0)
            {
                gloamIntake.text = "+" + -tile.crop.gloamIntake;
                gloamArrow.gameObject.transform.rotation = Quaternion.Euler(0f,0f,180f);
                gloamArrow.color = c_rising;
                gloamArrow.gameObject.SetActive(true);
            }
            else
            {
                gloamIntake.text = "";
                gloamArrow.gameObject.SetActive(false);
            }
            

            if(tile.crop.terraIntake > 0)
            {
                terraIntake.text = (-1 * tile.crop.terraIntake).ToString();
                terraArrow.gameObject.transform.rotation = Quaternion.Euler(0f,0f,0f);
                terraArrow.color = c_lowering;
                terraArrow.gameObject.SetActive(true);
            }
            else if(tile.crop.terraIntake < 0)
            {
                terraIntake.text = "+" + -tile.crop.terraIntake;
                terraArrow.gameObject.transform.rotation = Quaternion.Euler(0f,0f,180f);
                terraArrow.color = c_rising;
                terraArrow.gameObject.SetActive(true);
            }
            else
            {
                terraIntake.text = "";
                terraArrow.gameObject.SetActive(false);
            }

            if(tile.crop.ichorIntake > 0)
            {
                ichorIntake.text = (-1 * tile.crop.ichorIntake).ToString();
                ichorArrow.gameObject.transform.rotation = Quaternion.Euler(0f,0f,0f);
                ichorArrow.color = c_lowering;
                ichorArrow.gameObject.SetActive(true);
            }
            else if(tile.crop.ichorIntake < 0)
            {
                ichorIntake.text = "+" + -tile.crop.ichorIntake;
                ichorArrow.gameObject.transform.rotation = Quaternion.Euler(0f,0f,180f);
                ichorArrow.color = c_rising;
                ichorArrow.gameObject.SetActive(true);
            }
            else
            {
                ichorIntake.text = "";
                ichorArrow.gameObject.SetActive(false);
            }

            if(tile.crop.waterIntake > 0)
            {
                waterIntake.text = (-1 * tile.crop.waterIntake).ToString();
                waterArrow.color = c_lowering;
                waterArrow.gameObject.SetActive(true);
            }
            else if(tile.crop.waterIntake < 0)
            {
                waterIntake.text = "";
                waterArrow.gameObject.SetActive(false);
            }
            else
            {
                waterIntake.text = "";
                waterArrow.gameObject.SetActive(false);
            }
        }
        else
        {
            gloamIntake.text = "";
            gloamArrow.gameObject.SetActive(false);
            terraIntake.text = "";
            terraArrow.gameObject.SetActive(false);
            ichorIntake.text = "";
            ichorArrow.gameObject.SetActive(false);
            waterIntake.text = "";
            waterArrow.gameObject.SetActive(false);
        }

        //print(ichorFill.fillAmount);

        if(tile.isWeed == false && tile.crop && !tile.rotted)
        {
            growthString = "Stage: " + tile.growthStage + "/" + tile.crop.growthStages;
            growthStageNumberD.text = growthString;
            growthStageNumber.text = growthString;
            cropSprite.sprite = tile.crop.cropYield.icon;
            cropSprite.gameObject.SetActive(true);
            cropSpriteD.sprite = tile.crop.cropYield.icon;
            cropSpriteD.gameObject.SetActive(true);
        } 
        else if(tile.rotted && tile.crop)
        {
            growthString = "Rotten";
            growthStageNumberD.text = growthString;
            growthStageNumber.text = growthString;
            cropSprite.sprite = tile.crop.cropYield.icon;
            cropSprite.gameObject.SetActive(true);
            cropSpriteD.sprite = tile.crop.cropYield.icon;
            cropSpriteD.gameObject.SetActive(true);
        }
        else if(tile.crop == null)
        {
            growthStageNumberD.text = "";
            growthStageNumber.text = "";
            cropSprite.gameObject.SetActive(false);
            cropSpriteD.gameObject.SetActive(false);
        }
        else //Weeds
        {
            growthStageNumberD.text = "";
            growthStageNumber.text = "";
            cropSprite.gameObject.SetActive(true);
            cropSprite.sprite = tile.cropRenderer.sprite;
            cropSpriteD.gameObject.SetActive(true);
            cropSpriteD.sprite = tile.cropRenderer.sprite;
        }
    }
}
