using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class InventorySlot_UI : MonoBehaviour
{
    [SerializeField] private Image itemSprite, itemGrey;
    [SerializeField] private Slider foodCooldownSlider;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemCount;
    [SerializeField] public GameObject slotHighlight;
    [SerializeField] private InventorySlot assignedInventorySlot;
    [SerializeField] private Animator pickupAnim;
    public Slider durabilitySlider;
    public UISpriteAnim anim1, anim2;
    

    public InventorySlot AssignedInventorySlot => assignedInventorySlot;
    public InventoryDisplay ParentDisplay { get; private set; }
    ControlManager controlManager;
    bool isSelected;
    ToolTipScript toolTip; //Handles hovering item in inventory
    private InventoryAnims inventoryAnims;

    private Coroutine flashingCoroutine;
    private Image sliderFill;
    string itemDesc;
    Button button;

    private void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        ClearSlot();
        button = GetComponent<Button>();
        ParentDisplay = transform.parent.GetComponent<InventoryDisplay>();
        toolTip = GameObject.Find("InventoryItemDescriptions").GetComponent<ToolTipScript>();
        inventoryAnims = FindFirstObjectByType<InventoryAnims>();
        AddEventTriggers();
        itemName.gameObject.SetActive(false);
        itemGrey.enabled = false;
        foodCooldownSlider.value = 0;
        sliderFill = durabilitySlider.fillRect.GetComponent<Image>();
        if(transform.parent.gameObject.name == "PlayerTrinkets")
        {
            assignedInventorySlot.acceptedItemType = InventorySlot.AcceptedItemType.Trinket;
            TrinketInventoryHandler.Instance.OnInventoryUpdate(this, assignedInventorySlot);
        }
        else
        {
            assignedInventorySlot.acceptedItemType = InventorySlot.AcceptedItemType.Everything;
        }
        //Debug.Log(ParentDisplay.gameObject.name);
    }

    private void OnEnable()
    {
        controlManager.select.action.started += Select;
        controlManager.split.action.started += Split;

        controlManager.hotbarUp.action.started += LeftBumper;
        controlManager.hotbarDown.action.started += RightBumper;
        PlayerInventoryHolder.onItemAddedToInventory += OnItemAdded;

        //PlayerInteraction.onFoodConsumed += SetFoodCooldown;
    }
    private void OnDisable()
    {
        controlManager.select.action.started -= Select;
        controlManager.split.action.started -= Split;

        controlManager.hotbarUp.action.started -= LeftBumper;
        controlManager.hotbarDown.action.started -= RightBumper;
        PlayerInventoryHolder.onItemAddedToInventory -= OnItemAdded;

        //PlayerInteraction.onFoodConsumed -= SetFoodCooldown;
    }

    void Update()
    {
        if (PlayerMovement.accessingInventory)
        {
            button.enabled = true;
        }
        else
        {
            button.enabled = false;
        }

        //print(EventSystem.current.currentSelectedGameObject);
        if (PlayerMovement.accessingInventory && ControlManager.isGamepad)
        {
            slotHighlight.SetActive(isSelected);

            //itemName.gameObject.SetActive(isSelected);

            if (isSelected)
            {
                if (itemName.text != "")
                {
                    if (itemDesc != null) { toolTip.UpdateToolTip(assignedInventorySlot); }
                    toolTip.panel.SetActive(true);
                }
                else
                {
                    toolTip.panel.SetActive(false);
                }
            }
        }
        if (!PlayerMovement.accessingInventory)
        {
            itemName.gameObject.SetActive(false);
            if (HotbarDisplay.currentSlot == this) { slotHighlight.SetActive(true); }
        }

        if (assignedInventorySlot.ItemData == null)
        {
            foodCooldownSlider.gameObject.SetActive(false);
            itemGrey.enabled = false;
            return;
        }

        if (assignedInventorySlot.ItemData.useCooldown > 0)
        {
            FoodCooldownHandler();
        }
            
    }

    public void TestPrint()
    {
        print("Test");
    }


    // Add EventTrigger component and setup event listeners for highlight detection and clicks
    private void AddEventTriggers()
    {
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();

        // PointerEnter (highlighted)
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => { OnHighlight(true); });
        trigger.triggers.Add(pointerEnter);

        // PointerExit (no longer highlighted)
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => { OnHighlight(false); });
        trigger.triggers.Add(pointerExit);

        // PointerClick (detect left and right mouse clicks)
        EventTrigger.Entry pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        //pointerClick.callback.AddListener((eventData) => OnPointerClick((PointerEventData)eventData));
        trigger.triggers.Add(pointerClick);
    }

    private void Select(InputAction.CallbackContext obj)
    {
        //print("SelectCheck");
        if (PlayerMovement.accessingInventory == true)
        {
            OnLeftUISlotClick();
        }
    }
    private void Split(InputAction.CallbackContext obj)
    {
        if (PlayerMovement.accessingInventory == true)
        {
            OnRightUISlotClick();
        }
    }

    private void LeftBumper(InputAction.CallbackContext obj)
    {
        //print("SelectCheck");
        if (PlayerMovement.accessingInventory == true)
        {
            OnLeftUISlotBumper();
        }
    }
    private void RightBumper(InputAction.CallbackContext obj)
    {
        if (PlayerMovement.accessingInventory == true)
        {
            OnRightUISlotBumper();
        }
    }

    public void Selected()
    {
        isSelected = true;
    }

    public void Deselected()
    {
        isSelected = false;
    }


    public void OnLeftUISlotClick()
    {
        // Handle left-click behavior
        if (isSelected) { ParentDisplay?.HandleSlotLeftClick(this); }
    }

    public void OnRightUISlotClick()
    {
        // Handle right-click behavior
        if (isSelected) { ParentDisplay?.HandleSlotRightClick(this); }
    }

    public void OnLeftUISlotBumper()
    {
        // Handle left-Bumper behavior
        if (isSelected) { ParentDisplay?.HandleLeftBumper(this); }
    }

    public void OnRightUISlotBumper()
    {
        // Handle right-Bumper behavior
        if (isSelected) { ParentDisplay?.HandleRightBumper(this); }
    }

    private void OnHighlight(bool selected)
    {
        if (!ControlManager.isGamepad)
        {
            slotHighlight.SetActive(selected);
            //itemName.gameObject.SetActive(selected);
            if (selected)
            {
                if (itemName.text != "")
                {
                    toolTip.panel.SetActive(true);
                }
                else
                {
                    toolTip.panel.SetActive(false);
                }
            }
            else
            {
                toolTip.panel.SetActive(false);
            }
            if (assignedInventorySlot.ItemData != null) { toolTip.UpdateToolTip(assignedInventorySlot); }
        }

    }

    public void Init(InventorySlot slot)
    {
        assignedInventorySlot = slot;
        slot.uiSlot = this;
        UpdateUISlot(slot);
    }

    public void UpdateUISlot(InventorySlot slot)
    {
        foodCooldownSlider.gameObject.SetActive(false);
        itemGrey.enabled = false;
        if (slot.ItemData != null)
        {
            itemSprite.sprite = slot.ItemData.icon;
            itemSprite.color = Color.white;
            itemName.text = slot.ItemData.displayName;
            itemDesc = slot.ItemData.description;
            if (slot.StackSize > 1)
                itemCount.text = slot.StackSize.ToString();
            else
                itemCount.text = "";

            //Debug.Log("Kevin: Parent slot is " + transform.parent.gameObject.name);
            if(transform.parent.gameObject.name == "PlayerTrinkets")
            {
                slot.acceptedItemType = InventorySlot.AcceptedItemType.Trinket;
                TrinketInventoryHandler.Instance.OnInventoryUpdate(this, assignedInventorySlot);
                if(slot.ItemData != null)
                {
                    if (flashingCoroutine != null)
                    {
                        StopCoroutine(flashingCoroutine);
                        itemSprite.color = Color.white;
                    }
                    TrinketItem trinket = slot.ItemData as TrinketItem;
                    var durability = TrinketInventoryHandler.Instance.GetTrinketDurability(slot);

                    if(durability <= 1)
                    {
                        flashingCoroutine = StartCoroutine(TrinketSlotFlashing());
                        sliderFill.color = Color.red;
                    }
                    else if(durability <= trinket.maxDurability * 0.25f)
                    {
                        sliderFill.color = Color.red;
                    }
                    else if(durability <= trinket.maxDurability * 0.5f)
                    {
                        sliderFill.color = Color.yellow;
                    }
                    else
                    {
                        sliderFill.color = Color.green;
                    }
                }
                if(slot.ItemData == null)
                {
                    itemSprite.color = Color.clear;
                }
                
            }
            else
            {
                slot.acceptedItemType = InventorySlot.AcceptedItemType.Everything;
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ToggleHighlight()
    {
        slotHighlight.SetActive(!slotHighlight.activeInHierarchy);
    }

    public void UpdateUISlot()
    {
        if (assignedInventorySlot != null)
        {
            UpdateUISlot(assignedInventorySlot);
            //print(assignedInventorySlot);
        }
    }

    public void ClearSlot()
    {
        assignedInventorySlot?.ClearSlot();
        if(flashingCoroutine != null) StopCoroutine(flashingCoroutine);
        itemSprite.sprite = null;
        itemSprite.color = Color.clear;
        itemCount.text = "";
        itemName.text = "";
        itemDesc = "";
        foodCooldownSlider.gameObject.SetActive(false);
        itemGrey.enabled = false;
        durabilitySlider.gameObject.SetActive(false);
        durabilitySlider.value = 0;
        //itemName.gameObject.SetActive(false);
    }

    private void FoodCooldownHandler()
    {
        if (inventoryAnims.isFoodCooldownActive)
        {
            foodCooldownSlider.gameObject.SetActive(true);
            foodCooldownSlider.value = inventoryAnims.foodCooldownPercent;
            itemGrey.enabled = true;
        }
        else
        {
            foodCooldownSlider.gameObject.SetActive(false);
            itemGrey.enabled = false;
        }
    }

    private IEnumerator TrinketSlotFlashing()
    {
        while (true)
        {
            float flashDuration = 0.5f;
            float elapsedTime = 0f;
            Color originalColor = Color.white;
            Color flashColor = Color.red;

            while (elapsedTime < flashDuration)
            {
                itemSprite.color = Color.Lerp(originalColor, flashColor, Mathf.PingPong(elapsedTime * 4f, 1f));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            itemSprite.color = originalColor;
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnItemAdded(InventorySlot slot)
    {
        if (slot != assignedInventorySlot) return;

        pickupAnim.Play("ItemPickup");
        print("slot animated");
    }
}
