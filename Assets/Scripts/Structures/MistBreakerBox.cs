using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MistBreakerBox : StructureBehaviorScript
{

    public GameObject cropTile;

    public CropData mistBreaker;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        //fire.SetActive(false);
        if(absentFromGrid) return;
        NightSpawningManager.Instance.boxPlaced = true;
        GameSaveData.Instance.playerHasBox = true;
    }

    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);
        if (addedSuccessfully)
        {
            GameSaveData.Instance.playerHasBox = true;
            NightSpawningManager.Instance.boxPlaced = false;
            Destroy(this.gameObject);
        }
    }

    public override void HourPassed()
    {
        if(TimeManager.Instance.currentHour == 20 && !absentFromGrid)
        {
            clearTileOnDestroy = false;
            FarmLand script = Instantiate(cropTile, transform.position, Quaternion.identity).GetComponent<FarmLand>();
            GameSaveData.Instance.playerHasBox = false;
            script.InsertCrop(mistBreaker);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;
    }
}
