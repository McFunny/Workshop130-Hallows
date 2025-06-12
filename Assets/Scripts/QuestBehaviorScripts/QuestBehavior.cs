using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBehavior : ScriptableObject
{
    public string name;

    public int id = -1;
    
    public virtual void HourUpdate(Quest q)
    {
        //What happens when the event begins
    }

    public virtual void QuestAssigned(Quest q)
    {
        //What happens when the quest is given. Currently works for templates only
    }

    public virtual void StructureDestroyedEvent(StructureObject structData, Quest q)
    {
        //What happens when a structure breaks
    }
}
