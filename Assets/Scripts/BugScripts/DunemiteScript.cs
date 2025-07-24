using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DunemiteScript : BugBehaviorScript
{
    public StructureBehaviorScript targetStructure;

    bool inEatingRange;

    public AudioClip eatSound;

    void Start()
    {
        targetStructure = null;
        base.Start();
        StartCoroutine(FindStructure());
        StartCoroutine(EatStructure());
    }

    void Update()
    {
        base.Update();

        if(targetStructure && Vector3.Distance(transform.position, targetStructure.transform.position) < 4.5f && currentState == BugState.Wander) inEatingRange = true;
        else inEatingRange = false;
    }

    protected override void Wander()
    {
        if (!isMoving && currentState == BugState.Wander)
        {
            if(targetStructure != null)
            {
                Vector3 randomPoint = GetRandomPointAround(targetStructure.transform.position, 3.5f);
                walkRoutine = StartCoroutine(MoveToPoint(randomPoint));
            }
            else
            {
                Vector3 randomPoint;
                randomPoint = GetRandomPointAround(transform.position, 5f);
                walkRoutine = StartCoroutine(MoveToPoint(randomPoint));
            }
        }
    }

    IEnumerator FindStructure()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(5);
            if(targetStructure != null) continue;

            List<StructureBehaviorScript> nearbyStructures = StructureManager.Instance.allStructs;
            if(nearbyStructures.Count == 0) continue;

            StructureBehaviorScript nearestStructure = null;
            float minDistance = 1000;

            for(int i = 0; i < nearbyStructures.Count; i++)
            {
                if(nearbyStructures[i].structData && (nearbyStructures[i].structData.structureType == StructureType.Wood || nearbyStructures[i].structData.structureType == StructureType.Hay) &&
                 (!nearestStructure || Vector3.Distance(transform.position, nearbyStructures[i].transform.position) < minDistance))
                {
                    nearestStructure = nearbyStructures[i];
                    minDistance = Vector3.Distance(transform.position, nearbyStructures[i].transform.position);
                }
            }

            targetStructure = nearestStructure;
        }
    }

    IEnumerator EatStructure()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(3);
            if(targetStructure == null || !inEatingRange) continue;

            targetStructure.TakeDamage(0.5f);
            AudioPoolManager.Instance.PlayClipAtPosition(eatSound, transform.position);
        }
    }
}
