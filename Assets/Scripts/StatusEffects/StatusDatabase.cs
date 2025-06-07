using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusDatabase", menuName = "Databases/StatusDatabase")]
public class StatusDatabase : ScriptableObject
{
    private static StatusDatabase _instance;

    public static StatusDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance of the Database if not already set
                _instance = Resources.Load<StatusDatabase>("StatusDatabase");
            }
            return _instance;
        }
    }

    public List<StatusEffectObject> Effects;


    public StatusEffectObject GetStatus(StatusEffectName _name) //USE THIS FOR GRABBING STRUCTURES WITH THE DEBRIS PILE
    {
        return Effects.Find(i => i.name == _name);
    }

}
