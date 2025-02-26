using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TestQuestGiver : MonoBehaviour
{
    List<Quest> possibleQuests = new List<Quest>();
    public List<FetchQuest> possibleFetchQuests;
    public List<HuntQuest> possibleHuntQuests;

    void Start()
    {
        possibleQuests.AddRange(possibleFetchQuests);
        possibleQuests.AddRange(possibleHuntQuests);
    }

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
            QuestManager.Instance.activeQuests.Add(possibleQuests[Random.Range(0, possibleQuests.Count)]);

            QuestManager.Instance.activeQuests[0].assignee = (Character)Random.Range(1, System.Enum.GetValues(typeof(Character)).Length);

            FetchQuest f = QuestManager.Instance.activeQuests[0] as FetchQuest;

            if(f != null) print("This is a fetch quest");

            HuntQuest h = QuestManager.Instance.activeQuests[0] as HuntQuest;

            if(h != null) print("This is a hunt quest");
        }
    }
}
