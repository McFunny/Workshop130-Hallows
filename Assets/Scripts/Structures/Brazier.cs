using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brazier : StructureBehaviorScript, IFireHolder
{
    //public InventoryItemData recoveredItem;

    public FireFearTrigger fireTrigger;
    public GameObject fire;

    public int flameLeft; //if 0, fire is gone
    int maxFlame = 20; //How many hours it lasts

    bool isBurning;

    public List<RepairItem> fuelItems;

    public PopupScript needWoodP;

    public GameObject woodObject;

    //Rework to incorporate a fuel based system rather than static time.

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        //fireTrigger.OnScare += EnemyScaredByFire;
        //StartCoroutine(FireDrain());
        //flameLeft = 0;
        fire.SetActive(false);
        UpdateModel();
    }

    void Update()
    {
        base.Update();

        if(fire.activeSelf == true && flameLeft <= 0) ExtinguishFlame();
    }

    public override void StructureInteraction()
    {
        return;
        if(flameLeft == 0)
        {
            flameLeft = maxFlame;
            fire.SetActive(true);
            audioHandler.PlaySound(audioHandler.activatedSound);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        print("Interacted");
        if(type == ToolType.Torch)
        {
            print("Torch");
            if(PlayerInteraction.Instance.torchLit && !isBurning)
            {
                if(flameLeft == 0)
                {
                    PopupHandler.Instance.AddToQueue(needWoodP);
                    success = false;
                    return;
                }
                isBurning = true;
                fire.SetActive(true);
                audioHandler.PlaySound(audioHandler.activatedSound);
                success = true;
            }
            else if(isBurning && !PlayerInteraction.Instance.torchLit)
            {
                HandItemManager.Instance.TorchFlameToggle(true);
                success = true;
            }
            else success = false;
            return;
        }
        else if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
        else if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && isBurning)
        {
            PlayerInteraction.Instance.WaterChange(-1);
            HitWithWater();
            success = true;
        }
        else if (type == ToolType.Pyrefly && isBurning && !PlayerInteraction.Instance.pyreflyLit)
        {
            HandItemManager.Instance.PyreflyFlameToggle(true);
            success = true;
        }
        else success = false;
        
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(flameLeft >= maxFlame) return;
        foreach(RepairItem r in fuelItems)
        {
            if(r.item == item)
            {
                if(maxFlame <= r.repairAmount + flameLeft) flameLeft = maxFlame;
                else flameLeft += r.repairAmount;
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();
                UpdateModel();
                audioHandler.PlaySound(audioHandler.itemInteractSound);
                return;
            }
        }
    }

    void UpdateModel()
    {
        if(flameLeft > 0) woodObject.SetActive(true);
        else woodObject.SetActive(false);

        if(isBurning) fire.SetActive(true);
        else fire.SetActive(false);
    }

    public override void HitWithWater()
    {
        ExtinguishFlame();
    }

    /*IEnumerator DugUp()
    {
        yield return  new WaitForSeconds(1);
        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
        droppedItem.transform.position = transform.position;
        Destroy(this.gameObject);
        
    }*/

    public override void HourPassed()
    {
        if(isBurning)
        {
            flameLeft--;

            if(flameLeft == 0 && fire.activeSelf)
            {
                ExtinguishFlame();
            }
        }
    }

    /*IEnumerator FireDrain() //Disabled
    {
        int r;
        while(gameObject.activeSelf)
        {
            r = Random.Range(10, 20);
            yield return new WaitForSeconds(r);
            flameLeft -= 1;
            if(flameLeft < 0) flameLeft = 0;
            if(flameLeft == 0 && fire.activeSelf)
            {
                ExtinguishFlame();
            }
        }
    }
    */

    void ExtinguishFlame()
    {
        UpdateModel();
        if(!isBurning) return;
        isBurning = false;
        ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = fire.transform.position;
        fire.SetActive(false);
        audioHandler.PlaySound(audioHandler.miscSounds1[0]);
    }

    void OnDestroy()
    {
        //fireTrigger.OnScare -= EnemyScaredByFire;
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        if(flameLeft == 0 || Random.Range(0,30) < flameLeft) return;
        GameObject droppedItem;
        foreach(InventoryItemData item in savedItems)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(fuelItems[0].item);
            droppedItem.transform.position = transform.position;
        }
    }

    /*void EnemyScaredByFire(bool successful)
    {
        if(flameLeft <= 0 || !successful) return;
        flameLeft -= Random.Range(1,3);
        if(flameLeft <= 0) ExtinguishFlame();
    }*/
    
    public override void LoadVariables()
    {
        flameLeft = saveInt1;
        UpdateModel();
    }

    public override void SaveVariables()
    {
        saveInt1 = flameLeft;
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        structureUIVariables.valueGroups[1].value = flameLeft;
        structureUIVariables.valueGroups[1].maxValue = maxFlame;

        return structureUIVariables.valueGroups;
    }

    public bool CanBeExtinguished()
    {
        if(!isBurning) return false;
        else return true;
    }

    public void ExternalExtinguish()
    {
        ExtinguishFlame();
    }
}
