using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Status Effects/Default")]
public class StatusEffectObject : ScriptableObject
{
    //figure out how to handle things like the vfx and particle prefabs to appear and dissapear on things. Maybe tie it to a pool in the status manager
    public StatusEffectName name;
    
    public virtual void TimedEffect()
    {
        //What happens every second for player
    }

    public virtual void TimedEffect(CreatureBehaviorScript c)
    {
        //What happens every second for creature
    }
}
