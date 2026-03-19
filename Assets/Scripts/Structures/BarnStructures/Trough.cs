using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Trough : StructureBehaviorScript
{
    public List<SpriteRenderer> itemSockets = new List<SpriteRenderer>();

    public GameObject waterObject;
    public ParticleSystem splash;

    public int waterLevel = 0; //max is maxWaterLevel
    int maxWaterLevel = 5;

    public TextMeshProUGUI waterText;

    public GameObject crowPrefab;

    public void Awake()
    {
        waterObject.SetActive(false);
        base.Awake();
        for(int i = 0; i < itemSockets.Count; i++)
        {
            savedItems.Add(null);
        }
        allowContinousWatering = true;
    }

    public void Start()
    {
        base.Start();
        RefreshSockets();
        HourPassed();
    }

    public override void HourPassed()
    {
        if(TimeManager.Instance.currentHour == 8 && ContainsItems())
        {
            Collider[] hitStructures = Physics.OverlapSphere(transform.position, 40f, 1 << 6);
            foreach(Collider collider in hitStructures)
            {
                ImbuedScarecrow scarecrow = collider.gameObject.GetComponentInParent<ImbuedScarecrow>();
                if(scarecrow)
                {
                    return;
                }
            }
            SpawnCrow();
        }
    }

    void Update()
    {
        if(waterLevel > 0) waterText.text = waterLevel + "/" + maxWaterLevel;
        else waterText.text = "";
        base.Update();
    }

    public override void StructureInteraction()
    {
        if(ContainsItems())
        {
            RemoveClosestSocket();
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(ContainsItems()) return;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUp());
            success = true;
        }
        if (type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && waterLevel < maxWaterLevel)
        {
            int waterGained = 0;
            for(int i = 0; i < PlayerInteraction.Instance.maxWaterHeld; i++)
            {
                if(PlayerInteraction.Instance.waterHeld > 0 && waterLevel + waterGained < maxWaterLevel)
                {
                    PlayerInteraction.Instance.WaterChange(-1);
                    //PlayerInteraction.Instance.waterHeld--;
                    waterGained++;
                }
            }
            WaterLevelChange(waterGained);
            success = true;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item && !item.isKeyItem && waterLevel == 0 && item.animalHungerValue > 0)
        {
            PlaceOnClosestSocket(item);
        }
    }

    public override void HitWithWater()
    {
        if(waterLevel < maxWaterLevel) 
        {
            WaterLevelChange(1);
        }
    }

    /*public override void DigAction()
    {
        PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);

        Destroy(this.gameObject);
    }*/

    void PlaceOnClosestSocket(InventoryItemData item)
    {
        Vector3 fwd = PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        Vector3 hitPos;
        if (Physics.Raycast(PlayerInteraction.Instance.mainCam.transform.position, fwd, out hit, 15, 1 << 6))
        {
            hitPos = hit.point;
        }
        else return;

        float minDist = 100;
        float dist;
        int closestSocket = -1;

        for(int i = 0; i < itemSockets.Count; i++)
        {
            dist = Vector3.Distance(itemSockets[i].transform.position, hitPos);
            if(dist < minDist && savedItems[i] == null)
            {
                minDist = dist;
                closestSocket = i;
            }
        }
        if(closestSocket != -1)
        {
            itemSockets[closestSocket].sprite = item.icon;
            savedItems[closestSocket] = item;

            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
        }
    }

    void RemoveClosestSocket() //For player
    {
        Vector3 fwd = PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        Vector3 hitPos;
        if (Physics.Raycast(PlayerInteraction.Instance.mainCam.transform.position, fwd, out hit, 15, 1 << 6))
        {
            hitPos = hit.point;
        }
        else return;

        float minDist = 100;
        float dist;
        int closestSocket = -1;

        for(int i = 0; i < itemSockets.Count; i++)
        {
            dist = Vector3.Distance(itemSockets[i].transform.position, hitPos);
            if(dist < minDist && savedItems[i] != null)
            {
                minDist = dist;
                closestSocket = i;
            }
        }
        if(closestSocket != -1)
        {
            bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(savedItems[closestSocket], 1);
            if (!addedSuccessfully) return;

            itemSockets[closestSocket].sprite = null;
            savedItems[closestSocket] = null;

            PlayerInventoryHolder.Instance.UpdateInventory();
        }
    }

    public bool EatItem(List<InventoryItemData> foodDiet, out InventoryItemData itemAte) //For creatures eating
    {
        itemAte = null;
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(savedItems[i] != null && foodDiet.Contains(savedItems[i]))
            {
                itemAte = savedItems[i];
                itemSockets[i].sprite = null;
                savedItems[i] = null;
                ParticlePoolManager.Instance.MoveAndPlayParticle(itemSockets[i].transform.position, ParticlePoolManager.Instance.dirtParticle);
                return true;
            }
        }
        return false;
    }

    public bool EatItem(CritterType type, out InventoryItemData itemAte) //For creatures eating
    {
        itemAte = null;
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(savedItems[i] != null && savedItems[i].foodForCritters.Count > 0 && savedItems[i].foodForCritters.Contains(type))
            {
                itemAte = savedItems[i];
                itemSockets[i].sprite = null;
                savedItems[i] = null;
                ParticlePoolManager.Instance.MoveAndPlayParticle(itemSockets[i].transform.position, ParticlePoolManager.Instance.dirtParticle);
                return true;
            }
        }
        return false;
    }

    public bool HasEdibleItem(List<InventoryItemData> foodDiet) //For creatures eating
    {
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(savedItems.Count < i) savedItems.Add(null);
            if(savedItems[i] != null && foodDiet.Contains(savedItems[i]))
            {
                return true;
            }
        }
        return false;
    }

    public bool HasEdibleItem(CritterType type) //For creatures eating
    {
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(savedItems.Count < i) savedItems.Add(null);
            if(savedItems[i] != null && savedItems[i].foodForCritters.Count > 0 && savedItems[i].foodForCritters.Contains(type))
            {
                return true;
            }
        }
        return false;
    }

    public void WaterLevelChange(int amount)
    {
        waterLevel += amount;
        if(waterLevel > 0) waterObject.SetActive(true);
        else waterObject.SetActive(false);

        splash.Play();
        audioHandler.PlaySound(audioHandler.interactSound);

        if(waterLevel < maxWaterLevel) allowContinousWatering = true;
        else allowContinousWatering = false;
    }

    void RefreshSockets()
    {
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(i >= savedItems.Count)
            {
                itemSockets[i].sprite = null;
                continue;
            }
            if(savedItems[i] != null) itemSockets[i].sprite = savedItems[i].icon;
            else itemSockets[i].sprite = null;
        }
    }

    bool ContainsItems()
    {
        if(savedItems.Count == 0) return false;
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(savedItems[i] != null) return true;
        }
        return false;
    }

    void SpawnCrow()
    {
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(savedItems.Count < i) savedItems.Add(null);
            if(savedItems[i] != null && savedItems[i].animalHungerValue > 0 && Random.Range(0, 10) > 7)
            {
                Instantiate(crowPrefab, itemSockets[i].transform.position, Quaternion.identity).GetComponentInChildren<MutatedCrow>().isDecorCrow = true;
                itemSockets[i].sprite = null;
                savedItems[i] = null;
                return;
            }
        }
    }

    public override void LoadVariables()
    {
        if(saveInt1 > 0) WaterLevelChange(saveInt1);

        if(savedItems.Count < itemSockets.Count)
        {
            for(int i = 0; i < itemSockets.Count; i++)
            {
                savedItems.Add(null);
            }
        }
        RefreshSockets();
    }

    public override void SaveVariables()
    {
        saveInt1 = waterLevel;
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;

        structureUIVariables.valueGroups[0].value = waterLevel;
        structureUIVariables.valueGroups[0].maxValue = maxWaterLevel;
        return structureUIVariables.valueGroups;
    }
}
