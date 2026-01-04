using SaveLoadSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Achievements")]
    [SerializeField] private List<AchievementObject> allAchievements = new List<AchievementObject>();

    private Dictionary<string, AchievementObject> achievementById = new Dictionary<string, AchievementObject>();

    private Dictionary<string, float> progressById = new Dictionary<string, float>();
    private HashSet<string> unlockedIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ProcessAchievements();
        EnsureKeyExists();

        SaveLoad.OnSaveGame += SaveData;
        SaveLoad.OnLoadGame += LoadData;
    }

    private void OnDisable()
    {
        SaveLoad.OnSaveGame -= SaveData;
        SaveLoad.OnLoadGame -= LoadData;
    }

    /// <summary>
    /// Ensures that the achievementById dictionary is built for quick lookup.
    /// </summary>
    private void ProcessAchievements()
    {
        achievementById.Clear();

        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;

            if (string.IsNullOrWhiteSpace(ach.id))
            {
                Debug.LogError($"Achievement asset '{ach.name}' missing id.", ach);
                continue;
            }

            if (achievementById.ContainsKey(ach.id))
            {
                Debug.LogError($"Duplicate Achievement id '{ach.id}' found. IDs must be unique.", ach);
                continue;
            }

            achievementById.Add(ach.id, ach);
        }
    }

    /// <summary>
    /// Makes sure that all achievements have entries in the progress dictionary.
    /// </summary>
    private void EnsureKeyExists()
    {
        foreach (var id in achievementById.Keys)
        {
            if (!progressById.ContainsKey(id))
                progressById[id] = 0f;
        }
    }

    //Function to get all achievements with their progress
    public Dictionary<AchievementObject, float> GetAllAchievementsWithProgress()
    {
        var result = new Dictionary<AchievementObject, float>();
        foreach (var kvp in achievementById)
        {
            var ach = kvp.Value;
            var progress = GetProgress(kvp.Key);
            result[ach] = progress;
        }
        return result;
    }


    //Function to check if an achievement is unlocked
    public bool IsUnlocked(string id) => unlockedIds.Contains(id);

    //Function to get current progress of an achievement
    public float GetProgress(string id)
    {
        return progressById.TryGetValue(id, out var progress) ? progress : 0f;
    }

    //Function to get max progress of an achievement
    public float GetMaxProgress(string id)
    {
        if (achievementById.TryGetValue(id, out var ach))
            return Mathf.Max(1f, ach.maxProgress);

        return 1f;
    }

    //Function to add progress to an achievement via id and amount
    public void AddProgress(string id, float amount)
    {
        if (amount <= 0f) return;
        if (!achievementById.ContainsKey(id)) return;
        if (unlockedIds.Contains(id)) return;

        float max = GetMaxProgress(id);
        float current = GetProgress(id);

        float next = Mathf.Clamp(current + amount, 0f, max);
        progressById[id] = next;

        // Check for unlock
        if (next >= max)
        {
            ForceUnlock(id);
        }
    }

    //Function to force unlock an achievement via id
    public void ForceUnlock(string id)
    {
        if (!achievementById.ContainsKey(id)) return;
        if (unlockedIds.Contains(id)) return;

        progressById[id] = GetMaxProgress(id);
        unlockedIds.Add(id);

        //For UI events
        //OnAchievementUnlocked?.Invoke(achievementById[id]);
    }

    #region NotificationFunctions
    /// <summary>
    /// NOTIFY FUNCTIONS - Call these from relevant game systems to update achievements
    /// </summary>

    // Notify that a creature has been killed
    public void NotifyCreatureKill(CreatureObject creature)
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnCreatureKill(creature);
            Debug.Log($"Progress on {ach.displayName}: {progressById[ach.id]}/{ach.maxProgress}");
        }
    }

    // Notify that a crop has been harvested
    public void NotifyCropHarvest(CropData crop)
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnCropHarvest(crop);
        }
    }

    // Notify that a pyrefly has killed something else
    public void NotifyJustAddProgress()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.JustAddProgress();
        }
    }

    // Notify that an item has been collected
    public void NotifyItemCollected(InventoryItemData itemData)
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnItemCollected(itemData);
        }
    }

#endregion

    /// <summary>
    /// SAVE/LOAD FUNCTIONS
    /// </summary>

    private void SaveData()
    {
        if (SaveLoad.CurrentSaveData.achievementSaveData == null)
            SaveLoad.CurrentSaveData.achievementSaveData = new AchievementSaveData();

        var save = SaveLoad.CurrentSaveData.achievementSaveData;
        save.entries.Clear();

        foreach (var kvp in achievementById)
        {
            string id = kvp.Key;

            save.entries.Add(new AchievementEntry
            {
                id = id,
                progress = GetProgress(id),
                unlocked = unlockedIds.Contains(id)
            });
        }
    }

    private void LoadData(SaveData data)
    {
        progressById.Clear();
        unlockedIds.Clear();

        if (data == null || data.achievementSaveData == null || data.achievementSaveData.entries == null)
        {
            EnsureKeyExists();
            return;
        }

        foreach (var entry in data.achievementSaveData.entries)
        {
            if (entry == null) continue;
            if (string.IsNullOrWhiteSpace(entry.id)) continue;
            if (!achievementById.ContainsKey(entry.id)) continue;

            float max = GetMaxProgress(entry.id);
            progressById[entry.id] = Mathf.Clamp(entry.progress, 0f, max);

            if (entry.unlocked)
                unlockedIds.Add(entry.id);
        }

        EnsureKeyExists();
    }
}

[Serializable]
public class AchievementSaveData
{
    public List<AchievementEntry> entries = new List<AchievementEntry>();
}

[Serializable]
public class AchievementEntry
{
    public string id;
    public float progress;
    public bool unlocked;
}
