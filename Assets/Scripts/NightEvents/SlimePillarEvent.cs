using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Night Event", menuName = "Night Events/Slime Pillars")]
public class SlimePillarEvent : NightEventObject
{
    public StructureObject pillar;
    public override void InitiateEvent()
    {
        PopupHandler.Instance.AddToQueue(eventStartPopup);
        PopupHandler.Instance.StartCoroutine(PerformEvent());
    }

    public override bool CanStartEvent()
    {
        if(StructureManager.Instance.TallyStructure(pillar) > 3) return false;

        if(PlayerInteraction.Instance.totalMoneyEarned >= wealthPrerequisite) return true;
        else return false;
    }

    IEnumerator PerformEvent()
    {
        List<Vector3> openTiles = new List<Vector3>();
        int pillarsToSpawn = Random.Range(1, 2);
        for(int i = 0; i < pillarsToSpawn; i++)
        {
            openTiles.Add(StructureManager.Instance.GetRandomClearTile());
        }

        yield return new WaitForSeconds(5);

        for(int i = 0; i < openTiles.Count; i++)
        {
            GameObject newPillar = StructureManager.Instance.SpawnStructureWithInstance(pillar.objectPrefab, openTiles[i]);
            newPillar.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
            ParticlePoolManager.Instance.MoveAndPlayParticle(openTiles[i], ParticlePoolManager.Instance.dirtParticle);
            yield return new WaitForSeconds(Random.Range(1,5));
        }
    }
}
