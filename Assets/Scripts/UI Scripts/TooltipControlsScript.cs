using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipControlsScript : MonoBehaviour
{
    [SerializeField] private GameObject container;
    public TextMeshProUGUI[] containerArray;
    [SerializeField] private TextMeshProUGUI textBox;

    public void SelectedItem()
    {  
        if (HotbarDisplay.currentSlot.AssignedInventorySlot != null && HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData != null)
        {
            DestoryTextObjects();

            ToolItem t_item = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData as ToolItem;
            if (t_item)
            {
                if (t_item.itemInputsKBM.Count != 0)
                {
                    container.SetActive(true);
                    for(int i = 0; i < t_item.itemInputsKBM.Count; i++)
                    {
                        var newObj = Instantiate(textBox, container.transform, worldPositionStays:false);
                        newObj.text = t_item.itemInputsKBM[i];
                    }
                }
                else
                {
                    container.SetActive(false);
                }    
            }
            else
            {    
                InventorySlot_UI o_item = HotbarDisplay.currentSlot;
                if(o_item.AssignedInventorySlot.ItemData.itemInputsKBM.Count != 0)
                {
                    container.SetActive(true);
                    for (int i = 0; i < o_item.AssignedInventorySlot.ItemData.itemInputsKBM.Count; i++)
                    {
                        var newObj = Instantiate(textBox, container.transform, worldPositionStays:false);
                        newObj.text = o_item.AssignedInventorySlot.ItemData.itemInputsKBM[i];
                    }
                }
                else
                {
                    container.SetActive(false);
                }
            }
        }
        else
        {
            DestoryTextObjects();
            container.SetActive(false);
        }
    }

    public void DestoryTextObjects()
    {
        containerArray = GetComponentsInChildren<TextMeshProUGUI>();
        for(int i = 0; i < containerArray.Length; i++)
        {
            Destroy(containerArray[i].gameObject);
        }
    }
}
