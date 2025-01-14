using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventorySlot_UI : MonoBehaviour
{
    [SerializeField] private Image itemSprite;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemCount;
    [SerializeField] public GameObject slotHighlight;
    [SerializeField] private InventorySlot assignedInventorySlot;

    public InventorySlot AssignedInventorySlot => assignedInventorySlot;
    public InventoryDisplay ParentDisplay { get; private set; }
    ControlManager controlManager;
    bool isSelected;
    ToolTipScript toolTip;
    string itemDesc;

    Button button;

    private void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        ClearSlot();
        button = GetComponent<Button>();
        ParentDisplay = transform.parent.GetComponent<InventoryDisplay>();
        toolTip = FindFirstObjectByType<ToolTipScript>();
        AddEventTriggers();
    }
    void Start()
    {
        //toolTip = FindObjectOfType<ToolTipScript>();
    }
    private void OnEnable()
    {
        controlManager.select.action.started += Select;
        controlManager.split.action.started += Split;
    }
    private void OnDisable()
    {
        controlManager.select.action.started -= Select;
        controlManager.split.action.started -= Split;
    }

    void Update()
    {
        if(PlayerMovement.accessingInventory)
        {
            button.enabled = true;
        }
        else
        {
            button.enabled = false;
        }

        //print(EventSystem.current.currentSelectedGameObject);
        if(PlayerMovement.accessingInventory && ControlManager.isGamepad)
        {
            slotHighlight.SetActive(isSelected);
            itemName.gameObject.SetActive(isSelected);

            if(isSelected)
            {
                if(itemName.text != "")
                {
                    if(itemDesc!= null){toolTip.UpdateToolTip(itemDesc);}
                    toolTip.panel.SetActive(true);
                }
                else
                {
                    toolTip.panel.SetActive(false);
                }          
            }
        }
        if(!PlayerMovement.accessingInventory)
        {
            itemName.gameObject.SetActive(false);
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
        if(PlayerMovement.accessingInventory == true)
        {
           OnLeftUISlotClick();
        }     
    }
    private void Split(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.accessingInventory == true)
        {
            OnRightUISlotClick();
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
        if(isSelected){ParentDisplay?.HandleSlotLeftClick(this);}
    }

    public void OnRightUISlotClick()
    {
        // Handle right-click behavior
        if(isSelected){ParentDisplay?.HandleSlotRightClick(this);}
    }

    private void OnHighlight(bool selected)
    {
        if(!ControlManager.isGamepad)
        {
            slotHighlight.SetActive(selected);
            itemName.gameObject.SetActive(selected);
            if(selected)
            {
                if(itemName.text != "")
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
            if(itemDesc!= null){toolTip.UpdateToolTip(itemDesc);}
        }
            
    }

    public void Init(InventorySlot slot)
    {
        assignedInventorySlot = slot;
        UpdateUISlot(slot);
    }

    public void UpdateUISlot(InventorySlot slot)
    {
        if (slot.ItemData != null)
        {
            itemSprite.sprite = slot.ItemData.icon;
            itemSprite.color = Color.white;
            itemName.text = slot.ItemData.name;
            itemDesc = slot.ItemData.description;
            if (slot.StackSize > 1)
                itemCount.text = slot.StackSize.ToString();
            else
                itemCount.text = "";
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
        itemSprite.sprite = null;
        itemSprite.color = Color.clear;
        itemCount.text = "";
        itemName.text = "";
        itemDesc = "";
        itemName.gameObject.SetActive(false);
    }
}
