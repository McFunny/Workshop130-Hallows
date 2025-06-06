using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Lunorchid")]
public class LunorchidBehavior : CropBehavior
{
    public CreatureObject bug;

    public override void OnHour(FarmLand tile)
    {
        //Check nightspawning manager's count of bugs, spawn at the start of night
        if(TimeManager.Instance.currentHour == 20)
        {
            TimeManager.Instance.StartCoroutine(TrySpawnPollinator());
        }
    }

    IEnumerator TrySpawnPollinator()
    {
        yield return new WaitForSeconds(Random.Range(3, 15));
        int currentBugs = NightSpawningManager.Instance.ReportTotalOfCreature(bug);
        float chanceOfExtraBug = 150;
        if(currentBugs < 2 || chanceOfExtraBug/(currentBugs - 1) > Random.Range(0, 100))
        {
            NightSpawningManager.Instance.SpawnCreature(bug);
        }
    }
}
