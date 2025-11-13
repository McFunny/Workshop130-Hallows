using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Night Event", menuName = "Night Events/Corruption Invasion")]
public class CorruptionInvasion : NightEventObject
{
    public List<EnemyInvasionUnit> units = new List<EnemyInvasionUnit>();

    public override void InitiateEvent()
    {
        PopupHandler.Instance.AddToQueue(eventStartPopup);
        PopupHandler.Instance.StartCoroutine(PerformEvent());
    }

    public override bool CanStartEvent()
    {
        if(CorruptionManager.Instance.corruptedTiles > 15) return false;

        if(GameSaveData.Instance.siegesCleared >= 2) return true;
        else return false;
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
