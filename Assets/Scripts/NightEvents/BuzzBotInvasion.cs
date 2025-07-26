using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Night Event", menuName = "Night Events/BuzzBot Invasion")]
public class BuzzBotInvasion : NightEventObject
{
    public CreatureObject buzzBot;

    public override void InitiateEvent()
    {
        PopupHandler.Instance.AddToQueue(eventStartPopup);
        PopupHandler.Instance.StartCoroutine(PerformEvent());
    }

    IEnumerator PerformEvent()
    {
        int botsToSpawn = Random.Range(3, 7);

        yield return new WaitForSeconds(3);

        for(int i = 0; i < botsToSpawn; i++)
        {
            NightSpawningManager.Instance.SpawnCreature(buzzBot);
            yield return new WaitForSeconds(Random.Range(1, 4));
        }
    }
}