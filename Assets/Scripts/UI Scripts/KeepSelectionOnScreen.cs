using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeepSelectionOnScreen : MonoBehaviour //Bro I stg...
{
    [SerializeField] RectTransform scrollRectTransform;
    [SerializeField] RectTransform contentPanel;
    [SerializeField] RectTransform selectedRectTransform;
    [SerializeField] GameObject lastSelected;

    void Start() {
        scrollRectTransform = GetComponent<RectTransform>();
        contentPanel = GetComponent<ScrollRect>().content;
    }

    void Update() {
        if(!ControlManager.isController) return;
        //print("Controller Checked");
        // Get the currently selected UI element from the event system.
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        //print(selected);
        //print(selected.transform.parent);

        // Return if there are none.
        if (selected == null) {
            return;
        }

        if(selected.GetComponent<UIMenuButton>() != null)
        {
            var component = selected.GetComponent<UIMenuButton>();
            if(!component.isWithinScrollRect) return;
        }
        else{return;}
        //print("Component is UIMenuButton");
        
        // Return if the selected game object is the same as it was last frame,
        // meaning we haven't moved.
        if (selected == lastSelected) {
            //print("selected = lastSelected");
            return;
        }
        //print("selected != lastSelected");
        //print("Selected object is not the same as the last frame");

        // Get the rect tranform for the selected game object.
        if(selected.transform.parent.GetComponent<Button>() == null) selectedRectTransform = selected.transform.parent.GetComponent<RectTransform>();
        else selectedRectTransform = selected.GetComponent<RectTransform>();
        // The position of the selected UI element is the absolute anchor position,
        // ie. the local position within the scroll rect + its height if we're
        // scrolling down. If we're scrolling up it's just the absolute anchor position.
        float selectedPositionY = Mathf.Abs(selectedRectTransform.anchoredPosition.y) + selectedRectTransform.rect.height;

        // The upper bound of the scroll view is the anchor position of the content we're scrolling.
        float scrollViewMinY = contentPanel.anchoredPosition.y;
        // The lower bound is the anchor position + the height of the scroll rect.
        float scrollViewMaxY = contentPanel.anchoredPosition.y + scrollRectTransform.rect.height;

        // If the selected position is below the current lower bound of the scroll view we scroll down.
        if (selectedPositionY > scrollViewMaxY) {
            float newY = selectedPositionY - scrollRectTransform.rect.height;
            contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, newY);
        }
        // If the selected position is above the current upper bound of the scroll view we scroll up.
        else if (Mathf.Abs(selectedRectTransform.anchoredPosition.y) < scrollViewMinY) {
            contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, Mathf.Abs(selectedRectTransform.anchoredPosition.y));
        }

        lastSelected = selected;
        //print("help");
    }
}
