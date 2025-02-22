using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestQuestGiver : MonoBehaviour
{
    Quest[] possibleQuests;
    public FetchQuest[] possibleFetchQuests;
    public HuntQuest[] possibleHuntQuests;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            GiveQuest();
        }
    }

    void GiveQuest()
    {
        if(QuestManager.Instance.activeQuests.Count == 0)
        {
            QuestManager.Instance.activeQuests.Add(possibleQuests[Random.Range(0, possibleQuests.Length)]);
        }
    }
}
