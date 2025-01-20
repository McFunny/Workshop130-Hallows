using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompostBin : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;

    //public InventoryItemData fertilizerT, fertilizerG, fertilizerI;
    //public InventoryItemData[] fertilizers;
    public InventoryItemData compost;
    public InventoryItemData meat;
    public InventoryItemData fertilizerI;

    public Transform itemDropTransform;
    public GameObject fillPlane;

    public Animator anim;

    public int progress = 0;
    int maxProgress = 5;
    int maxContainedItems = 5;

    float bonusCompostValue = 0;
    float ichorFertilizerChance = 0;

    bool ignoreNextHour = true;
    bool isSpinning = false;

    //Are we doing this dont starve style (Put in 5 things to make 1) 
    //Items have an inherent "Bonus Compost Chance" value. Weeds have 0, carrots have 20

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }


    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {
        if(isSpinning && savedItems.Count != maxContainedItems) return;

        if(progress == maxProgress)
        {
            progress = 0;
            ichorFertilizerChance = 0;
            bonusCompostValue = 0;

            foreach(InventoryItemData item in savedItems)
            {
                bonusCompostValue += item.bonusCompostValue; 
                if(item == meat) ichorFertilizerChance++;
            }

            bool ready = false;
            int compostYield = 1;
            float r;
            while(!ready)
            {
                if(bonusCompostValue/2 > 100)
                {
                    bonusCompostValue -= 100;
                    compostYield++;
                }
                else
                {
                    if(bonusCompostValue > 100)
                    {
                        bonusCompostValue *= 0.5f;
                        for(int i = 0; i < 2; i++)
                        {
                            r = Random.Range(0,10);
                            if(r < bonusCompostValue) compostYield++;
                        }
                        ready = true;
                    }
                    else
                    {
                        r = Random.Range(0,10);
                        if(r < bonusCompostValue) compostYield++;
                        ready = true;
                    }
                }
            }
            //
            StartCoroutine(GrabItems(compostYield));
        }
    }

    IEnumerator GrabItems(int num)
    {
        anim.SetBool("Spinning", false);
        anim.SetBool("IsFull", false);

        yield return new WaitForSeconds(1.1f);

        GameObject droppedItem;
        for(int i = 0; i < num; i++)
        {
            float r = Random.Range(0,10);
            if(r < ichorFertilizerChance) droppedItem = ItemPoolManager.Instance.GrabItem(fertilizerI);
            else droppedItem = ItemPoolManager.Instance.GrabItem(compost);
            droppedItem.transform.position = itemDropTransform.position;

            Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(Vector3.forward * 20);
            itemRB.AddForce(Vector3.up * 10);
            audioHandler.PlaySound(audioHandler.activatedSound);

            GameObject poofParticle;
            poofParticle = ParticlePoolManager.Instance.GrabCloudParticle();
            poofParticle.transform.position = itemDropTransform.position;
            yield return new WaitForSeconds(0.2f);
        }
        savedItems.Clear();
        isSpinning = false;
        fillPlane.SetActive(false);
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item.bonusCompostValue > 0 && savedItems.Count < maxContainedItems)
        {
            //
            savedItems.Add(item);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            audioHandler.PlaySound(audioHandler.activatedSound);

            GameObject poofParticle;
            poofParticle = ParticlePoolManager.Instance.GrabCloudParticle();
            poofParticle.transform.position = itemDropTransform.position;

            //GameObject poofParticle = ParticlePoolManager.Instance.GrabExtinguishParticle();
            //poofParticle.transform.position = seedSocket.position;

            //audioHandler.PlaySound(audioHandler.itemInteractSound);

            fillPlane.SetActive(true);

            if(savedItems.Count == maxContainedItems)
            {
                isSpinning = true;
                anim.SetBool("Spinning", true);
                anim.SetBool("IsFull", true);
            }
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

            if(progress == maxProgress)
            {
                isSpinning = false;
                anim.SetBool("Spinning", false);
            }
        }
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
        droppedItem.transform.position = transform.position;

        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop seeds
        GameObject droppedItem;
        foreach(InventoryItemData item in savedItems)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(item);
            droppedItem.transform.position = itemDropTransform.position;
        }
    }
}
