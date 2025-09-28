using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaltLick : StructureBehaviorScript
{
    public List<GameObject> stageObjects;

    float useChance = 5;
    float hungerRestored = 10;

    void Start()
    {
        base.Start();
        StartCoroutine(InteractWithAnimals());
        OnDamage += UpdateModel;
    }

    void UpdateModel()
    {
        StartCoroutine(UpdateModelDelay());
    }

    IEnumerator UpdateModelDelay()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (GameObject obj in stageObjects) obj.SetActive(false);

        if(health < 4) stageObjects[2].SetActive(true); //small
        else if(health < 7) stageObjects[1].SetActive(true); //medium
        else stageObjects[0].SetActive(true); //big
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUp());
            success = true;
        }
    }

    IEnumerator InteractWithAnimals()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(15, 30));
            if(Random.Range(0, 100) > useChance) continue;

            Collider[] nearbyCritters = Physics.OverlapSphere(transform.position, 30, 1 << 9);
            foreach(Collider collider in nearbyCritters)
            {
                CritterBehaviorScript c = collider.gameObject.GetComponentInParent<CritterBehaviorScript>();

                if(c && c.hunger <= c.maxHunger - hungerRestored && c.friendshipLevel < c.MaxLevel && Random.Range(0,10) > 5)
                {
                    c.FriendPointsChange(10, true);
                    c.hunger += hungerRestored;
                    health -= 1;
                    UpdateModel();
                }
            }
        }
    }

    public override void LoadVariables()
    {
        UpdateModel();
    }

    void OnDestroy()
    {
        base.OnDestroy();
        OnDamage -= UpdateModel;
    }
}
