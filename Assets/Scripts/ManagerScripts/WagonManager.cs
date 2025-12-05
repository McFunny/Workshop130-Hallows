using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonManager : MonoBehaviour
{
    public static WagonManager Instance;

    [HideInInspector] public PlayerWagonScript farmWagon, wildernessWagon;

    public float wagonHealth = 500;
    public float maxWagonHealth = 500;
    public bool wagonDestroyed = false;

    public int daysToRepair = -1;
    public delegate void OnWagonHPChanged();
    public event OnWagonHPChanged onWagonHPChanged;

    public bool debugWagon; // if true, will force the player wagon to stay spawned in even if not unlocked

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }

    void Start()
    {
        WildernessManager.OnWildernessLeave += LeaveWilderness;
        TimeManager.OnHourlyUpdate += HourUpdate;
        StartCoroutine(DelayedStart());
    }

    void OnDestroy()
    {
        WildernessManager.OnWildernessLeave -= LeaveWilderness;
        TimeManager.OnHourlyUpdate -= HourUpdate;
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(3);
        if(!GameSaveData.Instance.playerWagonUnlocked && !debugWagon) farmWagon.gameObject.SetActive(false);
        if(wagonHealth <= 0) wagonDestroyed = true;
        farmWagon.UpdateModel(!wagonDestroyed);
    }

    void HourUpdate()
    {
        if(TimeManager.Instance.currentHour == 8)
        {
            if(farmWagon.gameObject.activeSelf == false && GameSaveData.Instance.playerWagonUnlocked)
            {
                farmWagon.gameObject.SetActive(true);
                QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.GetMainQuest(15), out bool removedSuccesfully);
                if(removedSuccesfully) QuestManager.Instance.AddQuest(QuestDatabase.Instance.GetMainQuest(16));
            }

            if(daysToRepair > 0)
            {
                daysToRepair--;
                if(daysToRepair == 0)
                {
                    //daysToRepair = 0;
                    wagonDestroyed = false;
                    wagonHealth = maxWagonHealth;
                    farmWagon.UpdateModel(true);
                }
            }
        }
    }

    public void WagonHealthChange(float amount)
    {
        if(amount <= 0 && wagonHealth <= 0) return;

        wagonHealth += amount;

        if(wagonHealth < 0) wagonHealth = 0;

        if (wagonHealth > maxWagonHealth) wagonHealth = maxWagonHealth;

        onWagonHPChanged.Invoke();

        print("Wagon health changed. Health is " + wagonHealth);

        if(!wagonDestroyed && wagonHealth == 0 && TownGate.Instance.location == PlayerLocation.InWilderness)
        {
            WildernessManager.Instance.ExitWilderness();
            wagonDestroyed = true;
            daysToRepair = 2;
        }
    }

    void LeaveWilderness()
    {
        if(!wagonDestroyed) wagonHealth = maxWagonHealth;
    }
}
