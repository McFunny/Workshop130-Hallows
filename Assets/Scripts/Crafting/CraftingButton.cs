using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingButton : MonoBehaviour
{
    public CraftingEntry assignedEntry;
    public TextMeshProUGUI itemNameText, itemCountText;
    public GameObject questionMark;
    public Image icon;
    [HideInInspector] public CraftingSystem craftingSystem;

    public void OnSelect()
    {
        

        craftingSystem.UpdateAssignedEntry(assignedEntry);
        
    }
    
}
