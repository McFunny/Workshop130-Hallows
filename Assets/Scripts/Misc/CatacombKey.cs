using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatacombKey : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2);
        if(GameSaveData.Instance.keyCollected) Destroy(this.gameObject);
    }

    void OnDisable()
    {
        if (!gameObject.scene.isLoaded) return;
        GameSaveData.Instance.keyCollected = true;
        QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.GetMainQuest(5));
    }
}
