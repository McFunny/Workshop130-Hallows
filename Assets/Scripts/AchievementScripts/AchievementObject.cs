using UnityEngine;

public abstract class AchievementObject : ScriptableObject
{
    [Header("Info")]
    public string id;                 //USE THIS FOR STEAM ID IF YOU WANT TO INTEGRATE WITH STEAM ACHIEVEMENTS
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    public bool hideAchievement = false; // if true, show ??? until unlocked
    public bool isUnlocked;

    [Header("Progress")]
    [Min(1)] public float maxProgress = 1f;

    // Called by the manager so the achievement can ask for progress/unlock.
    protected void AddProgress(float amount)
    {
        AchievementManager.Instance.AddProgress(id, amount);
    }

    protected void ResetProgress()
    {
        AchievementManager.Instance.ResetProgress(id);
    }

    protected void Unlock()
    {
        AchievementManager.Instance.UnlockAchievement(id);
    }

    /////These are all of the vitual functions that could contribuite to increasing the progress to the achievements. Achievements will typically use only 1 or 2 of these functions/////

    public virtual void OnCreatureKill(CreatureObject killedCreature) { }

    public virtual void OnCropHarvest(CropData harvestedCrop) { }

    public virtual void OnBugCatch(BugObject caughtBug) { }

    public virtual void OnPyreflyTeamKill() { } //For calling if u kill something by using a pyrefly explosion

    public virtual void OnItemCollected(InventoryItemData itemData) { } //This will be used for single item obtainments like the water gun or tool upgrades!

    public virtual void CheckProgress(float number = 1f) { } //For achievements that just need to have progress added without any specific notification

    public virtual void OnCreatureKillByOtherCreature(CreatureObject creatureKilled, CreatureObject killer) { } //For calling if u kill something by using a pyrefly explosion caused by hog

    public virtual void OnCreatureKilledByHoe() { }

    public virtual void On150KukriKill() { }

    public virtual void SleepWithLitTorch() { }

    public virtual void OnWaspsStuck() { }

    public virtual void OnKickedBucket() { }

    public virtual void OnCropPollinated() { }

    public virtual void OnFrozenProjectileKill() { }

    public virtual void OnHighCrowKill() { }

    public virtual void OnAllFriendsAch() { }

    public virtual void OnPachinkoJackpot() { }

    public virtual void OnHareAlmostDoneEatingDeath() { }

    public virtual void OnSiegeComplete(int siegeIndex) { }

    public virtual void OnFinaleComplete() { }

    public virtual void OnHourlyUpdate(int hour) { }

    public virtual void OnDareConsumed() { }

    public virtual void OnGrowHellaNuts() { }
}