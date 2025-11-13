using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Night Event", menuName = "Night Events/Enemy Invasion")]
public class EnemyInvasion : NightEventObject
{
    public List<EnemyInvasionUnit> units = new List<EnemyInvasionUnit>();

    public override void InitiateEvent()
    {
        PopupHandler.Instance.AddToQueue(eventStartPopup);
        PopupHandler.Instance.StartCoroutine(PerformEvent());
    }

    IEnumerator PerformEvent()
    {
        int r = 0;
        foreach(EnemyInvasionUnit unit in units)
        {
            r = Random.Range(unit.spawnMin, unit.spawnMax + 1);
            for(int i = 0; i < r; i++)
            {
                NightSpawningManager.Instance.SpawnCreature(unit.prefab);
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitForSeconds(2);
        }
    }
}

[System.Serializable]
public class EnemyInvasionUnit
{
    public GameObject prefab;
    public int spawnMin, spawnMax;
}
