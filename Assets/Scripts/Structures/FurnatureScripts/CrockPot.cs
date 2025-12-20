using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrockPot : FurnitureBehaviorScript
{
    public List<SpriteRenderer> itemSockets = new List<SpriteRenderer>();
    public List<SpriteRenderer> itemSocketsSubmerged = new List<SpriteRenderer>();
    public SpriteRenderer resultSprite;

    public ParticleSystem boilParticles, oilSplashParticles, finishPoof, placeItemParticles;

    public GameObject oilObject, fireObject;

    bool hasOil, isLit, isCooking, hasFinishedItem, lidClosed;

    float cookTimeLeft = 15;

    public InventoryItemData oilItem;

    public Animator anim;


    public void Awake()
    {
        base.Awake();
        for(int i = 0; i < itemSockets.Count; i++)
        {
            savedItems.Add(null);
        }
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
        RefreshSockets();

        lidClosed = true;
    }

    void Update()
    {
        anim.SetBool("Cooking", isCooking);
        anim.SetBool("LidOpen", !lidClosed);
    }

    public override void StructureInteraction()
    {
        if(lidClosed && !isCooking)
        {
            lidClosed = false;
            if(hasFinishedItem)
            {
                finishPoof.Play();
                audioHandler.PlaySound(audioHandler.miscSounds1[3]);
            }
            audioHandler.PlaySound(audioHandler.interactSound);
            return;
        }

        if(hasFinishedItem)
        {
            bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(savedItems[0], 1);
            if (addedSuccessfully)
            {
                hasFinishedItem = false;
                resultSprite.sprite = null;
                savedItems[0] = null;
                RefreshSockets();
            }
            return;
        }

        if(!CanBeRemoved()) RemoveClosestSocket();
        return;

        /*bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }*/
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false)
        {
            if(!CanBeRemoved()) return;
            success = true;
        }
        else if(type == ToolType.Torch && PlayerInteraction.Instance.torchLit && !isCooking && CanBeginCooking() && !isLit)
        {
            if(!CanBeginCooking())
            {
                audioHandler.PlaySound(audioHandler.miscSounds1[1]);
                return;
            }
            isLit = true;
            lidClosed = true;
            isCooking = true;
            RefreshModel();
            StartCoroutine(CookCoroutine());
            success = true;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(hasFinishedItem || isCooking || lidClosed) return;

        if(item == oilItem)
        {
            if(hasOil) return;
            hasOil = true;
            oilSplashParticles.Play();
            audioHandler.PlaySound(audioHandler.miscSounds1[2]);
            RefreshModel();

            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            return;

        }
        if(item && !item.isKeyItem)
        {
            PlaceOnClosestSocket(item);
        }
    }

    public override void DigAction()
    {
        PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);

        Destroy(this.gameObject);
    }

    IEnumerator CookCoroutine()
    {
        cookTimeLeft = 15;
        while(cookTimeLeft > 0)
        {
            yield return new WaitForSeconds(1);
            if(TimeManager.Instance.stopTime) continue;
            --cookTimeLeft;
        }
        FinishCooking();
    }

    void FinishCooking()
    {
        isLit = false;
        hasOil = false;
        hasFinishedItem = true;
        isCooking = false;

        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(i >= savedItems.Count) break;
            savedItems[i] = null;
        }

        savedItems[0] = oilItem; //Cooked Item

        audioHandler.PlaySound(audioHandler.miscSounds1[0]);


        RefreshModel();
        RefreshSockets();
    }

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
            itemSocketsSubmerged[closestSocket].sprite = item.icon;
            savedItems[closestSocket] = item;

            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            placeItemParticles.transform.position = itemSockets[closestSocket].transform.position;
            placeItemParticles.Play();

            audioHandler.PlaySound(audioHandler.itemInteractSound);
        }
    }

    void RemoveClosestSocket()
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
            itemSocketsSubmerged[closestSocket].sprite = null;
            savedItems[closestSocket] = null;

            PlayerInventoryHolder.Instance.UpdateInventory();

            placeItemParticles.transform.position = itemSockets[closestSocket].transform.position;
            placeItemParticles.Play();

            audioHandler.PlaySound(audioHandler.itemInteractSound);
        }
    }

    void RefreshModel()
    {
        if(hasOil)
        {
            oilObject.SetActive(true);
            if(isLit) boilParticles.Play();
            else boilParticles.Stop();
        }
        else
        {
            oilObject.SetActive(false);
            boilParticles.Stop();
        }

        if(isLit)
        {
            fireObject.SetActive(true);
        }
        else
        {
            fireObject.SetActive(false);
        }
    }

    void RefreshSockets()
    {
        if(hasFinishedItem)
        {
            resultSprite.sprite = savedItems[0].icon;
        }
        else resultSprite.sprite = null;

        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(i >= savedItems.Count)
            {
                itemSockets[i].sprite = null;
                itemSocketsSubmerged[i].sprite = null;
                continue;
            }
            if(savedItems[i] != null && !hasFinishedItem)
            {
                itemSockets[i].sprite = savedItems[i].icon;
                itemSocketsSubmerged[i].sprite = savedItems[i].icon;
            }
            else 
            {
                itemSockets[i].sprite = null;
                itemSocketsSubmerged[i].sprite = null;
            }
        }
    }

    bool CanBeRemoved()
    {
        if(isCooking) return false;
        if(savedItems.Count == 0) return true;
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(savedItems[i] != null) return false;
        }
        return true;
    }

    bool CanBeginCooking()
    {
        if(savedItems.Count == 0 || !hasOil || isCooking) return false;
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(savedItems[i] == null) return false;
        }
        return true;
    }

    public override void SaveVariables()
    {
        saveBool1 = hasOil;
        if(hasFinishedItem) saveInt1 = 1;
        else saveInt1 = 0;
    }

    public override void LoadVariables()
    {
        hasOil = saveBool1;
        if(saveInt1 == 1) hasFinishedItem = true;
        else hasFinishedItem = false;
        if(savedItems.Count == 0)
        {
            for(int i = 0; i < itemSockets.Count; i++)
            {
                savedItems.Add(null);
            }
        }
        RefreshSockets();
        RefreshModel();
    }
}
