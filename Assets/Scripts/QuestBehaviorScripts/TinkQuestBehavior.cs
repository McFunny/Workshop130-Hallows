using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Behavior", menuName = "QuestBehavior/Tinkerer Quest")]
public class TinkQuestBehavior : QuestBehavior
{
    public StructureObject tinkMachine;

    public override void StructureDestroyedEvent(StructureObject structData, Quest q)
    {
        if(structData == tinkMachine)
        {
            //Display quest fail popup?
            QuestManager.Instance.ForceRemoveQuest(q);
        }
    }

    public override void QuestAssigned(Quest q)
    {
        int x = 0;
        bool spawned = false;
        while(x < 50 && !spawned)
        {
            Vector3 spawnPos = StructureManager.Instance.GetRandomClearTile();
            spawned = StructureManager.Instance.SpawnLargeStructure(tinkMachine.objectPrefab, spawnPos, false);
        }

        if(!spawned) StructureManager.Instance.StartCoroutine(DelayRemove(q));
    }

    public IEnumerator DelayRemove(Quest q)
    {
        yield return new WaitForSeconds(3);
        QuestManager.Instance.ForceRemoveQuest(q);
    }

    public override void HourUpdate(Quest q)
    {
        if(TimeManager.Instance.currentHour == 6) //Make sure on game over this quest is failed
        {
            QuestManager.Instance.AddQuestProgress(1, q);
        }
    }
}
