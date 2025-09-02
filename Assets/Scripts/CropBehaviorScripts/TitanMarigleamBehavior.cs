using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Titan Marigleam")]
public class TitanMarigleamBehavior : CropBehavior
{
    public StructureObject farmTile;
    public CropData node;
    public int nodesToPlant = 8;

    public override void OnHour(FarmLand tile)
    {
        if(TimeManager.Instance.isDay == true && TimeManager.Instance.currentHour != 6 && tile.growthStage != 1) //Dies at morning, but not when its just planted
        {
            tile.CropDied();
        }
    }

    public override void OnCropAwake(FarmLand tile)
    {
        GameSaveData.Instance.siegeCropInHand = false;
        if(!TimeManager.Instance.isDay)
        {
            tile.CropDied();
            return;
        }
        if(SiegeManager.Instance) SiegeManager.Instance.siegeCropOnFarm = true;
    }

    public override void OnPlanted(FarmLand tile)
    {
        //Plant the nodes
        List<Vector3> openTiles = StructureManager.Instance.GetNearbyClearTiles(tile.transform.position, 25);
        int nodesPlanted = 0;

        while(nodesPlanted < nodesToPlant && openTiles.Count > 0)
        {
            Vector3 chosenPos = openTiles[Random.Range(0, openTiles.Count)];
            Instantiate(farmTile.objectPrefab, chosenPos, Quaternion.identity).GetComponentInParent<FarmLand>().InsertCrop(node);
            openTiles.Remove(chosenPos);
            nodesPlanted++;
        }
        int stressDamage = (nodesPlanted - nodesToPlant) * -1;
        if(stressDamage > 0) tile.TakeStressDamage(stressDamage);
    }

    public override void OnCropDestroyed(FarmLand tile)
    {
        if(SiegeManager.Instance) SiegeManager.Instance.siegeCropOnFarm = false;

        KillNodes(tile.transform.position);
        //Maybe redrop the seed if it wasnt harvested?
    }

    public override void OnHarvest(FarmLand tile, bool usedShovel, bool usedScythe)
    {
        SiegeManager.Instance.siegeCropOnFarm = false;

        KillNodes(tile.transform.position);

        ///////MOVE THIS CODE TO THE PEDASTAL SCRIPT SO ITS WHEN THE ITEM IS SOCKETED, THEN THIS HAPPENS. ALSO MAKE THESE NO LONGER A KEY ITEM SINCE MORE CAN BE GAINED
        /*GameSaveData.Instance.siegesCleared++;
        switch(GameSaveData.Instance.siegesCleared)
        {
            case 1:
            QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[9]);
            QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[10]);
            break;
            case 2:
            QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[10]);
            QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[11]);
            break;
            case 3:
            QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[11]);
            QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[12]);
            break;
            case 4:
            QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[12]);
            break;
            default:
            break;
        }*/
        ////////
    }

    public override bool CanDig(FarmLand tile)
    {
        if(tile.growthStage == 1) return false;
        return true;
    }

    void KillNodes(Vector3 pos)
    {
        Collider[] hitStructures = Physics.OverlapSphere(pos, 80f, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            FarmLand nodeTile = collider.gameObject.GetComponentInParent<FarmLand>();
            if(nodeTile && nodeTile.crop && nodeTile.crop == node) nodeTile.TakeStressDamage(99);
        }
    }
}
