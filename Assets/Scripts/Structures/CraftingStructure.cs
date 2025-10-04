using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingStructure : StructureBehaviorScript
{
    public List<CraftingEntry> assignedCrafts = new List<CraftingEntry>();
    private CraftingSystem craftingSystem;

    public void Start()
    {
        base.Start();
        craftingSystem = FindObjectOfType<CraftingSystem>();   
    }
    public override void StructureInteraction()
    {
        //THIS IS WHERE U DO THE CODE TO BRING UP THE MENU Thank cam

        craftingSystem.OpenCraftingInterface();

    }
}

