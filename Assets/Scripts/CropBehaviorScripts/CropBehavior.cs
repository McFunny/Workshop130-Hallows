using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropBehavior : ScriptableObject
{
    public virtual void OnHour(FarmLand tile){}
    public virtual bool DestroyOnHarvest()
    {
        return true;
    }
}
