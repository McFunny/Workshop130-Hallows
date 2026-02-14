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
    private HashSet<string> unlockedIDs = new HashSet<string>();

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
        TimeManager.OnHourlyUpdate += HandleHourlyUpdate;
    }

    private void OnDisable()
    {
        SaveLoad.OnSaveGame -= SaveData;
        SaveLoad.OnLoadGame -= LoadData;
        TimeManager.OnHourlyUpdate -= HandleHourlyUpdate;
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
    public bool IsUnlocked(string id) => unlockedIDs.Contains(id);

    public bool IsAchievementHidden(string id)
    {
        if (achievementById.TryGetValue(id, out var ach))
            return ach.hideAchievement;
        return false;
    }

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
        if (unlockedIDs.Contains(id)) return;

        float max = GetMaxProgress(id);
        float current = GetProgress(id);

        float next = Mathf.Clamp(current + amount, 0f, max);
        progressById[id] = next;

        // Check for unlock
        if (next >= max)
        {
            UnlockAchievement(id);
        }
    }

    public void ResetProgress(string id)
    {
        if (!achievementById.ContainsKey(id)) return;
        progressById[id] = 0f;
    }

    //Function to force unlock an achievement via id
    public void UnlockAchievement(string id)
    {
        if (!achievementById.ContainsKey(id)) return;
        if (unlockedIDs.Contains(id)) return;

        progressById[id] = GetMaxProgress(id);
        unlockedIDs.Add(id);

        if(achievementById[id].hideAchievement)
            achievementById[id].hideAchievement = false;
        

        achievementById[id].isUnlocked = true;
    
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

    // Notify that a bug has been caught
    public void NotifyBugCatch(BugObject bug)
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnBugCatch(bug);
        }
    }

    // Notify that a pyrefly has killed something else
    public void NotifyCheckProgress()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.CheckProgress();
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

    //First slot is the creature that got killed, second slot is the creature that killed it
    public void NotitfyCreatureKilledByCreature(CreatureObject creatureKilled, CreatureObject killer)
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnCreatureKillByOtherCreature(creatureKilled, killer);
            Debug.Log($"Progress on {ach.displayName}: {progressById[ach.id]}/{ach.maxProgress}");
        }
    }

    public void NotifyCreatureKilledWithHoe()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnCreatureKilledByHoe();
            Debug.Log($"Progress on {ach.displayName}: {progressById[ach.id]}/{ach.maxProgress}");
        }
    }

    public void Notify150KukriKill()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.On150KukriKill();
        }
    }

    public void NotifySleepWithTorchLit()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.SleepWithLitTorch();
        }
    }

    public void NotifyWaspsStuck()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnWaspsStuck();
        }
    }

    public void NotifyKickedBucket()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnKickedBucket();
        }
    }

    public void NotifyCropPollinated()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnCropPollinated();
        }
    }

    public void NotifyFrozenProjectileKill()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnFrozenProjectileKill();
        }
    }

    public void NotifyHighCrowKill()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnHighCrowKill();
        }
    }

    public void NotifyAllFriendsAch()
    {

        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnAllFriendsAch();
        }
    }

    public void NotifyPachinkoJackpot()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnPachinkoJackpot();
        }
    }

    public void NotifyHareDeadWhileEating()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnHareAlmostDoneEatingDeath();
        }
    }

    public void NotifyDareConsumed()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnDareConsumed();
        }
    }

    public void NotifySiegeCompleted(int siegeIndex)
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnSiegeComplete(siegeIndex);
        }
    }

    public void NotifyFinaleCompleted()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnFinaleComplete();
        }
    }

    public void NotifyPeanutFarmer()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnGrowHellaNuts();
        }
    }

    #endregion


    #region HelperFunctionsForAchievements

    /// <summary>
    /// Tracks the number of wasps that have gotten stuck. If 3 or more get stuck, notifies relevant achievements.
    /// </summary>
    int stuckWaspCount = 0;
    public void TrackStuckWasps(bool isWaspStuck)
    {
        if(isWaspStuck == true) stuckWaspCount += 1;
        else if(isWaspStuck == false) stuckWaspCount = Mathf.Max(0, stuckWaspCount - 1);

        if (stuckWaspCount >= 3)
        {
            NotifyWaspsStuck();
        }

    }

    //MannikkinID is 20
    CreatureObject mannikkinOBJ;
    public void TrackWhosFollowingPlayer()
    {
        mannikkinOBJ = CreatureDatabase.Instance.GetCreature(20);
        if (GameSaveData.Instance.currentPet != null && 
            NightSpawningManager.Instance.ReportTotalOfCreature(mannikkinOBJ) > 0 
            )
        {
            NotifyAllFriendsAch();
        }
    }

    public void HandleHourlyUpdate()
    {
        foreach (var ach in allAchievements)
        {
            if (ach == null) continue;
            if (IsUnlocked(ach.id)) continue;
            ach.OnHourlyUpdate(TimeManager.Instance.currentHour);
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
                isUnlocked = unlockedIDs.Contains(id),
                hideAchievement = achievementById[id].hideAchievement
            });
        }
    }

    private void LoadData(SaveData data)
    {
        progressById.Clear();
        unlockedIDs.Clear();

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

            if (entry.hideAchievement)
                achievementById[entry.id].hideAchievement = true;

            if (entry.isUnlocked)
            {
                unlockedIDs.Add(entry.id);
                achievementById[entry.id].hideAchievement = false;
            }

            
        }

        EnsureKeyExists();
    }

    internal void ClearAchievementsListForSurvivalMode()
    {
        allAchievements.Clear();
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
    public bool isUnlocked;
    public bool hideAchievement;
}
