using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class ToolTipScript : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI itemName, itemDesc, itemStamina, itemType;
    public Color c_default, c_tool, c_placeable, c_crop, c_consumable;
    public GameObject intakeParent, outputParent;
    public GameObject[] input, output;
    [SerializeField] private GameObject[] barterIcons;

    [Header("Only needed for barter tooltips")]
    [SerializeField] private Sprite mintImage;
    [SerializeField] private Image[] barterIconImages;
    [SerializeField] private TextMeshProUGUI[] barterIconTexts;
    private WaypointScript shopUI;
    [SerializeField] private List<VerticalLayoutGroup> verticalLayoutGroups = new List<VerticalLayoutGroup>();
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

    protected void LateUpdate()
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
    public void UpdateToolTip(InventoryItemData itemData)
    {
        if (itemData == null || !panel.activeSelf) return;

        var type = itemData.GetType();

        if (itemData.staminaValue != 0)
        {
            itemStamina.text = "Heals " + itemData.staminaValue + " stamina.";
            itemStamina.gameObject.SetActive(true);
            itemType.text = "Consumable";
            intakeParent.SetActive(false);
            outputParent.SetActive(false);
            itemType.color = c_consumable;
        }
        else if (type.Equals(typeof(ToolItem)))
        {
            itemType.text = "Tool";
            intakeParent.SetActive(false);
            outputParent.SetActive(false);
            itemStamina.gameObject.SetActive(false);
            itemType.color = c_tool;
        }
        else if (type.Equals(typeof(PlaceableItem)))
        {
            var item = itemData as PlaceableItem;
            //print(item);
            if (item.gridTypes.Count == 0) Debug.LogError("Forgot to assign this structure a grid type!");
            else if (item.gridTypes[0] == GridType.Any) itemType.text = "Structure";
            else if (item.gridTypes[0] == GridType.Farm) itemType.text = "Farm Structure";
            else if (item.gridTypes[0] == GridType.Cabin) itemType.text = "Cabin Structure";
            else if (item.gridTypes[0] == GridType.Town) itemType.text = "Town Structure";

            intakeParent.SetActive(false);
            outputParent.SetActive(false);
            itemStamina.gameObject.SetActive(false);
            itemType.color = c_placeable;
        }
        else if (type.Equals(typeof(CropItem)))
        {
            itemType.text = "Seed";
            var seedData = itemData as CropItem; //why did I name it like this

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
            itemStamina.gameObject.SetActive(false);
            itemType.color = c_crop;
        }
        else
        {
            itemType.text = "Misc";
            intakeParent.SetActive(false);
            outputParent.SetActive(false);
            itemStamina.gameObject.SetActive(false);
            itemType.color = c_default;
        }

        //if(itemData.GetType)

        itemName.text = itemData.displayName;
        itemDesc.text = itemData.description;

        /*itemName.gameObject.SetActive(true);
        itemType.gameObject.SetActive(true);
        itemDesc.gameObject.SetActive(true);*/

        for (int i = 0; i < verticalLayoutGroups.Count; i++)
        {
            Canvas.ForceUpdateCanvases();
            verticalLayoutGroups[i].enabled = false;
            verticalLayoutGroups[i].enabled = true;
        }

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
}
