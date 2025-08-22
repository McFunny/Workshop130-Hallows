using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    public static bool isDebugMenuOpen;

    [Header("Data")]
    public Database database;
    public CreatureDatabase creatureDatabase;

    [Header("UI Refs")]
    public GameObject debugButton;     
    public GameObject content;        
    public GameObject panel;           
    public ScrollRect scrollRect;      
    public Codex3 newCodex;
    [SerializeField] private Button switchButton;

    public static int SpawnCount { get; private set; } = 1;

    private GameObject codexActual;

    private List<InventoryItemData> itemList = new List<InventoryItemData>();
    private List<CreatureObject> creatureList = new List<CreatureObject>();

    private enum DebugMode { Items, Creatures }
    [SerializeField] private DebugMode currentMode = DebugMode.Items;

    void Start()
    {
        if (!StructureManager.Instance.enableCheats) return;

        panel.SetActive(true);
        isDebugMenuOpen = false;

        itemList = database ? database.GetItemDatabase() : new List<InventoryItemData>();
        creatureList = creatureDatabase ? creatureDatabase.GetCreatureDatabase() : new List<CreatureObject>();

        RebuildButtons();                // build initial list
        panel.SetActive(false);

        Debug.Log("File Mode: " + MainMenuScript.currentFileMode);
        Debug.Log("Save File: " + MainMenuScript.currentSaveSlot);

        if (newCodex) codexActual = newCodex.transform.GetChild(0).gameObject;

        if (switchButton != null)
            switchButton.onClick.AddListener(ToggleMode);
    }



    void Update()
    {
        if (!StructureManager.Instance.enableCheats) return;

        isDebugMenuOpen = panel.activeSelf;

        if (Input.GetKeyDown(KeyCode.Return) && !PauseScript.isPaused)
        {
            RebuildButtons();                    // refresh list
            panel.SetActive(!panel.activeInHierarchy);

            if (panel.activeSelf) PlayerMovement.restrictMovementTokens++;
            else PlayerMovement.restrictMovementTokens--;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !PauseScript.isPaused && isDebugMenuOpen)
        {
            PlayerMovement.restrictMovementTokens--;
            panel.SetActive(false);
        }
    }

    public void ToggleMode()
    {
        currentMode = currentMode == DebugMode.Items ? DebugMode.Creatures : DebugMode.Items;
        RebuildButtons();    
    }

    public void SetSpawnCount1() => SetSpawnCount(1);
    public void SetSpawnCount5() => SetSpawnCount(5);
    public void SetSpawnCount10() => SetSpawnCount(10);
    public void SetSpawnCount50() => SetSpawnCount(50);

    private void SetSpawnCount(int value)
    {
        SpawnCount = Mathf.Max(1, value);
    }

    private void RebuildButtons()
    {
       
        ClearContentChildren();

        
        if (currentMode == DebugMode.Items)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                var row = Instantiate(debugButton, content.transform, false);
                row.name = "DebugItemButton_" + i;

                var data = itemList[i];

                var img = row.GetComponent<Image>();
                if (img) img.sprite = data ? data.icon : null;

                var txt = row.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                if (txt) txt.text = data ? data.name : "NULL ITEM";

                var id = row.GetComponent<DebugButtonID>();
                if (id)
                {
                   
                    SafeClearPayload(id);
                    id.data = data;
                }
            }
        }
        else 
        {
            for (int c = 0; c < creatureList.Count; c++)
            {
                var creature = creatureList[c];

              
                {
                    var row = Instantiate(debugButton, content.transform, false);
                    row.name = $"DebugCreatureButton_{c}_Base";

                    var txt = row.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                    string label = "NULL CREATURE";
                    if (creature != null)
                    {
                        label = string.IsNullOrEmpty(creature.name) ? "Creature" : creature.name;
                        if (creature.objectPrefab == null) label += " (No Prefab)";
                    }
                    if (txt) txt.text = label;

                    var id = row.GetComponent<DebugButtonID>();
                    if (id)
                    {
                        SafeClearPayload(id);
                        id.creature = creature;   
                        
                    }
                }

               
                if (creature != null && creature.creatureVariants != null)
                {
                    for (int v = 0; v < creature.creatureVariants.Count; v++)
                    {
                        var variant = creature.creatureVariants[v];
                        var row = Instantiate(debugButton, content.transform, false);
                        row.name = $"DebugCreatureButton_{c}_Variant_{v}";

                        var txt = row.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                        string vLabel = $"{creature.name} – {(variant != null ? variant.name : "Variant")}";
                        if (variant == null || variant.prefab == null) vLabel += " (No Prefab)";
                        if (txt) txt.text = vLabel;

                        var id = row.GetComponent<DebugButtonID>();
                        if (id)
                        {
                            SafeClearPayload(id);
                            id.creature = creature; 
                            id.variant = variant; 
                        }
                    }
                }
            }
        }

       
        if (scrollRect)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f; 
            Canvas.ForceUpdateCanvases();
        }
    }

    private void ClearContentChildren()
    {
        
        for (int i = content.transform.childCount - 1; i >= 0; i--)
        {
            var child = content.transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    private static void SafeClearPayload(DebugButtonID id)
    {
       
        id.data = null;
        id.creature = null;
       
        if (id.GetType().GetField("variant") != null)
        {
            id.variant = null;
        }
    }
}