using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Night Event", menuName = "Night Events/Spider Piles")]
public class SpiderPileEvent : NightEventObject
{
    public StructureObject farmTree;
    public override void InitiateEvent()
    {
        PopupHandler.Instance.AddToQueue(eventStartPopup);
        PopupHandler.Instance.StartCoroutine(PerformEvent());
    }

    public override bool CanStartEvent()
    {
        if(StructureManager.Instance.TallyStructure(farmTree) == 0) return false;

        if(PlayerInteraction.Instance.totalMoneyEarned >= wealthPrerequisite) return true;
        else return false;
    }

    IEnumerator PerformEvent()
    {
        //List<FarmTree> trees = new List<FarmTree>();
        yield return new WaitForSeconds(1.5f);

        int pilesToSpawn = 3;
        foreach (var structure in StructureManager.Instance.allStructs)
        {
            if (structure && !structure.absentFromFarmGrid)
            {
                FarmTree f = structure as FarmTree;
                if(f && f.GetType() == TreeType.Orange)
                {
                    pilesToSpawn = Random.Range(0, 4);
                    for(int i = 0; i < pilesToSpawn; ++i)
                    {
                        f.StartCoroutine(f.SpawnLeafPile(true));
                        yield return new WaitForSeconds(0.2f);
                    }
                }
            }
                
        }


    }
}
