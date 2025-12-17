using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CraftingSystem : MonoBehaviour
{
    public static bool isCraftingMenuOpen;
    public CraftingEntry selectedEntry;
    public Button craftButton;
    public Button collectButton;
    [SerializeField] private bool showCategories;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collectSound, openSound;
    [SerializeField] private GameObject craftingMenu;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject outputContainer;
    [SerializeField] private ToolTipScript descriptionBox;
    [SerializeField] private GameObject descriptionBoxVisuals;
    [SerializeField] private TextMeshProUGUI timerText;
    [HideInInspector] public TextMeshProUGUI craftButtonText;
    [SerializeField] private Color activeColor, inactiveColor, completeColor;
    [SerializeField] private List<Image> buttonBackgrounds;
    [SerializeField] private List<Image> outputImages;
    [SerializeField] private List<TextMeshProUGUI> outputText;
    [SerializeField] private List<Image> controllerImages;
    [SerializeField] private UILerp collectLerp, timerLerp;
    [SerializeField] private List<CanvasGroup> canvasGroups = new List<CanvasGroup>();
    private List<CraftingEntry> craftingEntries = new List<CraftingEntry>();
    private GameObject descriptionBoxContainer;
    private CanvasGroup thisCanvasGroup;
    private TextMeshProUGUI collectButtonText;
    private List<CraftingButton> craftingButtons = new List<CraftingButton>();
    private enum Categories //THIS ORDER MUST MATCH WHAT WILL BE ON SCREEN FROM LEFT TO RIGHT
    {
        Seed,
        Structure,
        Furniture,
        Trinket,
        Misc
    }
    [SerializeField] private Categories currentCategory = Categories.Seed;
    [SerializeField] private List<UILerp> categoryLerps;

    [HideInInspector] public CraftingStructure currentStructure;
    private const int CRAFTCAP = 5;
    private ControlManager controlManager;
    private Coroutine closeCraftingCoroutine;

    private void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }
    private void Start()
    {
        
        craftButtonText = craftButton.GetComponentInChildren<TextMeshProUGUI>();
        collectButtonText = collectButton.GetComponentInChildren<TextMeshProUGUI>();
        thisCanvasGroup = GetComponent<CanvasGroup>();
        craftingMenu.SetActive(false);
        isCraftingMenuOpen = false;
        craftingEntries = CraftingDatabase.Instance.GetCraftingDatabase();
        descriptionBoxContainer = descriptionBox.gameObject.transform.GetChild(0).gameObject;
        descriptionBoxVisuals.SetActive(false);
        Reset();
    }

    private void OnEnable()
    {
        controlManager.hotbarUp.action.started += HotbarUp;
        controlManager.hotbarDown.action.started += HotbarDown;
        controlManager.openInventory.action.started += CloseCraftingInterface;
    }

    private void OnDisable()
    {
        controlManager.hotbarUp.action.started -= HotbarUp;
        controlManager.hotbarDown.action.started -= HotbarDown;
        controlManager.openInventory.action.started -= CloseCraftingInterface;
    }

    private void HotbarUp(InputAction.CallbackContext obj)
    {
        if(!isCraftingMenuOpen) return;
        ControllerChangeCategories(-1);
    }

    private void HotbarDown(InputAction.CallbackContext obj)
    {
        if(!isCraftingMenuOpen) return;
        ControllerChangeCategories(1);
    }

    private void CloseCraftingInterface(InputAction.CallbackContext obj)
    {
        if(closeCraftingCoroutine != null)
        {
            StopCoroutine(closeCraftingCoroutine);
        }
        closeCraftingCoroutine = StartCoroutine(CloseCraftingInterfaceCoroutine());
    }

    private IEnumerator CloseCraftingInterfaceCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        if(isCraftingMenuOpen) OpenCraftingInterface();
    }

    private void Update()
    {
        if (!isCraftingMenuOpen) return;

        collectLerp.lerpToStart = collectButton.interactable;
        timerLerp.lerpToStart = currentStructure.isCrafting;

        if (ControlManager.isController)
        {
            controllerImages[0].enabled = collectButton.interactable;
            controllerImages[1].enabled = craftButton.interactable;
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (container.transform.childCount > 0)
                {
                    EventSystem.current.SetSelectedGameObject(GetFirstActiveObject(container.transform));
                }
            }
        }
        else
        {
            controllerImages[0].enabled = false;
            controllerImages[1].enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenCraftingInterface();
        }

        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            OpenCraftingInterface();
        }

        if (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            if (collectButton.interactable)
            {
                collectButton.onClick.Invoke();
            }
        }

        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            if (craftButton.interactable)
            {
                craftButton.onClick.Invoke();
            }
        }
    }

    public void OpenCraftingInterface()
    {
        if (PlayerMovement.isStalled && !isCraftingMenuOpen) return;

        craftingMenu.SetActive(!craftingMenu.activeSelf);
        isCraftingMenuOpen = craftingMenu.activeSelf;

        if (isCraftingMenuOpen)
        {
            PlaySound(openSound, 0.012f);
            descriptionBoxContainer.SetActive(false);
            PlayerMovement.restrictMovementTokens++;
            Reset();
            PopulateCraftingInterface();
            EnableDisableAllCanvasGroups(false);
            if (ControlManager.isController) EventSystem.current.SetSelectedGameObject(container.transform.GetChild(0).gameObject);

            if (currentStructure.craftSlots.Count > 0)
            {
                UpdateTimerText(currentStructure.craftSlots[currentStructure.currentSlot].timeRemaining);
                UpdateCraftButton();
            }
            else
            {
                UpdateTimerText(0);
                collectButton.interactable = false;
                collectButtonText.text = "Nothing to Collect";
            }
            UpdateCategory(currentCategory.ToString());
            Debug.Log(currentCategory);
            
        }
        else
        {
            PlayerMovement.restrictMovementTokens--;
            currentStructure = null;
            selectedEntry = null;
            EventSystem.current.SetSelectedGameObject(null);
            UpdateTimerText(0);
            EnableDisableAllCanvasGroups(true);
        }
    }

    private void Reset()
    {
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < outputImages.Count; i++)
        {
            outputImages[i].enabled = false;
        }

        if(craftingButtons.Count != 0 && craftingButtons != null) craftingButtons.Clear();
        
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
            craftingButtons.Add(buttonVars);

            // Check level requirement
            if (!entry.isUnlocked)
            {
                buttonVars.unlocked = false;
                buttonVars.questionMark.SetActive(true);
                buttonVars.itemNameText.text = "???";
                buttonVars.itemCountText.text = "";
                buttonVars.icon.gameObject.SetActive(false);
                buttonVars.bulb.gameObject.SetActive(false);
                tempButton.name = "Locked Craft";
                continue;
            }

            // Item is unlocked
            buttonVars.itemCountText.text = "x" + entry.outputAmount;
            buttonVars.icon.sprite = entry.output.icon;
            buttonVars.unlocked = true;
            buttonVars.questionMark.SetActive(false);
            buttonVars.bulb.gameObject.SetActive(entry.isRecentlyUnlocked);

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

        // Sort so unlocked items are first
        var children = container.GetComponentsInChildren<CraftingButton>();
        var sortedChildren = children.OrderByDescending(x => x.unlocked).ToList();
        for (int i = 0; i < sortedChildren.Count; i++)
        {
            sortedChildren[i].transform.SetSiblingIndex(i);
        }

        UpdateActiveCrafts();
    }

    private void ControllerChangeCategories(int val)
    {
        //Debug.Log("we made it");
        int currentVal = (int)currentCategory + val;
        if (currentVal < 0 || currentVal > categoryLerps.Count - 1) return;
        
        string categoryToChangeTo = System.Enum.GetName(typeof(Categories), currentVal);
        UpdateCategory(categoryToChangeTo);
    }

    public void UpdateCategory(string categoryString)
    {
        currentCategory = (Categories)System.Enum.Parse(typeof(Categories), categoryString);

        for(int i = 0; i < categoryLerps.Count; i++)
        {
            if(i == (int)currentCategory) categoryLerps[i].lerpToStart = true;
            else categoryLerps[i].lerpToStart = false;
        }

        if(categoryString == "All")
        {
            foreach (CraftingButton button in craftingButtons)
            {
                button.gameObject.SetActive(true);
            }
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        CraftingCategory category = (CraftingCategory)System.Enum.Parse(typeof(CraftingCategory), categoryString); //Help me (turns the categoryString into a crafting category enum)
        
        foreach (CraftingButton button in craftingButtons)
        {
            if (button.assignedEntry.category == category)
            {
                button.gameObject.SetActive(true);
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }
        EventSystem.current.SetSelectedGameObject(null);   
    }

    public void UpdateActiveCrafts()
    {
        for (int i = 0; i < CRAFTCAP; i++)
        {
            if (i < currentStructure.craftSlots.Count && currentStructure.craftSlots[i].assignedCraft != null)
            {
                outputImages[i].sprite = currentStructure.craftSlots[i].assignedCraft.output.icon;
                outputImages[i].enabled = true;

                if (currentStructure.craftSlots[i].isComplete)
                {
                    buttonBackgrounds[i].color = completeColor;
                    outputText[i].text = "x" + currentStructure.craftSlots[i].assignedCraft.outputAmount;
                }
                else if (i == currentStructure.currentSlot)
                {
                    buttonBackgrounds[i].color = activeColor;
                    outputText[i].text = "x" + currentStructure.craftSlots[i].assignedCraft.outputAmount;
                }
                else
                {
                    buttonBackgrounds[i].color = inactiveColor;
                    outputText[i].text = "x" + currentStructure.craftSlots[i].assignedCraft.outputAmount;
                }

            }
            else
            {
                outputImages[i].enabled = false;
                outputText[i].text = "";
                buttonBackgrounds[i].color = inactiveColor;
            }
        }
        UpdateCraftButton();
    }

    public void UpdateAssignedEntry(CraftingEntry entry)
    {
        selectedEntry = entry;
        if(selectedEntry == null)
        {
            descriptionBoxContainer.SetActive(false);
            descriptionBoxVisuals.SetActive(false);
            return;
        }
        descriptionBoxContainer.SetActive(true);
        descriptionBoxVisuals.SetActive(true);
        descriptionBox.UpdateToolTip(entry.output, true);
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

    public bool IsCraftingInterfaceFull()
    {
        return !currentStructure.CanAddCraft();
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

        //if (IsInventoryFull())
        //{
        //    Debug.Log("Inventory full. Craft aborted. Put something on screen for this.");
        //    return new CraftReason(false, "Inventory Full");
        //}

        if (IsCraftingInterfaceFull())
        {
            Debug.Log("Workbench Interface full full. Craft aborted. Put something on screen for this.");
            return new CraftReason(false, "Workbench Full");
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
        currentStructure.AddCraft(selectedEntry);
        //PlayerInventoryHolder.Instance.AddToInventory(selectedEntry.output, selectedEntry.outputAmount);
        UpdateAssignedEntry(selectedEntry);
        UpdateActiveCrafts();
    }

    public void SetCurrentStructure(CraftingStructure s)
    {
        currentStructure = s;
    }

    public void CollectCrafts()
    {
        currentStructure.StopCrafting();
        Debug.Log("Craft Stopped");
        for (int i = 0; i < currentStructure.craftSlots.Count; i++)
        {
            if (currentStructure.craftSlots[i].isComplete)
            {
                if (PlayerInventoryHolder.Instance.IsInventoryFull(currentStructure.craftSlots[i].assignedCraft.output, currentStructure.craftSlots[i].assignedCraft.outputAmount))
                {
                    Debug.Log("Inventory Full");
                    collectButtonText.text = "Inventory Full";
                    collectButton.interactable = false;
                    break;
                }
                //give player item
                PlayerInventoryHolder.Instance.AddToInventory(currentStructure.craftSlots[i].assignedCraft.output, currentStructure.craftSlots[i].assignedCraft.outputAmount);
                currentStructure.craftSlots[i].assignedCraft = null;
            }
        }
        PlaySound(collectSound, 0.077f);
        currentStructure.craftSlots.RemoveAll(item => item.assignedCraft == null);

        if (currentStructure.craftSlots.Count > 0)
        {
            currentStructure.StartCrafting();
            Debug.Log("Craft Starting");
        }
        UpdateActiveCrafts();
        if (selectedEntry != null) UpdateAssignedEntry(selectedEntry);
        
    }

    public void UpdateTimerText(int timeRemaining)
    {
        if (timeRemaining <= 0)
        {
            timerText.text = "";
            return;
        }

        int minutes = timeRemaining / 60;
        int seconds = timeRemaining % 60;

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateCraftButton()
    {
        if (currentStructure.craftSlots.Count > 0)
        {
            bool inventoryFull = PlayerInventoryHolder.Instance.IsInventoryFull(currentStructure.craftSlots[0].assignedCraft.output, currentStructure.craftSlots[0].assignedCraft.outputAmount);
            if (currentStructure.craftSlots[0].isComplete && !inventoryFull)
            {
                collectButton.interactable = true;
                collectButtonText.text = "Collect";
            }
            else if (inventoryFull)
            {
                collectButton.interactable = false;
                collectButtonText.text = "Inventory Full";
            }
            else
            {
                collectButton.interactable = false;
                collectButtonText.text = "Nothing to Collect";
            }
        }
        else
        {
            collectButton.interactable = false;
            collectButtonText.text = "Nothing to Collect";
        }
        
    }

    private void EnableDisableAllCanvasGroups(bool val)
    {
        thisCanvasGroup.interactable = !val;
        thisCanvasGroup.blocksRaycasts = !val;
        foreach (CanvasGroup cg in canvasGroups)
        {
            cg.interactable = val;
            cg.blocksRaycasts = val;
        }
    }

    private GameObject GetFirstActiveObject(Transform parent)
    {
        foreach (Transform child in container.transform)
        {
            if (child.gameObject.activeSelf)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public void PlaySound(AudioClip clip, float volume)
    {
        audioSource.volume = volume;
        audioSource.PlayOneShot(clip);
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
