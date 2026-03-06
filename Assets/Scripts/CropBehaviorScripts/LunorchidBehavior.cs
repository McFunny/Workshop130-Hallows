using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Lunorchid")]
public class LunorchidBehavior : CropBehavior
{
    public CreatureObject bug;

    public List<CreatureObject> attractedBugs = new List<CreatureObject>();

    public override void OnHour(FarmLand tile)
    {
        //Check nightspawning manager's count of bugs, spawn at the start of night
        if(TimeManager.Instance.currentHour == 20)
        {
            TimeManager.Instance.StartCoroutine(TrySpawnPollinator());
            NightSpawningManager.Instance.ChangeMaxMoths(1);
        }
    }

    public override void OnPlanted(FarmLand tile)
    {
        if(!TimeManager.Instance.isDay)
        {
            NightSpawningManager.Instance.ChangeMaxMoths(1);
        }
    }

    public override void OnCropDestroyed(FarmLand tile)
    {
        NightSpawningManager.Instance.ChangeMaxMoths(-1);
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

    public override void BehaviorUpdate(FarmLand tile)
    {
        float range = 15f;

        Collider[] hitEnemies = Physics.OverlapSphere(tile.transform.position, range, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && attractedBugs.Contains(creature.creatureData))
            {
                creature.NewPriorityTarget(tile);
            }
        }
    }
}
