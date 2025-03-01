using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CropNeedsUI : MonoBehaviour
{
    private FarmLand farmLand;
    private CropData cropData;
    private NutrientStorage nutrients;
    public GameObject gloam, terra, ichor, water, background, canvas;
    ControlManager controlManager;
    private bool isDetailed;


    // Start is called before the first frame update
    void Start()
    {
        farmLand = GetComponent<FarmLand>();
        nutrients = farmLand.GetCropStats();
        DisableStats();
        controlManager = FindObjectOfType<ControlManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNeedsUI();
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

        nutrients = farmLand.GetCropStats();
        cropData = farmLand.crop;

        if(nutrients.gloamLevel < cropData.gloamIntake) gloam.SetActive(true);
        else gloam.SetActive(false);

        if(nutrients.terraLevel < cropData.terraIntake) terra.SetActive(true);
        else terra.SetActive(false);

        if(nutrients.ichorLevel < cropData.ichorIntake) ichor.SetActive(true);
        else ichor.SetActive(false);

        if(nutrients.waterLevel < cropData.waterIntake) water.SetActive(true);
        else water.SetActive(false);

        //canvas.SetActive(UICropStats.isDetailed);

        if(!gloam.activeSelf && !terra.activeSelf && !ichor.activeSelf && !water.activeSelf) {background.SetActive(false);}
        else background.SetActive(true);

    }

    private void DisableStats()
    {
        gloam.SetActive(false);
        terra.SetActive(false);
        ichor.SetActive(false);
        water.SetActive(false);
        background.SetActive(false);
    }
}
