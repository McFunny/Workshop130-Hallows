using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    //public bool isDebug;
    public static bool isDebugMenuOpen;
    public Database database;
    public GameObject debugButton, content, panel;
    public Codex3 newCodex;
    private GameObject codexActual;
    private int databaseLength;
    [SerializeField] private List<InventoryItemData> items;
    private bool itemsLoaded = false;
    // Start is called before the first frame update
    void Start()
    {
        if (!StructureManager.Instance.enableCheats) return;
        panel.SetActive(true);
        isDebugMenuOpen = false;
        items = database.GetItemDatabase();

        LoadItemDatabase();

        itemsLoaded = true;
        panel.SetActive(false);
        print("File Mode: " + MainMenuScript.currentFileMode);
        print("Save File: " + MainMenuScript.currentSaveSlot);

        codexActual = newCodex.transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if(!StructureManager.Instance.enableCheats) return;

        isDebugMenuOpen = panel.activeSelf;

        if (Input.GetKeyDown(KeyCode.F1) && !PauseScript.isPaused)
        {
            if (!PlayerMovement.isCodexOpen)
            {
                newCodex.OpenCodex();
            }
            else
            {
                newCodex.CloseCodex();
            }    
        }

        if (Input.GetKeyDown(KeyCode.Return) && !PauseScript.isPaused)
            {
                LoadItemDatabase();
                panel.SetActive(!panel.activeInHierarchy);

                if (panel.activeSelf) PlayerMovement.restrictMovementTokens++;
                else PlayerMovement.restrictMovementTokens--;
            }

        if(Input.GetKeyDown(KeyCode.Escape) && !PauseScript.isPaused && isDebugMenuOpen)
        {
            PlayerMovement.restrictMovementTokens--;
            panel.SetActive(false);
        }
    }

    private void LoadItemDatabase()
    {
        if(itemsLoaded) return;
        items = database.GetItemDatabase();

        for(int i = 0; i < items.Count; i++)
        {
            var tempButton = Instantiate(debugButton, content.transform, worldPositionStays:false);
            tempButton.name = "DebugButton" + i;

            var data = items[i];
            var imgCmp = tempButton.GetComponent<Image>();
            var textCmp = tempButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var dataCmp = tempButton.GetComponent<DebugButtonID>();

            //print(data.name);
            if (data.icon != null) imgCmp.sprite = data.icon;
            textCmp.text = data.name;
            dataCmp.data = data;
        }

        itemsLoaded = true;
    }
}
