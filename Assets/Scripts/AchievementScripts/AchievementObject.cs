using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementObject : ScriptableObject
{
    public string name;
    public string description;

    public float progress = 0;
    public float maxProgress = 0;

    public bool hideAchievement = false; //If true, the name of this achievement should display as ??? if not completed

    /////These are all of the vitual functions that could contribuite to increasing the progress to the achievements. Achievements will typically use only 1 or 2 of these functions/////

    public virtual void OnCreatureKill(){}

    public virtual void OnCropHarvest(){}

    public virtual void OnPyreflyTeamKill(){} //For calling if u kill something by using a pyrefly explosion
}
