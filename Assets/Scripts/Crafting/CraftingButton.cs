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
    public Image icon, bulb;
    public bool unlocked = false;
    [SerializeField] private AudioClip selectSound;
    [HideInInspector] public CraftingSystem craftingSystem;

    public void OnSelect()
    {
        if(unlocked)
        {
            craftingSystem.UpdateAssignedEntry(assignedEntry);
            craftingSystem.PlaySound(selectSound, 0.25f);
            if(assignedEntry.isRecentlyUnlocked)
            {
                bulb.gameObject.SetActive(false);
                assignedEntry.isRecentlyUnlocked = false;
            }
            return;
        }
    }
    
}
