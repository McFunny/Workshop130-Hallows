using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UICropStats : MonoBehaviour
{
    public Camera mainCam;
    public GameObject cropStatsObject, growthStageText, cropStatsObjectD, growthStageTextD;
    public TextMeshProUGUI cropNameText, gloamAmount, terraAmount, ichorAmount, waterAmount, growthStageNumber;
    public TextMeshProUGUI cropNameTextD, gloamAmountD, terraAmountD, ichorAmountD, waterAmountD, growthStageNumberD;
    public TextMeshProUGUI gloamNumber, terraNumber, ichorNumber, waterNumber;
    public float reach = 5;
    private FarmLand hitCrop;
    private string cropName = "Crop Name";
    string growthString;
    public float wlHigh, wlMedium; //Water Level Values
    public float nHigh, nMedium;
    [SerializeField] private bool isDetailed, isActive;


    public Image gloamBG, terraBG, ichorBG;
    public Image gloamBGD, terraBGD, ichorBGD;
    public Color c_default, c_rising, c_lowering;

    // Start is called before the first frame update
    void Start()
    {
        growthStageNumberD.text = "";
        growthStageNumber.text = "";
        if(!mainCam) mainCam = FindObjectOfType<Camera>();
        StartCoroutine(CheckTimer());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButton("Tab"))
        {
            isDetailed = true;
        }
        else
        {
            isDetailed = false;
        }

        if(isDetailed && isActive)
        {
            cropStatsObjectD.SetActive(true);
            cropStatsObject.SetActive(false);
            growthStageTextD.SetActive(true);
            growthStageText.SetActive(false);
        }
        else if(isActive)
        {
            cropStatsObjectD.SetActive(false);
            cropStatsObject.SetActive(true);
            growthStageTextD.SetActive(false);
            growthStageText.SetActive(true);
        }
        else
        {
            cropStatsObjectD.SetActive(false);
            cropStatsObject.SetActive(false);
            growthStageTextD.SetActive(false);
            growthStageText.SetActive(false);
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
                //cropStatsObject.SetActive(true);
                hitCrop = hit.collider.GetComponent<FarmLand>();

                if(hitCrop.growthStage < 0 || !hitCrop.crop)
                {
                    isActive = true;
                    //growthStageText.SetActive(false);
                    cropNameText.text = "Farmland";
                    
                }
                else
                {
                    isActive = true;
                    //growthStageText.SetActive(true);
                    cropNameText.text = hitCrop.crop.name;
                }
                FarmlandStatUpdate(hitCrop);
            }
            else
            {
                isActive = false;
                //cropStatsObject.SetActive(false);
            }
        }
        else
        {
            isActive = false;
            //cropStatsObject.SetActive(false);
        }
    }

    void FarmlandStatUpdate(FarmLand tile) //Cam don't look at this. //I looked at it // STOPPPP ITTTTT!!!!
    {
        NutrientStorage tileNutrients = tile.GetCropStats();
            // I know this is a mess but I really don't feel like rewriting this script. If I have to write another if statement I WILL go insane.

            //Gloam Level Check I feel so gloaming
            if(tileNutrients.gloamLevel >= nHigh)
            {
                gloamAmountD.text = "High";
                gloamAmount.text = "High";
            }
            else if(tileNutrients.gloamLevel >= nMedium)
            {
                gloamAmountD.text = "Medium";
                gloamAmount.text = "Medium";
            }
            else if(tileNutrients.gloamLevel == 0)
            {
                gloamAmountD.text = "Depleted";
                gloamAmount.text = "Depleted";
            }
            else
            {
                gloamAmountD.text = "Low";
                gloamAmount.text = "Low";
            }
            gloamNumber.text = tileNutrients.gloamLevel.ToString() + "/ 10";

            //Terra Level Check
            if(tileNutrients.terraLevel >= nHigh)
            {
                terraAmountD.text = "High";
                terraAmount.text = "High";
                
            }
            else if(tileNutrients.terraLevel >= nMedium)
            {
                terraAmountD.text = "Medium";
                terraAmount.text = "Medium";
            }
            else if(tileNutrients.terraLevel == 0)
            {
                terraAmountD.text = "Depleted";
                terraAmount.text = "Depleted";
            }
            else
            {
                terraAmountD.text = "Low";
                terraAmount.text = "Low";
            }
            terraNumber.text = tileNutrients.terraLevel.ToString() + "/ 10";

            //Ichor Level Check idk if it's actually called ichor but that's what it says in the structure manager script so that's what I'm going with
            if(tileNutrients.ichorLevel >= nHigh)
            {
                ichorAmountD.text = "High";
                ichorAmount.text = "High";
            }
            else if(tileNutrients.ichorLevel >= nMedium)
            {
                ichorAmountD.text = "Medium";
                ichorAmount.text = "Medium";
            }
            else if(tileNutrients.ichorLevel == 0)
            {
                ichorAmountD.text = "Depleted";
                ichorAmount.text = "Depleted";
            }
            else
            {
                ichorAmountD.text = "Low";
                ichorAmount.text = "Low";
            }
            ichorNumber.text = tileNutrients.ichorLevel.ToString() + "/ 10";

            //Water Level Check
            if(tileNutrients.waterLevel >= wlHigh)
            {
                waterAmountD.text = "High";
                waterAmount.text = "High";
            }
            else if(tileNutrients.waterLevel >= wlMedium)
            {
                waterAmountD.text = "Medium";
                waterAmount.text = "Medium";
            }
            else if(tileNutrients.waterLevel == 0)
            {
                waterAmountD.text = "Drained";
                waterAmount.text = "Drained";
            }
            else
            {
                waterAmountD.text = "Low";
                waterAmount.text = "Low";
            }
            waterNumber.text = tileNutrients.waterLevel.ToString() + "/ 5";

            //Growth Stage Check
            if(tile.isWeed == false && tile.crop)
            {
                growthString = "Stage: " + tile.growthStage + "/" + tile.crop.growthStages;
                growthStageNumberD.text = growthString;
                growthStageNumber.text = growthString;
            } 
            else 
            {
                growthStageNumberD.text = "";
                growthStageNumber.text = "";
            }

            if(!tile.crop)
            {
                gloamBGD.color = c_default;
                terraBGD.color = c_default;
                ichorBGD.color = c_default;
                gloamBG.color = c_default;
                terraBG.color = c_default;
                ichorBG.color = c_default;
                return;
            } 

            if(tile.crop.gloamIntake > 0) 
            {
                gloamBGD.color = c_lowering;
                gloamBG.color = c_lowering;
            }
            else if(tile.crop.gloamIntake < 0)
            {
                gloamBGD.color = c_rising;
                gloamBG.color = c_rising;
            } 
            else 
            {
                gloamBGD.color = c_default;
                gloamBG.color = c_default;
            }

            if(tile.crop.terraIntake > 0) 
            {
                terraBGD.color = c_lowering;
                terraBG.color = c_lowering;
            }
            else if(tile.crop.terraIntake < 0) 
            {
                terraBGD.color = c_rising;
                terraBG.color = c_rising;
            }
            else 
            {
                terraBGD.color = c_default;
                terraBG.color = c_default;
            }

            if(tile.crop.ichorIntake > 0) 
            {
                ichorBGD.color = c_lowering;
                ichorBG.color = c_lowering;
            }
            else if(tile.crop.ichorIntake < 0) 
            {
                ichorBGD.color = c_rising;
                ichorBG.color = c_rising;
            }
            else 
            {
                ichorBGD.color = c_default;
                ichorBG.color = c_default;
            }
    }
}
