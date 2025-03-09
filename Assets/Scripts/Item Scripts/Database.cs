using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Item Database")]
public class Database : ScriptableObject
{
    private static Database _instance;

    public static Database Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<Database>("Database");

                if (_instance == null)
                {
                    Debug.LogError("Database could not be loaded! Make sure ItemDatabase exists in Resources.");
                }
            }
            return _instance;
        }
    }


    [SerializeField] private List<InventoryItemData> _itemDatabase;

    [ContextMenu("Set IDs")]
    public void SetItemIDs()
    {
        _itemDatabase = new List<InventoryItemData>();

        var foundItems = Resources.LoadAll<InventoryItemData>("").OrderBy(i => i.ID).ToList();

        var hasIDInRange = foundItems.Where(i => i.ID != -1 && i.ID < foundItems.Count).OrderBy(i => i.ID).ToList();
        var hasIDNotInRange = foundItems.Where(i => i.ID != -1 && i.ID >= foundItems.Count).OrderBy(i => i.ID).ToList();
        var noID = foundItems.Where(i => i.ID <= -1).ToList();

        var index = 0;
        for (int i = 0; i < foundItems.Count; i++)
        {
            InventoryItemData itemToAdd;
            itemToAdd = hasIDInRange.Find(d => d.ID == i);

            if (itemToAdd != null)
            {
                _itemDatabase.Add(itemToAdd);
            }
            else if (index < noID.Count)
            {
                noID[index].ID = i;
                itemToAdd = noID[index];
                index++;
                _itemDatabase.Add(itemToAdd);
            }
        }

        foreach (var item in hasIDNotInRange)
        {
            _itemDatabase.Add(item);
        }
    }

    public InventoryItemData GetItem(int id)
    {
        return _itemDatabase.Find(i => i.ID == id);
    }

    public List<CropItem> GetAllCrops()
    {
        return Resources.LoadAll<CropItem>("").ToList();
    }

    public List<InventoryItemData> GetItemDatabase()
    {
        return _itemDatabase;
    }

}
