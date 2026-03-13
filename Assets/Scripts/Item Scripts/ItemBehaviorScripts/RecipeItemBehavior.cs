using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/RecipeItem")]
public class RecipeItemBehavior : ItemBehavior
{
    public override void OnRecieve(InventoryItemData recievedItem)
    {
        CookingDatabase.Instance.UnlockRandomRecipe();
    }
}
