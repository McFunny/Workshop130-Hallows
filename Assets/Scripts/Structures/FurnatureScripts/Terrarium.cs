using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Terrarium : FurnitureBehaviorScript
{
    public SpriteRenderer bugPetRenderer;//, flyingBugPetRenderer;
    public GameObject petBugObject;
    BugPet currentBugPet;

    public Transform[] bugMoveLocations;

    public List<BugPet> bugPets;

    public bool cantDigUp;

    public void Awake()
    {
        base.Awake();
        savedItems.Add(null);
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
        bugPetRenderer.sprite = null;
        //flyingBugPetRenderer.sprite = null;
        LoadVariables();
        StartCoroutine(AnimateBug());
        StartCoroutine(MoveBug());
    }

    public override void StructureInteraction()
    {
        bool addedSuccessfully;
        if (cantDigUp) return;
        if(!CanBeRemoved())
        {
            addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(savedItems[0], 1);
            if (!addedSuccessfully) return;

            bugPetRenderer.sprite = null;
            //flyingBugPetRenderer.sprite = null;
            currentBugPet = null;
            savedItems[0] = null;

            PlayerInventoryHolder.Instance.UpdateInventory();
            return;
        }
        addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(!CanBeRemoved()) return;
        if (cantDigUp) return;
        if (type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false)
        {
            //StartCoroutine(DugUp());
            success = true;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if (item && (savedItems.Count == 0 || savedItems[0] == null))
        {
            InsertItem(item);
        }
    }

    public override void DigAction()
    {
        PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);

        Destroy(this.gameObject);
    }

    void InsertItem(InventoryItemData _item)
    {
        for(int i = 0; i < bugPets.Count; i++)
        {
            if(bugPets[i].item == _item)
            {
                currentBugPet = bugPets[i];
                if(savedItems.Count > 0) savedItems[0] = _item;
                else savedItems.Add(_item);
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();

                return;
            }
        }
    }

    bool CanBeRemoved()
    {
        if(savedItems.Count > 0 && savedItems[0] != null) return false;
        return true;
    }

    IEnumerator AnimateBug()
    {
        int currentSprite = 0;
        float animSpeed = 0.3f;
        while(gameObject.activeSelf)
        {
            if(currentBugPet == null)
            {
                currentSprite = 0;
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            currentSprite++;
            if(currentSprite >= currentBugPet.sprites.Length) currentSprite = 0;
            yield return new WaitForSeconds(animSpeed);
            if(currentBugPet == null) continue;

            /*if(currentBugPet.isFlying) //flyingBugPetRenderer.sprite = currentBugPet.sprites[currentSprite];
            else*/ bugPetRenderer.sprite = currentBugPet.sprites[currentSprite];
        }
    }

    IEnumerator MoveBug()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(Random.Range(6, 18));

            if(currentBugPet == null) continue;

            Vector3 newDestination = bugMoveLocations[Random.Range(0, bugMoveLocations.Length)].position;

            petBugObject.transform.LookAt(newDestination);

            Vector3 direction = PlayerInteraction.Instance.transform.position - petBugObject.transform.position;
            float dotProduct = Vector3.Dot(direction, transform.right);

            if (dotProduct > 0)
            {
                bugPetRenderer.flipX = true;
                //flyingBugPetRenderer.flipX = true;
                //Debug.Log("player is to the right");
            }
            else if (dotProduct < 0)
            {
                bugPetRenderer.flipX = false;
                //flyingBugPetRenderer.flipX = false;
                //Debug.Log("player is to the left");
            }

            petBugObject.transform.DOMove(newDestination, Random.Range(1f, 3f));
        }
    }

    public override void LoadVariables()
    {
        if(savedItems.Count == 0 || savedItems[0] == null)
        {
            bugPetRenderer.sprite = null;
            //flyingBugPetRenderer.sprite = null;
            savedItems.Clear();
            savedItems.Add(null);
            return;
        }


        for(int i = 0; i < bugPets.Count; i++)
        {
            if(bugPets[i].item == savedItems[0])
            {
                currentBugPet = bugPets[i];
                return;
            }
        }

        savedItems[0] = Database.Instance.GetItem(saveInt3);
    }

    public override void SaveVariables()
    {
        if (savedItems.Count > 0 && savedItems[0] != null)
            saveInt3 = savedItems[0].ID;

    }
}

[System.Serializable]
public class BugPet
{
    public InventoryItemData item;
    public Sprite[] sprites;
    //public bool isFlying;
}
