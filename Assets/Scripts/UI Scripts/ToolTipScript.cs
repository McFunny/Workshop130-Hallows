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
    public GameObject gloam, terra, ichor, water, intakeParent;
    //protected Vector3[] corners;

    public void Awake()
    {
        //corners = new Vector3[4];
        //eventSystem = EventSystem.current;
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
        }
        else
        {
            itemStamina.gameObject.SetActive(false);
        }

        if(type.Equals(typeof(ToolItem)))
        {
            itemType.text = "Tool";
            intakeParent.SetActive(false);
        }
        else if(type.Equals(typeof(PlaceableItem)))
        {
            itemType.text = "Structure";
            intakeParent.SetActive(false);
        }
        else if(type.Equals(typeof(CropItem)))
        {
            itemType.text = "Seed";
            var seedData = itemData as CropItem; //why did I name it like this

            if(seedData.cropData.gloamIntake > 0){gloam.SetActive(true);}
            else{gloam.SetActive(false);}

            if(seedData.cropData.terraIntake > 0){terra.SetActive(true);}
            else{terra.SetActive(false);}

            if(seedData.cropData.ichorIntake > 0){ichor.SetActive(true);}
            else{ichor.SetActive(false);}

            if(seedData.cropData.waterIntake > 0){water.SetActive(true);}
            else{water.SetActive(false);}
            
            intakeParent.SetActive(true);
        }
        else
        {
            itemType.text = "Sellable";
            intakeParent.SetActive(false);
        }

        //if(itemData.GetType)

        itemName.text = itemData.displayName;
        itemDesc.text = itemData.description;
        
    }
}
