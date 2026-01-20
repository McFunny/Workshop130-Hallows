using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ToolTipScript : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI itemName, itemDesc, itemStamina, itemType, canStack, chanceToBreak;
    public Color c_default, c_tool, c_placeable, c_crop, c_consumable, c_ammo, c_bug;
    public GameObject intakeParent, outputParent;
    public GameObject[] input, output;
    [SerializeField] private GameObject[] barterIcons;
    [SerializeField] private Sprite mintImage;

    [Header("Only needed for barter tooltips")]
    [SerializeField] private Image[] barterIconImages;
    [SerializeField] private TextMeshProUGUI[] barterIconTexts;
    private WaypointScript shopUI;
    [SerializeField] private List<VerticalLayoutGroup> verticalLayoutGroups = new List<VerticalLayoutGroup>();

    //[Header("Only needed for crafting tooltips")]

    //protected Vector3[] corners;

    public void Awake()
    {
        //corners = new Vector3[4];
        //eventSystem = EventSystem.current;
    }

    void Start()
    {
        //shopUI = FindFirstObjectByType<WaypointScript>();
        input = new GameObject[6];
        output = new GameObject[4];

        if (barterIcons.Length > 0)
        {
            barterIconImages = new Image[barterIcons.Length];
            barterIconTexts = new TextMeshProUGUI[barterIcons.Length];
            for (int i = 0; i < barterIcons.Length; i++)
            {
                barterIconImages[i] = barterIcons[i].GetComponentInChildren<Image>();
                barterIconTexts[i] = barterIcons[i].GetComponentInChildren<TextMeshProUGUI>();

                barterIcons[i].SetActive(false);
            }
        }

        foreach (var layoutGroup in GetComponentsInChildren<VerticalLayoutGroup>())
        {
            verticalLayoutGroups.Add(layoutGroup);
        }

        for (int i = 0; i < 6; i++)
        {
            input[i] = intakeParent.transform.GetChild(1).GetChild(i).gameObject;
        }

        for (int i = 0; i < 4; i++)
        {
            output[i] = outputParent.transform.GetChild(1).GetChild(i).gameObject;
        }

        panel.SetActive(false);
    }

    protected void LateUpdate() //keeping this just in case
    {
        /*if(!ControlManager.isGamepad)
        {
            pos = Input.mousePosition;
        }
        else
        {
            if(eventSystem.currentSelectedGameObject != null)
            {
                pos = new Vector3(eventSystem.currentSelectedGameObject.transform.position.x + 50, eventSystem.currentSelectedGameObject.transform.position.y + 50, eventSystem.currentSelectedGameObject.transform.position.z);
            }
        }
        
        
        ((RectTransform) transform).GetWorldCorners(corners);
        var width = corners[2].x - corners[0].x;
        var height = corners[1].y - corners[0].y;

        var distPastX = pos.x + width - Screen.width;
        if (distPastX > 0)
            pos = new Vector3(pos.x - distPastX, pos.y, pos.z);
        var distPastY = pos.y - height;
        if (distPastY < 0)
            pos = new Vector3(pos.x, pos.y - distPastY, pos.z);

        transform.position = pos;*/
    }
    public void UpdateToolTip(InventoryItemData itemData, bool isCraft = false, float currentDurability = -1f)
    {
        if (itemData == null || !panel.activeSelf) return;

        var type = itemData.type;

        intakeParent.SetActive(false);
        outputParent.SetActive(false);
        itemStamina.gameObject.SetActive(false);
        canStack.gameObject.SetActive(false);
        chanceToBreak.gameObject.SetActive(false);
        itemType.color = c_default;

        switch (type)
        {
            case ItemType.Misc:
                itemType.text = "Misc";
                break;

            case ItemType.Consumable:
                itemType.text = "Consumable";
                if(itemData.staminaValue > 0)
                {
                    itemStamina.text = "Heals " + itemData.staminaValue + " stamina.";
                    itemStamina.gameObject.SetActive(true);
                }
                else itemStamina.gameObject.SetActive(false);
                itemType.color = c_consumable;
                break;

            case ItemType.Tool:
                itemType.text = "Tool";
                itemType.color = c_tool;
                break;

            case ItemType.Structure:
                itemType.text = "Structure";
                itemType.color = c_placeable;
                break;

            case ItemType.BarnStructure:
                itemType.text = "Barn Structure";
                itemType.color = c_placeable;
                break;

            case ItemType.CabinDecor:
                itemType.text = "Cabin Decor";
                itemType.color = c_placeable;
                break;

            case ItemType.Seed: //help
                itemType.text = "Seed";
                var seedData = itemData as CropItem; //why did I name it like this

                if(isCraft)
                {
                    itemType.color = c_crop;
                    break;
                }
                //Consumes

                if (seedData.cropData.gloamIntake > 0) { input[0].SetActive(true); }
                else { input[0].SetActive(false); }

                if (seedData.cropData.terraIntake > 0) { input[1].SetActive(true); }
                else { input[1].SetActive(false); }

                if (seedData.cropData.ichorIntake > 0) { input[2].SetActive(true); }
                else { input[2].SetActive(false); }

                if (seedData.cropData.waterIntake > 0) { input[3].SetActive(true); }
                else { input[3].SetActive(false); }

                if (seedData.cropData.requirePollination) { input[4].SetActive(true); }
                else { input[4].SetActive(false); }

                if (seedData.requireTrellis) { input[5].SetActive(true); }
                else { input[5].SetActive(false); }

                //Produces

                if (seedData.cropData.gloamIntake < 0) { output[0].SetActive(true); }
                else { output[0].SetActive(false); }

                if (seedData.cropData.terraIntake < 0) { output[1].SetActive(true); }
                else { output[1].SetActive(false); }

                if (seedData.cropData.ichorIntake < 0) { output[2].SetActive(true); }
                else { output[2].SetActive(false); }

                if (seedData.cropData.waterIntake < 0) { output[3].SetActive(true); }
                else { output[3].SetActive(false); }

                intakeParent.SetActive(true);
                outputParent.SetActive(true);
                itemType.color = c_crop;
                break;

            case ItemType.Ammo:
                itemType.text = "Ammo";
                itemType.color = c_ammo;
                break;

            case ItemType.Creature:
                itemType.text = "Creature";
                itemType.color = c_default;
                break;

            case ItemType.Bug:
                itemType.text = "Bug";
                itemType.color = c_bug;
                break;

            case ItemType.Throwable:
                itemType.text = "Throwable";
                itemType.color = c_ammo;
                break;
            
            case ItemType.Trinket:
                itemType.text = "Trinket";
                
                var trinket = itemData as TrinketItem;

                if (trinket.stackable) canStack.gameObject.SetActive(true);
                else canStack.gameObject.SetActive(false);

                if(currentDurability == -1f)
                {
                    chanceToBreak.gameObject.SetActive(false);
                }
                else if(currentDurability >= 0f)
                {
                    float chance = (currentDurability / trinket.maxDurability) * 100f;
                    if (chance > 100f) chance = 100;
                    chance = 100f - chance;
                    
                    if(chance == 0f) chanceToBreak.text = "<color=green>" + Mathf.RoundToInt(chance) + "%</color> chance to break when unequipped";
                    else if(chance > 0f && chance < 50f) chanceToBreak.text = "<color=yellow>" + Mathf.RoundToInt(chance) + "%</color> chance to break when unequipped";
                    else chanceToBreak.text = "<color=red>" + Mathf.RoundToInt(chance) + "%</color> chance to break when unequipped";

                    chanceToBreak.gameObject.SetActive(true);
                }
                else chanceToBreak.gameObject.SetActive(false);
                
                itemType.color = c_bug;
                break;

            default:
                itemType.text = "Misc";
                break;
        }

        itemName.text = itemData.displayName;
        itemDesc.text = itemData.description;

        for (int i = 0; i < verticalLayoutGroups.Count; i++)
        {
            Canvas.ForceUpdateCanvases();
            verticalLayoutGroups[i].enabled = false;
            verticalLayoutGroups[i].enabled = true;
        }
    }

    public void UpdateToolTip(InventorySlot slot, bool isCraft = false)
    {
        float durability = TrinketInventoryHandler.Instance.GetTrinketDurability(slot);
        UpdateToolTip(slot.ItemData, false, durability);
    }

    public void UpdateTooltipBarter(InventoryItemData item, List<ItemWithAmount> barterCost, int cost)
    {
        UpdateToolTip(item);

        if (cost > 0)
        {
            barterIcons[0].SetActive(true);
            barterIconImages[0].sprite = mintImage;
            barterIconTexts[0].text = "x" + cost.ToString();
        }
        else
        {
            barterIcons[0].SetActive(false);
        }

        if (barterCost == null) return;

        for (int i = 1; i < barterIcons.Length; i++)
        {
            if (i > barterCost.Count)
            {
                barterIconImages[i].sprite = null;
                barterIconTexts[i].text = "";
                barterIcons[i].SetActive(false);
            }
            else
            {
                barterIcons[i].SetActive(true);
                barterIconImages[i].sprite = barterCost[i - 1].item.icon;
                barterIconTexts[i].text = "x" + barterCost[i - 1].amount.ToString();
            }
        }
    }

    public void UpdateTooltipCraft(CraftingEntry entry)
    {
        for (int i = 0; i < barterIconImages.Length; i++)
        {
            barterIconImages[i].gameObject.SetActive(false);
            barterIconTexts[i].gameObject.SetActive(false);
        }

        int count = 0;

        if (entry.mintCost > 0)
        {
            barterIconImages[count].sprite = mintImage;
            barterIconTexts[count].text = "x" + entry.mintCost + " (" + PlayerInteraction.Instance.currentMoney + ")";
            barterIconImages[count].gameObject.SetActive(true);
            barterIconTexts[count].gameObject.SetActive(true);
            count++;
        }

        foreach (CraftingRequirement requirement in entry.craftingRequirements)
        {
            barterIconImages[count].sprite = requirement.requiredItem.icon;
            barterIconTexts[count].text = "x" + requirement.requiredAmount + " (" + PlayerInventoryHolder.Instance.ReturnItemCountInPlayerInventory(requirement.requiredItem) + ")";
            barterIconImages[count].gameObject.SetActive(true);
            barterIconTexts[count].gameObject.SetActive(true);
            count++;
        }
    }
}
