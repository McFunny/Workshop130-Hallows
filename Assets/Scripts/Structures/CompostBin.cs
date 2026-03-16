using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompostBin : StructureBehaviorScript
{
    public InventoryItemData compost;
    public InventoryItemData meat, meatSmall, meatLarge;
    public InventoryItemData fertilizerI;

    public Transform itemDropTransform;
    public GameObject fillPlane;

    public Animator anim;

    public int progress = 0;
    int maxProgress = 5;
    //int maxContainedItems = 5;
    float currentCompostValue = 0;
    int maxCompostValue = 100;

    //float bonusCompostValue = 0;
    float ichorFertilizerChance = 0; //

    bool ignoreNextHour = false;
    bool isSpinning = false;

    public ParticleSystem completedParticles;

    //public TextMeshProUGUI itemText;

    //public bool isFunctioning = false; //cannot interact with it until its been on the farm at night
    //public PopupScript chargingPopup;

    void Awake()
    {
        base.Awake();
        //itemText.text = currentCompostValue + "/" + maxCompostValue;
    }

    void Start()
    {
        base.Start();

        

        if(StructureManager.Instance.ValidateGridType(transform.position, GridType.Farm) == false) maxProgress *= 3; //Takes longer when not on the farm
    }


    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {

        if(isSpinning) return;

        if(progress == maxProgress)
        {
            progress = 0;
            ichorFertilizerChance = 0;
            //bonusCompostValue = 0;

            foreach(InventoryItemData item in savedItems)
            {
                //bonusCompostValue += item.bonusCompostValue; 
                if(item == meat) ichorFertilizerChance += 0.5f;
                if(item == meatSmall) ichorFertilizerChance += 0.25f;
                if(item == meatLarge) ichorFertilizerChance += 2;
            }

            int compostYield = Random.Range(2, 4);
            if(Random.Range(0, 10) > 4) compostYield++;
            if(Random.Range(0, 10) > 6) compostYield++;

            /*float r;
            bool ready = false;
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
                            r = Random.Range(0,80);
                            if(r < bonusCompostValue) compostYield++;
                        }
                        ready = true;
                    }
                    else
                    {
                        r = Random.Range(0,80);
                        if(r < bonusCompostValue) compostYield++;
                        ready = true;
                    }
                }
            }*/
            StartCoroutine(GrabItems(compostYield));
            completedParticles.Stop();
        }
    }

    IEnumerator GrabItems(int num)
    {
        anim.SetBool("Spinning", false);
        anim.SetBool("IsFull", false);

        yield return new WaitForSeconds(0.7f);

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
        currentCompostValue = 0;
        //itemText.text = currentCompostValue + "/" + maxCompostValue;
    }

    public override void ItemInteraction(InventoryItemData item)
    {

        if(item.bonusCompostValue > 0 && currentCompostValue < maxCompostValue)
        {
            currentCompostValue += item.bonusCompostValue;

            savedItems.Add(item);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            audioHandler.PlaySound(audioHandler.activatedSound);

            GameObject poofParticle;
            poofParticle = ParticlePoolManager.Instance.GrabCloudParticle();
            poofParticle.transform.position = itemDropTransform.position;

            fillPlane.SetActive(true);

            anim.Play("Recoil");

            if(currentCompostValue >= maxCompostValue)
            {
                currentCompostValue = maxCompostValue;
                isSpinning = true;
                ignoreNextHour = true;
                anim.SetBool("Spinning", true);
                anim.SetBool("IsFull", true);
            }
            //itemText.text = currentCompostValue + "/" + maxCompostValue;
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
        
        if(progress < maxProgress && currentCompostValue >= maxCompostValue)
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
                completedParticles.Play();
            }
        }
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

    public override void LoadVariables()
    {
        progress = saveInt1;
        currentCompostValue = saveFloat1;
        if(currentCompostValue >= maxCompostValue)
        {
            isSpinning = true;
            anim.SetBool("Spinning", true);
            anim.SetBool("IsFull", true);
        }

        if(progress == maxProgress)
        {
            isSpinning = false;
            anim.SetBool("Spinning", false);
            completedParticles.Play();
        }

        //itemText.text = currentCompostValue + "/" + maxCompostValue;
    }

    public override void SaveVariables()
    {
        saveInt1 = progress;
        saveFloat1 = currentCompostValue;
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        structureUIVariables.valueGroups[1].value = progress;
        structureUIVariables.valueGroups[1].maxValue = maxProgress;

        structureUIVariables.valueGroups[2].value = currentCompostValue;
        structureUIVariables.valueGroups[2].maxValue = maxCompostValue;
        return structureUIVariables.valueGroups;
    }
}
