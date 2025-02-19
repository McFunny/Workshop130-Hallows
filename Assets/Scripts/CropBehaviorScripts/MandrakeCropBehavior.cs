using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Mandrake")]
public class MandrakeCropBehavior : CropBehavior
{
    public GameObject mandrake;
    public CreatureObject mandrakeData;
    public override void OnHour(FarmLand tile)
    {
        if(TimeManager.Instance.isDay == false && TimeManager.Instance.timeSkipping == true)
        {
            tile.CropDestroyed();
            return;
        }
        if(TimeManager.Instance.isDay == false && tile.crop.growthStages == tile.growthStage)
        {
            float r = Random.Range(0, 4);
            if(r <= 3)
            {
                tile.StartCoroutine(SpawnMandrake(tile));
            }
        }
    }

    IEnumerator SpawnMandrake(FarmLand tile)
    {
        Debug.Log("Spawning");
        float r = Random.Range(0.1f, 15);
        yield return new WaitForSeconds(r);
        if(tile.crop && tile.ShouldIgnoreNextGrowth() == false && TallyMandrakes() < mandrakeData.spawnCap) 
        {
            CreatureBehaviorScript mandrakeScript = Instantiate(mandrake, tile.transform.position, Quaternion.identity).GetComponent<CreatureBehaviorScript>();
            NightSpawningManager.Instance.allCreatures.Add(mandrakeScript);
            tile.CropDestroyed();
        }

    }

    int TallyMandrakes()
    {
        int currentMandrakes = 0;
        NightSpawningManager sManager = NightSpawningManager.Instance;
        foreach(CreatureBehaviorScript creature in sManager.allCreatures)
        {
            if(creature.creatureData == mandrakeData) currentMandrakes++;
        }
        return currentMandrakes;
    }
}
