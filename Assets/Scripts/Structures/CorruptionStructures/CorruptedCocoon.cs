using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedCocoon : StructureBehaviorScript
{
    public List<ObjectWithProbability> hatchables;

    bool hatched;

    Transform player;

    public float hatchRange = 15;

    public ParticleSystem hatchParticles;

    public GameObject hatchedModel, intactModel;

    public GameObject seedPod;

    void Start()
    {
        base.Start();
        player = PlayerInteraction.Instance.transform;
        StartCoroutine(TrackPlayer());
    }

    void Update()
    {
        if(health <= 0 && !hatched)
        {
            Hatch();
        }
    }

    IEnumerator TrackPlayer()
    {
        float dist = 0;
        float checkTime = Random.Range(0.7f, 1.1f);
        while(!hatched)
        {
            yield return new WaitForSeconds(checkTime);
            dist = Vector3.Distance(player.position, transform.position);
            if(dist < hatchRange && !hatched) Hatch();

            if(dist > 100) yield return new WaitForSeconds(5);
        }
    }

    public void Hatch()
    {
        if(hatched) return;
        hatched = true;
        SwapModels();

        audioHandler.PlaySound(audioHandler.breakSound);

        hatchParticles.Play();

        int attempts = 0;
        GameObject newObject = null;

        while(!newObject)
        {
            int i = Random.Range(0, hatchables.Count);
            if(hatchables[i]._probability > Random.Range(0,100) || attempts >= 10)
            {
                newObject = Instantiate(hatchables[i]._object, transform.position, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
            }
            attempts++;
        }

        CreatureBehaviorScript c = newObject.GetComponentInChildren<CreatureBehaviorScript>();
        if(!c)
        {
            newObject.transform.position = new Vector3(newObject.transform.position.x, newObject.transform.position.y + 1.5f, newObject.transform.position.z);
            FyllaraNut nut = newObject.GetComponent<FyllaraNut>();
            if(nut) nut.TreeNutDrop();
            return;
        }
        c.patrolPoint = transform;
        c.inWilderness = true;

        if(Random.Range(0, 5) > 1) 
        {
            newObject = Instantiate(seedPod, transform.position, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
            newObject.transform.position = new Vector3(newObject.transform.position.x, newObject.transform.position.y + 1.5f, newObject.transform.position.z);
        }
    }

    void SwapModels()
    {
        if(hatched)
        {
            hatchedModel.SetActive(true);
            intactModel.SetActive(false);
        }
        else
        {
            hatchedModel.SetActive(false);
            intactModel.SetActive(true);
        }
    }
}
