using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Night Event", menuName = "Night Events/Grub Weed Invasion")]
public class GrubWeedEvent : NightEventObject
{
    public CreatureObject grub;
    
    public int maxGrubs = 20;
    
    public override void InitiateEvent()
    {
        PopupHandler.Instance.AddToQueue(eventStartPopup);
        PopupHandler.Instance.StartCoroutine(PerformEvent());
    }

    public override bool CanStartEvent()
    {
        int weedTotal = 0;
        for(int i = 0; i < StructureManager.Instance.allStructs.Count; i++)
        {
            FarmLand weedScript = StructureManager.Instance.allStructs[i] as FarmLand;
            if(weedScript && weedScript.isWeed) weedTotal++;
        }

        if(weedTotal < 6) return false;

        if(PlayerInteraction.Instance.totalMoneyEarned >= wealthPrerequisite) return true;
        else return false;
    }

    IEnumerator PerformEvent()
    {
        yield return new WaitForSeconds(1.5f);

        int grubsSpawned = 0;
        for(int i = 0; i < StructureManager.Instance.allStructs.Count; i++)
        {
            FarmLand weedScript = StructureManager.Instance.allStructs[i] as FarmLand;
            if(weedScript && weedScript.isWeed)
            {
                if(Random.Range(0, 10) > 3) continue;
                Instantiate(grub.objectPrefab, weedScript.transform.position, Quaternion.identity);
                if(Random.Range(0, 10) > 6) Destroy(weedScript.gameObject);
                grubsSpawned++;
                if(grubsSpawned >= maxGrubs) yield break;
                yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));
            }
        }
    }
}
