using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBehavior : ScriptableObject
{
    public string name;

    public int id = -1;
    
    public virtual void HourUpdate()
    {
        //What happens when the event begins
    }
}
