using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MistBreakerBox : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;

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
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);
        if (addedSuccessfully)
        {
            GameSaveData.Instance.playerHasBox = true;
            Destroy(this.gameObject);
        }
    }

    public override void HourPassed()
    {
        if(TimeManager.Instance.currentHour == 20 && !absentFromGrid)
        {
            clearTileOnDestroy = false;
            FarmLand script = Instantiate(cropTile, transform.position, Quaternion.identity).GetComponent<FarmLand>();
            script.InsertCrop(mistBreaker);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;
        if(TimeManager.Instance.currentHour != 20 && !absentFromGrid)
        {
            NightSpawningManager.Instance.boxPlaced = false;
            GameSaveData.Instance.playerHasBox = false;
        }
    }
}
