using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MouseItemData : MonoBehaviour
{
    public Image itemSprite;
    public TextMeshProUGUI itemCount;
    public InventorySlot assignedInventorySlot;
    ControlManager controlManager;
    EventSystem eventSystem;

    private void Awake()
    {
        eventSystem = EventSystem.current;
        controlManager = FindFirstObjectByType<ControlManager>();
        itemSprite.gameObject.SetActive(true);
        itemCount.gameObject.SetActive(true);
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    public void UpdateMouseSlot(InventorySlot invSlot)
    {
        assignedInventorySlot.AssignItem(invSlot);
        itemSprite.sprite = invSlot.ItemData.icon;
        itemCount.text = invSlot.StackSize.ToString();
        itemSprite.color = Color.white;
    }

    private void Update()
    {
        //TODO: Add controller support

        if (assignedInventorySlot.ItemData != null) //If has an item, follow the mouse position
        {
            if(ControlManager.isGamepad == false)
            {
                transform.position = Input.mousePosition;
            }
            else
            {
                transform.position = new Vector3(eventSystem.currentSelectedGameObject.transform.position.x - 20, eventSystem.currentSelectedGameObject.transform.position.y + 50, eventSystem.currentSelectedGameObject.transform.position.z);
            }
            

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                ClearSlot();
                // TODO: Drop the item on the ground
            }

        }
    }

    public void ClearSlot()
    {
        assignedInventorySlot.ClearSlot();
        itemCount.text = "";
        itemSprite.color = Color.clear;
        itemSprite.sprite = null;
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.mousePosition;
        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, result);
        return result.Count > 0;

    }

    public bool IsHoldingItem()
    {
        if(assignedInventorySlot.ItemData) return true;
        else return false;
    }



}
