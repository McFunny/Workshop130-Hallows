using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;

public class ToolTipScript : MonoBehaviour
{
    public GameObject toolTip, panel;
    public TextMeshProUGUI itemName, itemDesc, itemStamina, itemType;
    public Color c_default, c_tool, c_placeable, c_crop, c_consumable;
    public GameObject intakeParent, outputParent;
    public GameObject[] input, output;
    //protected Vector3[] corners;

    public void Awake()
    {
        //corners = new Vector3[4];
        //eventSystem = EventSystem.current;
    }

    void Start()
    {
        input = new GameObject[4];
        output = new GameObject[4];

        for(int i = 0; i < 4; i++)
        {
            input[i] = intakeParent.transform.GetChild(1).GetChild(i).gameObject;
        }

        for(int i = 0; i < 4; i++)
        {
            output[i] = outputParent.transform.GetChild(1).GetChild(i).gameObject;
        }
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
        var type = itemData.GetType();

        if(itemData == null || !panel.activeSelf) return;

        if(itemData.staminaValue != 0)
        {
            itemStamina.text = "Heals " + itemData.staminaValue + " stamina.";
            itemStamina.gameObject.SetActive(true);
            itemType.text = "Consumable";
            intakeParent.SetActive(false);
            itemType.color = c_consumable;
        }
        else if(type.Equals(typeof(ToolItem)))
        {
            itemType.text = "Tool";
            intakeParent.SetActive(false);
            itemStamina.gameObject.SetActive(false);
            itemType.color = c_tool;
        }
        else if(type.Equals(typeof(PlaceableItem)))
        {
            itemType.text = "Structure";
            intakeParent.SetActive(false);
            itemStamina.gameObject.SetActive(false);
            itemType.color = c_placeable;
        }
        else if(type.Equals(typeof(CropItem)))
        {
            itemType.text = "Seed";
            var seedData = itemData as CropItem; //why did I name it like this

            //Consumes

            if(seedData.cropData.gloamIntake > 0){input[0].SetActive(true);}
            else{input[0].SetActive(false);}

            if(seedData.cropData.terraIntake > 0){input[1].SetActive(true);}
            else{input[1].SetActive(false);}

            if(seedData.cropData.ichorIntake > 0){input[2].SetActive(true);}
            else{input[2].SetActive(false);}

            if(seedData.cropData.waterIntake > 0){input[3].SetActive(true);}
            else{input[3].SetActive(false);}

            //Produces

            if(seedData.cropData.gloamIntake < 0){output[0].SetActive(true);}
            else{output[0].SetActive(false);}

            if(seedData.cropData.terraIntake < 0){output[1].SetActive(true);}
            else{output[1].SetActive(false);}

            if(seedData.cropData.ichorIntake < 0){output[2].SetActive(true);}
            else{output[2].SetActive(false);}

            if(seedData.cropData.waterIntake < 0){output[3].SetActive(true);}
            else{output[3].SetActive(false);}
            
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
        
    }
}
