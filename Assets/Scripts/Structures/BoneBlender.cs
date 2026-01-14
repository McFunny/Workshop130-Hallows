using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneBlender : StructureBehaviorScript
{
    public Transform itemDropTransform, itemInsertPos;

    public InventoryItemData bones, glue;

    public PopupScript itemWarning; //Warning that the player does not have enough items

    public int progress = 0;
    int maxProgress = 3;

    int itemsNeeded = 4;

    bool ignoreNextHour = false;

    public ParticleSystem fumes, completedParticles;
    public Animator anim;

    public AudioSource loopSource;

    void Start()
    {
        base.Start();
        if(StructureManager.Instance.ValidateGridType(transform.position, GridType.Farm) == false) maxProgress *= 3; //Takes longer when not on the farm
    }

    public override void StructureInteraction()
    {

        if(progress < maxProgress || savedItems.Count == 0) return;

        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(glue);
        droppedItem.transform.position = itemDropTransform.position;

        Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
        itemRB = droppedItem.GetComponent<Rigidbody>();
        itemRB.AddForce(Vector3.forward * 20);
        itemRB.AddForce(Vector3.up * 10);

        ParticlePoolManager.Instance.GrabCloudParticle().transform.position = itemDropTransform.position;

        savedItems.Clear();

        audioHandler.PlaySound(audioHandler.activatedSound);

        fumes.Stop();
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsFinished", false);
        progress = 0;
        completedParticles.Stop();

    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item == bones && savedItems.Count == 0)
        {

            if(HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize < itemsNeeded) //Not enough items
            {
                PopupHandler.Instance.AddToQueue(itemWarning);
                return;
            }
            for(int i = 0; i < itemsNeeded; i++)
            {
                savedItems.Add(item);
            }
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(itemsNeeded);
            PlayerInventoryHolder.Instance.UpdateInventory();

            audioHandler.PlaySound(audioHandler.itemInteractSound);

            ParticlePoolManager.Instance.GrabCloudParticle().transform.position = itemInsertPos.position;
            fumes.Play();
            anim.SetBool("IsRunning", true);
            loopSource.Play();
            ignoreNextHour = true;
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
    }

    public override void HourPassed()
    {
        if(progress < maxProgress && savedItems.Count > 0)
        {
            if(ignoreNextHour)
            {
                ignoreNextHour = false;
                return;
            }
            progress++;

            if(progress >= maxProgress)
            {
                progress = maxProgress;
                fumes.Stop();
                anim.SetBool("IsFinished", true);
                loopSource.Stop();
                completedParticles.Play();
            }
        }
        else 
        {
            fumes.Stop();
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop items
        GameObject droppedItem;
        foreach(InventoryItemData item in savedItems)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(item);
            droppedItem.transform.position = itemDropTransform.position;
        }
    }

    public override void LoadVariables()
    {
        progress = saveInt1;

        if(progress < maxProgress && savedItems.Count > 0) 
        {
            fumes.Play();
            anim.SetBool("IsRunning", true);
        }

        if(progress == maxProgress)
        {
            anim.SetBool("IsFinished", true);
            completedParticles.Play();
        }
    }

    public override void SaveVariables()
    {
        saveInt1 = progress;
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        structureUIVariables.valueGroups[1].value = progress;
        structureUIVariables.valueGroups[1].maxValue = maxProgress;

        return structureUIVariables.valueGroups;
    }
}
