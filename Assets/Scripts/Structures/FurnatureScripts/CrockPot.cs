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

    CookingRecipe currentRecipe;

    bool madeBugDish = false; // For achievement purposes


    public void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
        RefreshSockets();

        lidClosed = true;

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1);
        for(int i = 0; i < itemSockets.Count; i++)
        {
            savedItems.Add(null);
        }
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
                audioHandler.PlaySound(audioHandler.miscSounds1[5]);
            }
            //audioHandler.PlaySound(audioHandler.interactSound);
            audioHandler.PlaySound(audioHandler.miscSounds1[4]);
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

                placeItemParticles.transform.position = resultSprite.transform.position;
                placeItemParticles.Play();

                audioHandler.PlaySound(audioHandler.itemInteractSound);
                currentRecipe.amountMade += 1;
            }
            return;
        }

        if(!CanBeRemoved() && !isCooking) RemoveClosestSocket();
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
        if(hasFinishedItem || isCooking || lidClosed) 
        {
            audioHandler.PlaySound(audioHandler.miscSounds1[1]);
            return;
        }

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
        currentRecipe = GetRecipe();

        cookTimeLeft = currentRecipe.cookTimeInSeconds;

        yield return new WaitForSeconds(0.5f);
        audioHandler.PlaySound(audioHandler.interactSound);
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

        savedItems[0] = currentRecipe.output; //Cooked Item

        audioHandler.PlaySound(audioHandler.miscSounds1[0]);

        finishPoof.Play();
        audioHandler.PlaySound(audioHandler.miscSounds1[3]);


        RefreshModel();
        RefreshSockets();

        if(madeBugDish) AchievementManager.Instance.CompleteProgressWithEnum(ACHKey.Grub_Hub);
    }

    CookingRecipe GetRecipe()
    {
        List<CookingRecipe> allRecipes = CookingDatabase.Instance.GetCraftingDatabase();

        List<CookingRecipe> validRecipes = new List<CookingRecipe>();

        List<InventoryItemData> ingredients = new List<InventoryItemData>(savedItems);

        List<CookingStats> recipeStats = InitializeStats();

        //Get the stats of the ingredients
        for(int i = 0; i < ingredients.Count; ++i)
        {
            if(ingredients[i] == null || ingredients[i].cookingStats == null)
            {
                ingredients.RemoveAt(i);
                i--;
                continue;
            }
            for(int c = 0; c < ingredients[i].cookingStats.Count; ++c)
            {
                bool addedValue = false;
                for(int r = 0; r < recipeStats.Count; ++r)
                {
                    if(recipeStats[r].type == ingredients[i].cookingStats[c].type)
                    {
                        recipeStats[r].value += ingredients[i].cookingStats[c].value;
                        addedValue = true;
                        if(recipeStats[r].type == IngredientType.Bug) madeBugDish = true;
                        break;
                    }
                }

                if(!addedValue)
                {
                    recipeStats.Add(new CookingStats(ingredients[i].cookingStats[c].type, ingredients[i].cookingStats[c].value));
                }
            }
        }

        //Find matching recipes. Stop once at least one is found in that tier
        for(int i = 6; i >= -1; --i)
        {
            for(int x = 0; x < allRecipes.Count; ++x)
            {
                if(allRecipes[x].priority == i)
                {
                    if(allRecipes[x].EligibleRecipe(ingredients, recipeStats)) validRecipes.Add(allRecipes[x]); //Matching recipe
                    allRecipes.RemoveAt(x);
                    --x;
                }
            }
            if(validRecipes.Count > 0) 
            {
                if(i == -1) madeBugDish = false;
                break; //We found a matching recipe at the highest priority, so we do not need to iterate anymore
            }
        }

        if(validRecipes.Count == 0) return null;

        CookingRecipe chosenRecipe = validRecipes[Random.Range(0, validRecipes.Count)];
        chosenRecipe.AddNewRecipe(ingredients);
        return chosenRecipe;
    }

    void PlaceOnClosestSocket(InventoryItemData item)
    {
        if(item.cookingStats == null || item.cookingStats.Count == 0) return; // Not a valid Ingredient

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
        if(savedItems.Count < itemSockets.Count)
        {
            for(int i = 0; i < itemSockets.Count; i++)
            {
                savedItems.Add(null);
            }
        }
        RefreshSockets();
        RefreshModel();
    }

    public List<CookingStats> InitializeStats()
    {
        List<CookingStats> newStats = new List<CookingStats>();
        newStats.Add(new CookingStats(IngredientType.Veggie, 0));
        newStats.Add(new CookingStats(IngredientType.Fruit, 0));
        newStats.Add(new CookingStats(IngredientType.Meat, 0));
        newStats.Add(new CookingStats(IngredientType.Sweetener, 0));
        newStats.Add(new CookingStats(IngredientType.Bug, 0));
        newStats.Add(new CookingStats(IngredientType.Egg, 0));
        newStats.Add(new CookingStats(IngredientType.Filler, 0));
        newStats.Add(new CookingStats(IngredientType.Weeds, 0));
        newStats.Add(new CookingStats(IngredientType.Tuber, 0));
        newStats.Add(new CookingStats(IngredientType.Nut, 0));

        return newStats;
    }
}
