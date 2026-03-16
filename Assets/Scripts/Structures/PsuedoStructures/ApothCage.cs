using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApothCage : StructureBehaviorScript
{
    public FleeingApothecary apo;
    
    void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded || health > 0) return;

        apo.gameObject.transform.parent = null;
        apo.Released();

        QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.GetMainQuest(15));
        QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.GetMainQuest(16)); //Ideally this is called after the apoth is freed from her cage
        GameSaveData.Instance.apo_rescued = true;
    }
}
