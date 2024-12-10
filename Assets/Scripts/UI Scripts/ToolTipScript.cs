using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class ToolTipScript : MonoBehaviour
{
        public GameObject toolTip, panel;
        TextMeshProUGUI textBox;
        protected Vector3[] corners;
        Vector3 pos;
        EventSystem eventSystem;

        public void Awake()
        {
            corners = new Vector3[4];
            textBox = toolTip.GetComponent<TextMeshProUGUI>();
            eventSystem = EventSystem.current;
        }

        protected void LateUpdate()
        {
            if(!ControlManager.isGamepad)
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

            transform.position = pos;
        }
        public void UpdateToolTip(string updatedText)
        {
            //print(updatedText);
            if(updatedText == null || !panel.activeSelf) return;
            textBox.text = updatedText;
            
        }

        public void ToolTipPosition()
        {

        }
    
}
