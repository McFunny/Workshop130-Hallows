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
        NightSpawningManager.Instance.boxPlaced = true;
    }

    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {
        return;
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }
    }

    public override void HourPassed()
    {
        if(TimeManager.Instance.currentHour == 20)
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
        if(TimeManager.Instance.currentHour != 20) NightSpawningManager.Instance.boxPlaced = false;
        //if (!gameObject.scene.isLoaded) return; 
    }
}
