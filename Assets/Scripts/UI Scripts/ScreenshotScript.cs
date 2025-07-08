using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
using System.Collections;

public class ScreenshotScript : MonoBehaviour
{
    [SerializeField] KeyCode screenshotKey = KeyCode.RightArrow;
    [SerializeField] private string path;
    [Range(1, 5)] [SerializeField] private int size = 1;
    private string fileName;
    private StructureManager structureManager;
    private Camera mainCamera;
    [SerializeField] private PopupScript screenshotPopup;

    private void Start()
    {
        structureManager = FindObjectOfType<StructureManager>();
        //Debug.Log(DateTime.Now);
        mainCamera = Camera.main;

        if (!Directory.Exists(Application.persistentDataPath + path))
        {
            Directory.CreateDirectory(Application.persistentDataPath + path);
            Debug.Log("Created screenshot directory: " + Application.persistentDataPath + path);
        }
    }

    void Update()
    {
        //if (!structureManager.enableCheats) return;
        
        if (Input.GetKeyDown(screenshotKey))
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            fileName = "VeilWood_Screenshot_" + date;
            ScreenCapture.CaptureScreenshot(Application.persistentDataPath + path + fileName + ".png", size);
            Debug.Log("Screenshot saved to: " + Application.persistentDataPath + path + fileName);
            if (screenshotPopup != null)
            {
                PopupHandler.Instance.AddToQueue(screenshotPopup);
            }
        }
    }
}

