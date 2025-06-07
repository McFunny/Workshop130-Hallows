using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class CropNeedsUI : MonoBehaviour
{
    private FarmLand farmLand;
    private CropData cropData;
    private NutrientStorage nutrients;
    public GameObject gloam, terra, ichor, water, rot, pollen, background, canvas;
    public Image gloamRed, terraRed, ichorRed, waterRed;
    ControlManager controlManager;
    private bool isDetailed;


    // Start is called before the first frame update
    void Start()
    {
        farmLand = GetComponent<FarmLand>();
        nutrients = farmLand.GetCropStats();
        DisableStats();
        controlManager = FindObjectOfType<ControlManager>();

        StartCoroutine(OneSecondTimer());
    }

    // Update is called once per frame
    /*void Update()
    {
        UpdateNeedsUI();
    }*/

    IEnumerator OneSecondTimer()
    {
        while(true)
        {
            UpdateNeedsUI();
            yield return new WaitForSeconds(0.4f);
        }
    }

    public void UpdateNeedsUI()
    {
        //if (farmLand.crop)
        if (farmLand.crop == null) 
        {
            DisableStats();
            return;
        }
        if (farmLand.isWeed)
        {
            DisableStats();
            return;
        }

        if(farmLand.growthStage == farmLand.crop.growthStages)
        {
            DisableStats();
            return;
        }

        if(farmLand.rotted)
        {
            rot.SetActive(true);
            Rotten();
            return;
        }
        else {rot.SetActive(false);}

        nutrients = farmLand.GetCropStats();
        cropData = farmLand.crop;

        if(!gloam.activeSelf && !terra.activeSelf && !ichor.activeSelf && !water.activeSelf && !rot.activeSelf && !pollen.activeSelf) {background.SetActive(false);}
        else background.SetActive(true);

        //print("Are we even getting here???");

        if(nutrients.gloamLevel < cropData.gloamIntake) gloam.SetActive(true);
        else gloam.SetActive(false);

        if(nutrients.terraLevel < cropData.terraIntake) terra.SetActive(true);
        else terra.SetActive(false);

        if(nutrients.ichorLevel < cropData.ichorIntake) ichor.SetActive(true);
        else ichor.SetActive(false);

        if(nutrients.waterLevel < cropData.waterIntake) water.SetActive(true);
        else water.SetActive(false);

        if(farmLand.NeedsPollination()) pollen.SetActive(true);
        else {pollen.SetActive(false);}

        if(farmLand.hoursSpent == farmLand.crop.hoursPerStage - 1)
        {
            gloamRed.enabled = true;
            terraRed.enabled = true;
            ichorRed.enabled = true;
            waterRed.enabled = true;
        }
        else
        {
            gloamRed.enabled = false;
            terraRed.enabled = false;
            ichorRed.enabled = false;
            waterRed.enabled = false;
        }

        //canvas.SetActive(UICropStats.isDetailed);

    }

    private void DisableStats()
    {
        gloam.SetActive(false);
        terra.SetActive(false);
        ichor.SetActive(false);
        water.SetActive(false);
        rot.SetActive(false);
        background.SetActive(false);
    }

    private void Rotten()
    {
        gloam.SetActive(false);
        terra.SetActive(false);
        ichor.SetActive(false);
        water.SetActive(false);
        //background.SetActive(false);
    }
}
