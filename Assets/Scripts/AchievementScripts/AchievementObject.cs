using UnityEngine;

public abstract class AchievementObject : ScriptableObject
{
    [Header("Info")]
    public string id;                 // MUST be unique
    public string displayName;
    [TextArea] public string description;

    public bool hideAchievement = false; // if true, show ??? until unlocked

    [Header("Progress")]
    [Min(1)] public float maxProgress = 1f;

    // Called by the manager so the achievement can ask for progress/unlock.
    protected void AddProgress(float amount)
    {
        AchievementManager.Instance.AddProgress(id, amount);
    }

    protected void Unlock()
    {
        AchievementManager.Instance.ForceUnlock(id);
    }

    /////These are all of the vitual functions that could contribuite to increasing the progress to the achievements. Achievements will typically use only 1 or 2 of these functions/////

    public virtual void OnCreatureKill(CreatureObject killedCreature){}

    public virtual void OnCropHarvest(CropData harvestedCrop){}

    public virtual void OnPyreflyTeamKill(){} //For calling if u kill something by using a pyrefly explosion

    public virtual void OnItemCollected(InventoryItemData itemData, int amount) { }
}
