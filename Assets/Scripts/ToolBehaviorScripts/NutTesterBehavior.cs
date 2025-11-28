using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/NutTester")]
public class NutTesterBehavior : ToolBehavior
{
    public AudioClip blipSFX;

    bool holding = false;

    Coroutine scanningCoroutine;
    Coroutine chargingCoroutine;

    InventoryItemData currentSeed; // Synced Seed
    Vector3 currentTile = new Vector3(-1,-1,-1); // Stored Tile
    int currentSlotIndex = -1; //When -1, it is not paired with a seed
    bool onHotbar = true;


    public override void OnHolster()
    {
        currentSeed = null;
        currentTile = new Vector3(-1, -1, -1);
        currentSlotIndex = -1;
        onHotbar = true;
        NutrientTesterScript.Instance.UpdateSeed(null);
        NutrientTesterScript.Instance.UpdateTile(null);
    }

    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        BeginCharge();
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        switch (NutrientTesterScript.Instance.ReturnMode())
        {
            case NutrientTesterScript.TesterMode.Nutrient:
                CycleSeed();
                break;
            case NutrientTesterScript.TesterMode.Radar:
                //No secondary use in radar mode Yet
                break;
        }
    }

    private void CycleSeed()
    {
        bool foundSeed = false;
        //Change Synced Seed
        HandItemManager.Instance.toolSource.PlayOneShot(blipSFX);

        List<InventorySlot> inventorySlots;
        CropItem c_item = null;

        for (int iterations = 0; iterations < 2; iterations++) //Iterating twice to make sure both inventories are checked
        {
            if (onHotbar) inventorySlots = PlayerInventoryHolder.Instance.PrimaryInventorySystem.InventorySlots;
            else inventorySlots = PlayerInventoryHolder.Instance.secondaryInventorySystem.InventorySlots;

            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (i <= currentSlotIndex) continue;

                c_item = inventorySlots[i].ItemData as CropItem;
                if (c_item) //New Seed found to sync to
                {
                    currentSeed = c_item;
                    currentSlotIndex = i;
                    foundSeed = true;
                    break;
                }
            }

            if (foundSeed)
            {
                break;
            }
            else
            {
                currentSlotIndex = -1;
                currentSeed = null;
                onHotbar = !onHotbar;
            }
        }

        //Display new info. If the current need is null, then dont display seed info

        NutrientTesterScript.Instance.UpdateSeed(currentSeed as CropItem);

        if(currentSeed) Debug.Log("Found " + currentSeed + " in slot " + currentSlotIndex);
        else Debug.Log("No seed is present in the inventory or cycled through all seeds");
    }

    void BeginCharge()
    {
        if(scanningCoroutine == null) 
        {
            scanningCoroutine = HandItemManager.Instance.StartCoroutine(Scan());
            chargingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeTimer());
        }
    }

    IEnumerator Scan()
    {
        yield return new WaitForSeconds(0.01f);
        PlayerInteraction.Instance.ToolUseToggle(true);
        yield return new WaitUntil(() => !InputManager.isCharging);

        //THIS IS WHERE U WOULD ADD CODE FOR THE ITEM NOT BEING USED ANYMORE

        HandItemManager.Instance.StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
        toolAnim.Play("MoveToIdle");
        scanningCoroutine = null;
        PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);

        yield return new WaitForSeconds(0.4f);
        PlayerInteraction.Instance.ToolUseToggle(false);

    }

    IEnumerator ChargeTimer()
    {
        //Play the anim and particles
        toolAnim.Play("MoveToUse");
        yield return new WaitForSeconds(0.4f);
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(PlayerInteraction.Instance.gameObject, 0.8f, "NutTester", false));
        float timeBetweenScans = 0.2f;

        if(NutrientTesterScript.Instance.ReturnMode() == NutrientTesterScript.TesterMode.Nutrient)
        {
            while(InputManager.isCharging)
            {
                yield return new WaitForSeconds(timeBetweenScans);
                Vector3 fwd = player.TransformDirection(Vector3.forward);
                RaycastHit hit;
                //Debug.Log("AttemptRaycast");
                if (Physics.Raycast(player.position, fwd, out hit, 7f, mask))
                {
                    Vector3 tile = StructureManager.Instance.CheckTile(hit.point);
                    if (!StructureManager.Instance.ValidateGridType(tile, GridType.Farm))
                    {
                        //NutrientTesterScript.Instance.UpdateTile(null);
                        continue;
                    }

                    if (tile != currentTile)
                    {
                        NutrientStorage nutrients = StructureManager.Instance.FetchNutrient(tile);
                        NutrientTesterScript.Instance.UpdateTile(nutrients);
                        currentTile = tile;
                        HandItemManager.Instance.toolSource.PlayOneShot(blipSFX);
                    }

                    Debug.Log(currentTile);

                    
                }
                //HandItemManager.Instance.toolSource.PlayOneShot(blipSFX);
            }
        }
    }
}
