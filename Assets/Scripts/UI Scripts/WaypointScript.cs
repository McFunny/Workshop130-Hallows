using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaypointScript : MonoBehaviour
{
    public GameObject shopImgObj;
    Image shopImg;
    public Transform shopTarget;

    // Update is called once per frame
    void Start()
    {
        shopImgObj = GameObject.FindGameObjectWithTag("ShopUI");
        shopImg = shopImgObj.GetComponent<Image>();
        shopImgObj.SetActive(false);
    }
    void Update()
    {
        if(shopTarget != null)
        {
            float minX = shopImg.GetPixelAdjustedRect().width / 2;
            float maxX = Screen.width - minX;

            float minY = shopImg.GetPixelAdjustedRect().height / 2;
            float maxY = Screen.height - minY;

            Vector2 shopPos = Camera.main.WorldToScreenPoint(shopTarget.position); //Shop Waypoint

            if(Vector3.Dot(shopTarget.position - transform.position, transform.forward) < 0)
            {
                //ShopTarget is behind player
                if(shopPos.x < Screen.width / 2)
                {
                    shopPos.x = maxX;
                }
                else
                {
                    shopPos.x = minX;
                }
            }

            shopPos.x = Mathf.Clamp(shopPos.x, minX, maxX);
            shopPos.y = Mathf.Clamp(shopPos.y, minY, maxY);

            shopImg.transform.position = shopPos;
            //shopImgObj.transform.position = Camera.main.WorldToScreenPoint(shopTarget.position); //Shop Waypoint
        }
        
    }
}
