using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FermentationVat : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;

    public Transform itemDropTransform;

    public ParticleSystem activatedParticles, completedParticles;

    public int progress = 0;
    int maxProgress = 18;
    int maxContainedItems = 1;

    bool ignoreNextHour = false;
    bool playingActiveParticles = false;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        ParticleToggle();
    }


    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {
        if(progress < maxProgress && savedItems.Count > 0) return; //smth is hangin

        if(progress >= maxProgress)
        {
            progress = 0;

            //get item
            GameObject droppedItem;
            float r = Random.Range(0,10);
            droppedItem = ItemPoolManager.Instance.GrabItem(savedItems[0].pickledForm);
            droppedItem.transform.position = itemDropTransform.position;

            Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(Vector3.forward * 20);
            itemRB.AddForce(Vector3.up * 10);
            //audioHandler.PlaySound(audioHandler.activatedSound);

            GameObject poofParticle;
            poofParticle = ParticlePoolManager.Instance.GrabCloudParticle();
            poofParticle.transform.position = itemDropTransform.position;

            savedItems.Clear();

            ParticleToggle();

        }
    }


    public override void ItemInteraction(InventoryItemData item)
    {
        if(item.pickledForm && savedItems.Count < maxContainedItems)
        {
            //
            savedItems.Add(item);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            //audioHandler.PlaySound(audioHandler.activatedSound);

            /*GameObject poofParticle;
            poofParticle = ParticlePoolManager.Instance.GrabCloudParticle();
            poofParticle.transform.position = itemDropTransform.position;*/

            ParticleToggle();

            ignoreNextHour = true;

        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            StartCoroutine(DugUp());
            success = true;
        }
    }

    public override void HourPassed()
    {
        if(progress < maxProgress && savedItems.Count == maxContainedItems)
        {
            if(ignoreNextHour)
            {
                ignoreNextHour = false;
                return;
            }
            progress++;
        }
        ParticleToggle();
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
        droppedItem.transform.position = transform.position;

        Destroy(this.gameObject);
    }

    void ParticleToggle()
    {
        if(!playingActiveParticles && progress < maxProgress && savedItems.Count > 0)
        {
            playingActiveParticles = true;
            activatedParticles.Play();
            return;
        }

        if(progress >= maxProgress && savedItems.Count > 0)
        {
            playingActiveParticles = false;
            activatedParticles.Stop();
            completedParticles.Play();
        }

        if(savedItems.Count == 0)
        {
            playingActiveParticles = false;
            activatedParticles.Stop();
            completedParticles.Stop();
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
            if(progress >= maxProgress) droppedItem = ItemPoolManager.Instance.GrabItem(item.pickledForm);
            else droppedItem = ItemPoolManager.Instance.GrabItem(item);
            droppedItem.transform.position = itemDropTransform.position;
        }
    }

    public override void LoadVariables()
    {
        saveInt1 = progress;
        ParticleToggle();
    }

    public override void SaveVariables()
    {
        progress = saveInt1;
    }
}
