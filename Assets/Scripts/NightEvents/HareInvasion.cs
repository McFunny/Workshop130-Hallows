using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Night Event", menuName = "Night Events/Hare Invasion")]
public class HareInvasion : NightEventObject
{
    public CreatureObject hare;
    public StructureObject burrow;

    public override void InitiateEvent()
    {
        PopupHandler.Instance.AddToQueue(eventStartPopup);
        PopupHandler.Instance.StartCoroutine(PerformEvent());
    }

    IEnumerator PerformEvent()
    {
        List<Vector3> openTiles = new List<Vector3>();
        int burrowsToSpawn = Random.Range(7, 12);
        int haresToSpawn = Random.Range(3, 7);
        for(int i = 0; i < burrowsToSpawn; i++)
        {
            openTiles.Add(StructureManager.Instance.GetRandomClearTile());
        }

        yield return new WaitForSeconds(7);

        for(int i = 0; i < openTiles.Count; i++)
        {
            StructureManager.Instance.SpawnStructure(burrow.objectPrefab, openTiles[i]);
            ParticlePoolManager.Instance.MoveAndPlayParticle(openTiles[i], ParticlePoolManager.Instance.dirtParticle);
            if(haresToSpawn > 0)
            {
                haresToSpawn--;
                Instantiate(hare.objectPrefab, openTiles[i], Quaternion.identity);
            }
        }
    }
}
