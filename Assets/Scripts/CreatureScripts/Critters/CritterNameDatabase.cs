using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Critter Name Database")]
public class CritterNameDatabase : ScriptableObject
{
    private static CritterNameDatabase _instance;

    public static CritterNameDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance of the Database if not already set
                _instance = Resources.Load<CritterNameDatabase>("CritterNameDatabase");
            }
            return _instance;
        }
    }
    [SerializeField] private List<string> _critterNames;
    [SerializeField] private List<string> _hogNames;
    [SerializeField] private List<string> _henNames;
    [SerializeField] private List<string> _mimicNames;
    [SerializeField] private List<string> _flyNames;

    public string GetCritterName(CritterType type) 
    {

        switch(type)
        {
            case(CritterType.Hog):
            return _hogNames[Random.Range(0, _hogNames.Count)];
            break;
            case(CritterType.Hen):
            return _henNames[Random.Range(0, _henNames.Count)];
            break;
            case(CritterType.Mimic):
            return _mimicNames[Random.Range(0, _mimicNames.Count)];
            break;
            case(CritterType.Fly):
            return _flyNames[Random.Range(0, _flyNames.Count)];
            break;
            default:
            return _critterNames[Random.Range(0, _critterNames.Count)];
            break;
        }
    }
}
