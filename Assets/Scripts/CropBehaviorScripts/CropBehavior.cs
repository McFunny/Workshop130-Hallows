using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropBehavior : ScriptableObject
{
    public virtual bool ConsumeNutrientsWhileGrown()
    {
        return false;
    }
    public virtual void OnHour(FarmLand tile){}
    public virtual bool DestroyOnHarvest()
    {
        return true;
    }
    public virtual void OnIchorRefill(FarmLand tile){}

    public virtual void CropBonusYield(FarmLand tile, out int cropBonus, out int secondaryCropBonus)
    {
        cropBonus = 0;
        secondaryCropBonus = 0;
    }

    public virtual void OnCropDestroyed(FarmLand tile){}

    public virtual void OnWatered(FarmLand tile){}

    public virtual void OnPlanted(FarmLand tile){}

    public virtual void OnConsumed(CreatureBehaviorScript creature){}
}
