using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UICropStats : MonoBehaviour
{
    public Camera mainCam;
    public GameObject cropStatsObject, growthStageText;
    public TextMeshProUGUI cropNameText, gloamAmount, terraAmount, ichorAmount, waterAmount, growthStageNumber;
    public float reach = 5;
    private FarmLand hitCrop;
    private string cropName = "Crop Name";
    public float wlHigh, wlMedium; //Water Level Values
    public float nHigh, nMedium;

    public Image gloamBG, terraBG, ichorBG;

    // Start is called before the first frame update
    void Start()
    {
        if(!mainCam) mainCam = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        FarmlandCheck();
    }

    public void FarmlandCheck()
    {
        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, fwd, out hit, reach, 1 << 6))
        {
            if (hit.collider.gameObject.tag == "FarmLand")
            {
                cropStatsObject.SetActive(true);
                hitCrop = hit.collider.GetComponent<FarmLand>();
                //print(hitCrop.cropStats.waterLevel);
                if(hitCrop.growthStage < 0 || !hitCrop.crop)
                {
                    growthStageText.SetActive(false);
                    cropNameText.text = "Farmland";
                }
                else
                {
                    growthStageText.SetActive(true);
                    cropNameText.text = hitCrop.crop.name;
                }
                FarmlandStatUpdate(hitCrop);
            }
            else
            {
                cropStatsObject.SetActive(false);
            }
        }
        else
        {
            cropStatsObject.SetActive(false);
        }
    }

    void FarmlandStatUpdate(FarmLand tile) //Cam don't look at this. //I looked at it
    {
        //Gloam Level Check I feel so gloaming
        if(tile.cropStats.gloamLevel >= nHigh)
        {
            gloamAmount.text = "High";
        }
        else if(tile.cropStats.gloamLevel >= nMedium)
        {
            gloamAmount.text = "Medium";
        }
        else if(tile.cropStats.gloamLevel == 0)
        {
            gloamAmount.text = "Depleted";
        }
        else
        {
            gloamAmount.text = "Low";
        }

        //Terra Level Check
        if(tile.cropStats.terraLevel >= nHigh)
        {
            terraAmount.text = "High";
        }
        else if(tile.cropStats.terraLevel >= nMedium)
        {
            terraAmount.text = "Medium";
        }
        else if(tile.cropStats.terraLevel == 0)
        {
            terraAmount.text = "Depleted";
        }
        else
        {
            terraAmount.text = "Low";
        }

        //Ichor Level Check idk if it's actually called ichor but that's what it says in the structure manager script so that's what I'm going with
        if(tile.cropStats.ichorLevel >= nHigh)
        {
            ichorAmount.text = "High";
        }
        else if(tile.cropStats.ichorLevel >= nMedium)
        {
            ichorAmount.text = "Medium";
        }
        else if(tile.cropStats.ichorLevel == 0)
        {
            ichorAmount.text = "Depleted";
        }
        else
        {
            ichorAmount.text = "Low";
        }

        //Water Level Check
        if(tile.cropStats.waterLevel >= wlHigh)
        {
            waterAmount.text = "High";
        }
        else if(tile.cropStats.waterLevel >= wlMedium)
        {
            waterAmount.text = "Medium";
        }
        else if(tile.cropStats.waterLevel == 0)
        {
            waterAmount.text = "Drained";
        }
        else
        {
            waterAmount.text = "Low";
        }

        //Growth Stage Check
        if(tile.isWeed == false && tile.crop)
        {
            string growthString = "Stage: " + tile.growthStage + "/" + tile.crop.growthStages;
            growthStageNumber.text = growthString;
        } 
        else growthStageNumber.text = "";
    }
}
