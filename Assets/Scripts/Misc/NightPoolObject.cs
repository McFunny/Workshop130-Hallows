using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Night Pool", menuName = "Night Pool")]
public class NightPoolObject : ScriptableObject
{
    public string name;
    public int forceSetDifficultyPoints = 0; //If 0, will calculate the points normally
    public List<CreatureObject> creatures = new List<CreatureObject>();
    public bool forceCorrupted = false;
}
