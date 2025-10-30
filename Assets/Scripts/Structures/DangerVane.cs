using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerVane : StructureBehaviorScript
{
    public static List<CreatureBehaviorScript> trackedCreatures = new List<CreatureBehaviorScript>();

    public CreatureBehaviorScript trackedCreature;

    public List<CreatureObject> ignoredCreatures;

    public GameObject trackingEffect;
    public Transform pivot;
    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
        base.Start();
        StartCoroutine(DetectCreatures());
    }

    void Update()
    {
        if(trackedCreature) 
        {
            trackingEffect.transform.position = trackedCreature.transform.position;

            Vector3 targetPosition = trackedCreature.transform.position;
            targetPosition.y = pivot.position.y;

            pivot.LookAt(targetPosition);
        }
        else if(trackingEffect.activeSelf) trackingEffect.SetActive(false);
    }

    /*void OnTriggerEnter(Collider other)
    {
        if(trackedCreature) return;
        CreatureBehaviorScript c = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();

        if(c && c.shovelVulnerable && !trackedCreatures.Contains(c))
        {
            trackedCreature = c;
            trackingEffect.SetActive(true);
            trackedCreatures.Add(c);
            audioHandler.PlaySound(audioHandler.interactSound);
        }
    }*/

    IEnumerator DetectCreatures()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(Random.Range(2, 5));
            if(trackedCreature)
            {
                if(Vector3.Distance(transform.position, trackedCreature.transform.position) > 30 || trackedCreature.health <= 0) 
                {
                    trackedCreature = null;
                    trackedCreatures.Remove(trackedCreature);
                }
                else continue;
            }

            Collider[] nearbyCreatures = Physics.OverlapSphere(transform.position, 25, 1 << 9);
            foreach(Collider collider in nearbyCreatures)
            {
                CreatureBehaviorScript c = collider.gameObject.GetComponentInParent<CreatureBehaviorScript>();

                if(c && c.health > 0 && c.shovelVulnerable && !trackedCreatures.Contains(c) && !ignoredCreatures.Contains(c.creatureData))
                {
                    trackedCreature = c;
                    trackingEffect.SetActive(true);
                    trackedCreatures.Add(c);
                    audioHandler.PlaySound(audioHandler.interactSound);
                    break;
                }
            }
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 

        if(trackedCreature)
        {
            trackedCreatures.Remove(trackedCreature);
        }
    }
}
