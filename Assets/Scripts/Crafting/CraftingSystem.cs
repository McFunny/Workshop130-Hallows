using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSystem : MonoBehaviour
{
    public static bool isCraftingMenuOpen;
    public CraftingEntry selectedEntry;
    public Button craftButton;
    [SerializeField] private GameObject craftingMenu;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject container;
    [SerializeField] private ToolTipScript descriptionBox;
    [SerializeField] private GameObject descriptionBoxVisuals;
    [HideInInspector] public TextMeshProUGUI craftButtonText;
    private CraftingEntry[] craftingEntries;
    private GameObject descriptionBoxContainer;

    private void Start()
    {
        craftButtonText = craftButton.GetComponentInChildren<TextMeshProUGUI>();
        craftingMenu.SetActive(false);
        isCraftingMenuOpen = false;
        craftingEntries = Resources.LoadAll<CraftingEntry>("Crafting");
        descriptionBoxContainer = descriptionBox.gameObject.transform.GetChild(0).gameObject;
        descriptionBoxVisuals.SetActive(false);
        Reset();
        PopulateCraftingInterface();
    }

    private void Update()
    {
        DebugOpenInventory();
    }

    private void DebugOpenInventory()
    {
        if (!StructureManager.Instance.enableCheats) return;

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            OpenCraftingInterface();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isCraftingMenuOpen) OpenCraftingInterface();
        }
    }

    public void OpenCraftingInterface()
    {
        if (PlayerMovement.isStalled && !isCraftingMenuOpen) return;

        craftingMenu.SetActive(!craftingMenu.activeSelf);
        isCraftingMenuOpen = craftingMenu.activeSelf;

        if (isCraftingMenuOpen)
        {
            descriptionBoxContainer.SetActive(false);
            PlayerMovement.restrictMovementTokens++;
            Reset();
            PopulateCraftingInterface();
        }
        else
        {
            PlayerMovement.restrictMovementTokens--;
        }
    }

    private void Reset()
    {
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }
        descriptionBoxContainer.SetActive(false);
        descriptionBoxVisuals.SetActive(false);
    }

    private void PopulateCraftingInterface()
    {
        foreach (CraftingEntry entry in craftingEntries)
        {
            var tempButton = Instantiate(buttonPrefab, container.transform);
            var buttonVars = tempButton.GetComponent<CraftingButton>();

            buttonVars.assignedEntry = entry;

            buttonVars.craftingSystem = this;

            buttonVars.itemCountText.text = "x" + entry.outputAmount;
            buttonVars.icon.sprite = entry.output.icon;

            //Implement Unlocking later
            buttonVars.questionMark.SetActive(false);

            if (entry.nameOverride == "")
            {
                tempButton.name = entry.output.displayName;
                buttonVars.itemNameText.text = entry.output.displayName;
            }
            else
            {
                tempButton.name = entry.nameOverride;
                buttonVars.itemNameText.text = entry.nameOverride;
            }

        }
    }

    public void UpdateAssignedEntry(CraftingEntry entry)
    {
        selectedEntry = entry;
        descriptionBoxContainer.SetActive(true);
        descriptionBoxVisuals.SetActive(true);
        descriptionBox.UpdateToolTip(entry.output);
        descriptionBox.UpdateTooltipCraft(entry);

        CraftReason CanCraft = IsAbleToCraft();

        if (CanCraft.Success)
        {
            craftButtonText.text = "Craft";
            craftButton.interactable = true;
        }
        else
        {
            craftButtonText.text = CanCraft.Reason;
            craftButton.interactable = false;
        }
    }

    public bool CanAffordCraft()
    {
        if (PlayerInteraction.Instance.currentMoney < selectedEntry.mintCost) return false;
        for (int i = 0; i < selectedEntry.craftingRequirements.Count; i++)
        {
            int amountToFind = selectedEntry.craftingRequirements[i].requiredAmount;
            if (PlayerInventoryHolder.Instance.ReturnItemCountInPlayerInventory(selectedEntry.craftingRequirements[i].requiredItem) < amountToFind) return false;
        }
        return true;
    }

    public bool IsInventoryFull()
    {
        if (PlayerInventoryHolder.Instance.IsInventoryFull(selectedEntry.output, selectedEntry.outputAmount))
        {
            return true;
        }
        else return false;
    }

    public CraftReason IsAbleToCraft()
    {
        if (selectedEntry == null)
        {
            Debug.LogWarning("No crafting entry found. This text should never show up!");
            return new CraftReason(false, "No Entry Found!");
        }

        if (!CanAffordCraft())
        {
            Debug.LogWarning("Cannot afford craft. This text should never show up!");
            return new CraftReason(false, "Can't Afford");
        }

        if (IsInventoryFull())
        {
            Debug.Log("Inventory full. Craft aborted. Put something on screen for this.");
            return new CraftReason(false, "Inventory Full");
        }

        return new CraftReason(true, "");
    }

    public void CraftItem()
    {
        CraftReason CanCraft = IsAbleToCraft();

        if (!CanCraft.Success) return;

        List<ItemWithAmount> classConv = new List<ItemWithAmount>();

        for (int i = 0; i < selectedEntry.craftingRequirements.Count; i++)
        {
            var item = selectedEntry.craftingRequirements[i].requiredItem;
            var amount = selectedEntry.craftingRequirements[i].requiredAmount;

            classConv.Add(new ItemWithAmount(item, amount));
        }

        PlayerInteraction.Instance.currentMoney -= selectedEntry.mintCost;
        PlayerInventoryHolder.Instance.RemoveItemsFromBothInventories(classConv);
        PlayerInventoryHolder.Instance.AddToInventory(selectedEntry.output, selectedEntry.outputAmount);
        UpdateAssignedEntry(selectedEntry);
    }
}

public class CraftReason
{
    public bool Success { get; private set; }
    public string Reason { get; private set; }

    public CraftReason(bool _success, string _reason)
    {
        Success = _success;
        Reason = _reason;
    }
}
