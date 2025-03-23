using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipControlsScript : MonoBehaviour
{
    [SerializeField] private GameObject KBMContainer, controllerContainer, defaultContainerKBM, defaultContainerController;
    public List<GameObject> kbmContainerList, controllerContainerList, defaultKBMContainerList, defaultControllerContainerList;
    [SerializeField] private TextMeshProUGUI textBox;
    public List<string> DefaultControlsKBM, DefaultControlsController, InventoryControlsKBM, InventoryControlsController;
    private bool isEmpty;

    void Start()
    {
        ShowDefaultControls();
    }

    void Update()
    {
        if(ControlManager.isController && controllerContainerList.Count != 0) 
        {
            controllerContainer.SetActive(true);
            KBMContainer.SetActive(false);
        }
        else if(!ControlManager.isController && kbmContainerList.Count != 0)
        {
            controllerContainer.SetActive(false);
            KBMContainer.SetActive(true);
        }
        else
        {
            controllerContainer.SetActive(false);
            KBMContainer.SetActive(false);
        }

        if(ControlManager.isController)
        {
            defaultContainerController.SetActive(true);
            defaultContainerKBM.SetActive(false);
        }
        else
        {
            defaultContainerController.SetActive(false);
            defaultContainerKBM.SetActive(true);
        }
    }
    

    public void SelectedItem()
    {  
        return; //disables item specific tooltips currently
        if (HotbarDisplay.currentSlot.AssignedInventorySlot != null && HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData != null)
        {
            DestoryTextObjects();

            ToolItem t_item = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData as ToolItem;
            if (t_item)
            {
                if (t_item.itemInputsKBM.Count != 0 || t_item.itemInputsController.Count != 0)
                {
                    isEmpty = false;
                    for(int i = 0; i < t_item.itemInputsKBM.Count; i++)
                    {
                        var newObj = Instantiate(textBox, KBMContainer.transform, worldPositionStays:false);
                        newObj.text = t_item.itemInputsKBM[i];
                        kbmContainerList.Add(newObj.gameObject);
                    }

                    if (t_item.itemInputsController.Count != 0)
                    {
                        for(int i = 0; i < t_item.itemInputsController.Count; i++)
                        {
                            var newObj = Instantiate(textBox, controllerContainer.transform, worldPositionStays:false);
                            newObj.text = t_item.itemInputsController[i];
                            controllerContainerList.Add(newObj.gameObject);
                        }
                    }
                    
                }
                else
                {
                    isEmpty = true;
                }    
            }
            else
            {    
                InventorySlot_UI o_item = HotbarDisplay.currentSlot;
                if(o_item.AssignedInventorySlot.ItemData.itemInputsKBM.Count != 0 || o_item.AssignedInventorySlot.ItemData.itemInputsController.Count != 0)
                {
                    isEmpty = false;
                    for (int i = 0; i < o_item.AssignedInventorySlot.ItemData.itemInputsKBM.Count; i++)
                    {
                        var newObj = Instantiate(textBox, KBMContainer.transform, worldPositionStays:false);
                        newObj.text = o_item.AssignedInventorySlot.ItemData.itemInputsKBM[i];
                        kbmContainerList.Add(newObj.gameObject);
                    }
                    if (o_item.AssignedInventorySlot.ItemData.itemInputsController.Count != 0)
                    {
                        for(int i = 0; i < o_item.AssignedInventorySlot.ItemData.itemInputsController.Count; i++)
                        {
                            var newObj = Instantiate(textBox, controllerContainer.transform, worldPositionStays:false);
                            newObj.text =o_item.AssignedInventorySlot.ItemData.itemInputsController[i];
                            controllerContainerList.Add(newObj.gameObject);
                        }
                    }
                    
                }
                else
                {
                    isEmpty = true;
                }
            }
        }
        else
        {
            DestoryTextObjects();
            isEmpty = true;
        }
    }

    public void DestoryTextObjects()
    {
        for(int i = 0; i < kbmContainerList.Count; i++)
        {
            Destroy(kbmContainerList[i].gameObject);
        }

        for(int i = 0; i < controllerContainerList.Count; i++)
        {
            Destroy(controllerContainerList[i].gameObject);
        }

        kbmContainerList.Clear();
        controllerContainerList.Clear();
    }

    public void DestoryDefaultTextObjects() //For The Left Side Tooltips
    {
        if(defaultKBMContainerList.Count == 0 || defaultControllerContainerList.Count == 0) return;

        for(int i = 0; i < defaultKBMContainerList.Count; i++)
        {
            Destroy(defaultKBMContainerList[i].gameObject);
        }

        for(int i = 0; i < defaultControllerContainerList.Count; i++)
        {
            Destroy(defaultControllerContainerList[i].gameObject);
        }

        defaultKBMContainerList.Clear();
        defaultControllerContainerList.Clear();
    }
    

    public void ShowDefaultControls()
    {
        DestoryDefaultTextObjects();

        for(int i = 0; i < DefaultControlsKBM.Count; i++)
        {
            var tempObj = Instantiate(textBox, defaultContainerKBM.transform, worldPositionStays:false);
            tempObj.text = DefaultControlsKBM[i];
            defaultKBMContainerList.Add(tempObj.gameObject);
        }
        
        for(int i = 0; i < DefaultControlsController.Count; i++)
        {
            var tempObj = Instantiate(textBox, defaultContainerController.transform, worldPositionStays:false);
            tempObj.text = DefaultControlsController[i];
            defaultControllerContainerList.Add(tempObj.gameObject);
        }
    }

    public void ShowInventoryControls()
    {
        DestoryDefaultTextObjects();

        for(int i = 0; i < InventoryControlsKBM.Count; i++)
        {
            var tempObj = Instantiate(textBox, defaultContainerKBM.transform, worldPositionStays:false);
            tempObj.text = InventoryControlsKBM[i];
            defaultKBMContainerList.Add(tempObj.gameObject);
        }
        
        for(int i = 0; i < InventoryControlsController.Count; i++)
        {
            var tempObj = Instantiate(textBox, defaultContainerController.transform, worldPositionStays:false);
            tempObj.text = InventoryControlsController[i];
            defaultControllerContainerList.Add(tempObj.gameObject);
        }
    }
}
