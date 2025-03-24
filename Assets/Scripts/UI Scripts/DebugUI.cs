using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    public bool isDebug;
    public static bool isDebugMenuOpen;
    public Database database;
    public GameObject debugButton, content, panel;
    private int databaseLength;
    [SerializeField] private List<InventoryItemData> items;
    // Start is called before the first frame update
    void Start()
    {
        if(!isDebug) return;
        panel.SetActive(true);
        isDebugMenuOpen = false;
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

        panel.SetActive(false);
    }

    void Update()
    {
        return;
        if(!isDebug) return;

        isDebugMenuOpen = panel.activeSelf;

        if(Input.GetKeyDown(KeyCode.Return) && !PauseScript.isPaused)
        {
            panel.SetActive(!panel.activeInHierarchy);

            if(panel.activeSelf) PlayerMovement.restrictMovementTokens++;
            else PlayerMovement.restrictMovementTokens--;
        }

        if(Input.GetKeyDown(KeyCode.Escape) && !PauseScript.isPaused && isDebugMenuOpen)
        {
            PlayerMovement.restrictMovementTokens--;
            panel.SetActive(false);
        }
    }
}
