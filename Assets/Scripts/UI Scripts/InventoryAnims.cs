using System.Collections;
using UnityEngine;

public class InventoryAnims : MonoBehaviour
{
    public float defaultCooldown = 5.0f; // Replace later if we need to I think the food cooldown is hardcoded to be 5 seconds in PlayerInteraction.cs
    private float foodCooldown; // Current cooldown for food items
    public float foodCooldownPercent => foodCooldown / defaultCooldown; // Percentage of cooldown remaining
    public bool isFoodCooldownActive => foodCooldown != defaultCooldown; // Apparently this can check if the cooldown is active??? Wow

    private void Awake()
    {
        foodCooldown = defaultCooldown; // Initialize cooldown
        PlayerInteraction.onFoodConsumed += SetFoodCooldown;
    }

    private void SetFoodCooldown(InventoryItemData item)
    {
        foodCooldown = item.useCooldown; // Reset cooldown
        defaultCooldown = item.useCooldown; // Set the cooldown based on the item
        StartCoroutine(InitiateFoodCooldown());
    }

    private IEnumerator InitiateFoodCooldown()
    {
        Debug.Log($"Food cooldown started for {defaultCooldown} seconds.");
        while (foodCooldown > 0)
        {
            foodCooldown -= Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        foodCooldown = defaultCooldown;
        StopCoroutine(InitiateFoodCooldown());
    }


}
