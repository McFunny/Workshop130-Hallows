using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HenNest : StructureBehaviorScript
{
    public bool containsEgg;

    public bool containsHen;

    public GameObject egg;
    public InventoryItemData eggItem;

    public override void StructureInteraction()
    {
        print("Interacted");
        if(containsEgg)
        {
            ItemPoolManager.Instance.GrabItem(eggItem).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            EggChange(false);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !containsEgg && !containsHen)
        {
            success = true;
        }
    }

    public void EggChange(bool addEgg)
    {
        if(containsEgg == addEgg) return;

        containsEgg = addEgg;

        if(containsEgg)
        {
            egg.SetActive(true);
        }
        else
        {
            egg.SetActive(false);
        }
    }

    public override void SaveVariables()
    {
        saveBool1 = containsEgg;
    }

    public override void LoadVariables()
    {
        EggChange(saveBool1);
    }
}
