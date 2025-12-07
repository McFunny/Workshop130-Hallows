using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropBehavior : ScriptableObject
{
    public float behaviorUpdateTime = 0; //For use only on behavior update function

    public virtual void OnCropAwake(FarmLand tile){} //Called when the crop is planted AND when the game reloads

    public virtual bool ConsumeNutrientsWhileGrown()
    {
        return false;
    }
    public virtual void OnHour(FarmLand tile){}
    public virtual void OnFullyGrown(FarmLand tile){}
    public virtual bool DestroyOnHarvest(FarmLand tile, out int stagesReduced)
    {
        stagesReduced = 0;
        return true;
    }
    public virtual void OnIchorRefill(FarmLand tile){}

    public virtual void CropBonusYield(FarmLand tile, out int cropBonus, out int secondaryCropBonus)
    {
        cropBonus = 0;
        secondaryCropBonus = 0;
    }

    public virtual void CropRemovalBonusYield(FarmLand tile, out int secondaryCropBonus) //This is called at any time the plant is removed
    {
        secondaryCropBonus = 0;
    }

    public virtual void OnCropDestroyed(FarmLand tile){} //Will be called if the crop was harvested or killed

    public virtual void OnWatered(FarmLand tile){}

    public virtual void OnPlanted(FarmLand tile){} //Happens when the crop is planted

    public virtual void OnConsumed(CreatureBehaviorScript creature){} //For when eaten at full growth

    public virtual void OnConsumedBeforeMaturity(CreatureBehaviorScript creature){} //For when eaten at all

    public virtual void OnContact(FarmLand tile, GameObject contactedObject){}

    public virtual void OnHarvest(FarmLand tile, bool usedShovel, bool usedScythe){}

    public virtual void OnPollinate(FarmLand tile){}

    public virtual void OnGrowth(FarmLand tile){}

    public virtual bool IsFlammable()
    {
        return true;
    }

    public virtual bool CanGrow(FarmLand tile)
    {
        return true;
    }

    public virtual bool CanDig(FarmLand tile)
    {
        return true;
    }

    public virtual void BehaviorUpdate(FarmLand tile){}

    public virtual void OnFrost(FarmLand tile){}

    public virtual bool OverrideWaterNeed(FarmLand tile) //Forces the ui to display it needs water
    {
        return false;
    }
}
