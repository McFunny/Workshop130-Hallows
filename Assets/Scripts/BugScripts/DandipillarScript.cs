using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DandipillarScript : BugBehaviorScript
{
    public FarmLand targetPlant;

    public StructureObject farmData;

    void Start()
    {
        targetPlant = null;
        base.Start();
        StartCoroutine(FindCrop());
    }

    void Update()
    {
        base.Update();

        if(targetPlant && Vector3.Distance(transform.position, targetPlant.transform.position) < 2) PlantWeeds();
    }

    protected override void Wander()
    {
        if (!isMoving && currentState == BugState.Wander)
        {
            if(targetPlant != null)
            {
                walkRoutine = StartCoroutine(MoveToPoint(targetPlant.transform.position));
            }
            else
            {
                Vector3 randomPoint;
                randomPoint = GetRandomPointAround(transform.position, 5f);
                walkRoutine = StartCoroutine(MoveToPoint(randomPoint));
            }
        }
    }

    IEnumerator FindCrop()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(5);
            if(targetPlant != null) continue;

            List<GameObject> nearbyCrops = StructureManager.Instance.ReturnStructuresOfType(farmData);
            if(nearbyCrops.Count == 0) continue;

            int x = 0;
            FarmLand selectedTile = null;
            while(x < 10 && selectedTile == null)
            {
                selectedTile = nearbyCrops[Random.Range(0, nearbyCrops.Count)].GetComponent<FarmLand>();
                if(selectedTile && selectedTile.crop && selectedTile.currentUpgrade == FarmLand.FarmTileUpgrade.None) x += 20;
                else selectedTile = null;
                x++;
            }
            targetPlant = selectedTile;
        }
    }

    void PlantWeeds()
    {
        targetPlant.ApplyNewUpgrade(FarmLand.FarmTileUpgrade.MiniWeeds);
        targetPlant = null;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
    }
}
