using System.Collections;
using UnityEngine;

public class InventoryAnims : MonoBehaviour
{
    public float defaultCooldown = 5.0f; // Replace later if we need to I think the food cooldown is hardcoded to be 5 seconds in PlayerInteraction.cs
    public float foodCooldown; // Current cooldown for food items
    public bool isFoodCooldownActive => foodCooldown != defaultCooldown; // Apparently this can check if the cooldown is active??? Wow

    private void Awake()
    {
        foodCooldown = defaultCooldown; // Initialize cooldown
        PlayerInteraction.onFoodConsumed += SetFoodCooldown;
    }

    private void SetFoodCooldown()
    {
        if (foodCooldown < defaultCooldown) return; // If cooldown is already active, do nothing
        StartCoroutine(InitiateFoodCooldown());
    }

    private IEnumerator InitiateFoodCooldown()
    {
        foodCooldown = defaultCooldown; // Reset cooldown
        while (foodCooldown > 0)
        {
            foodCooldown -= Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        foodCooldown = defaultCooldown;
        StopCoroutine(InitiateFoodCooldown());
    }


}
