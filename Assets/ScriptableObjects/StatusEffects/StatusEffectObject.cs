using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectObject : ScriptableObject
{
    //figure out how to handle things like the vfx and particle prefabs to appear and dissapear on things. Maybe tie it to a list in the status manager
    
    public virtual void OneSecondEffect()
    {
        //What happens every second for player
    }

    public virtual void OneSecondEffect(CreatureBehaviorScript c)
    {
        //What happens every second for creature
    }
}
