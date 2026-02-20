using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance;
    uint appID = 3772610;
    private int totalNumberOfAchievements;
    bool connectedToSteam = false;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        try
        {
            Steamworks.SteamClient.Init(appID);
            connectedToSteam = true;
            Debug.LogError("Connected to Steam");
        }
        catch(System.Exception exception)
        {
            Debug.LogError("Not connected to Steam");
            connectedToSteam = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(connectedToSteam)
        {
            Steamworks.SteamClient.RunCallbacks();
        }
    }

    public void DisconnectFromSteam()
    {
        if(connectedToSteam) Steamworks.SteamClient.Shutdown();
    }

    public void UnlockAchievement(string _AchievementToUnlock)
    {
        if(connectedToSteam)
        {
            var ach = new Steamworks.Data.Achievement(_AchievementToUnlock);
            ach.Trigger();
        }
    }

    public void ResetAllAchievements(List<AchievementObject> allAchievements) 
    {
        if(!connectedToSteam) return;
        for(int i = 0; i < allAchievements.Count; ++i)
        {
            var ach = new Steamworks.Data.Achievement(allAchievements[i].id);
            ach.Clear();
        }
    }
}
