using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightEventObject : ScriptableObject
{
    public string name;

    public float occurenceChance = .1f; //out of 100, every hour

    public int difficultyPointsCost = 10; //How many difficulty points will be removed when this occurs

    public PopupScript eventStartPopup;
    
    public virtual void InitiateEvent()
    {
        //What happens when the event begins
    }
}
