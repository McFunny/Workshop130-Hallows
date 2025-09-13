using System.Collections;
using UnityEngine;

public class InventoryAnims : MonoBehaviour
{
    public float cooldownStart = 5.0f; 
    private float foodCooldown; // Current cooldown for food items
    public float foodCooldownPercent => foodCooldown / cooldownStart; // Percentage of cooldown remaining
    public bool isFoodCooldownActive => foodCooldown != cooldownStart; // Apparently this can check if the cooldown is active??? Wow

    private void Awake()
    {
        foodCooldown = cooldownStart; // Initialize cooldown
        //PlayerInteraction.onFoodConsumed += SetFoodCooldown;
    }

    void OnEnable()
    {
        PlayerInteraction.onFoodConsumed += SetFoodCooldown;
    }

    private void SetFoodCooldown(InventoryItemData item)
    {
        foodCooldown = item.useCooldown; // Reset cooldown
        cooldownStart = item.useCooldown; // Set the cooldown based on the item
        StartCoroutine(InitiateFoodCooldown());
    }

    private IEnumerator InitiateFoodCooldown()
    {
        Debug.Log($"Food cooldown started for {cooldownStart} seconds.");
        while (foodCooldown > 0)
        {
            foodCooldown -= Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        foodCooldown = cooldownStart;
        StopCoroutine(InitiateFoodCooldown());
    }

    void OnDisable()
    {
        PlayerInteraction.onFoodConsumed -= SetFoodCooldown;
    }


}
