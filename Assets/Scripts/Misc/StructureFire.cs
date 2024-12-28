using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureFire : MonoBehaviour
{
    public StructureBehaviorScript burningStruct;
    public Transform flameBase;

    public Collider burnBox; //collider that damages enemies and players that wander into it

    public AudioClip extinguishedSFX;

    float playerDamage = 6;
    float creatureDamage = 10;

    // Update is called once per frame
    void Update()
    {
        if(!burningStruct || !burningStruct.onFire)
        {
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        StartCoroutine(ColliderToggle());
    }

    void OnDisable()
    {
        if(!gameObject.scene.isLoaded) return;
        ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = flameBase.position;
        AudioSource.PlayClipAtPoint(extinguishedSFX, transform.position);
        StopAllCoroutines();
    }

    void OnTriggerEnter(Collider other) //Burn things that come into contact
    {
        var player = other.GetComponent<PlayerInteraction>();
        if (player != null)
        {
            player.StaminaChange(-playerDamage);
        }

        var creature = other.GetComponentInParent<CreatureBehaviorScript>();
        if (creature != null && creature.shovelVulnerable)
        {
            creature.TakeDamage(creatureDamage);

            creature.PlayHitParticle(new Vector3(0, 0, 0));
        }
    }

    IEnumerator ColliderToggle()
    {
        int burnTimer = 0;
        List<StructureBehaviorScript> nearbyStructs = new List<StructureBehaviorScript>();
        while(gameObject.activeSelf)
        {
            burnBox.enabled = true;
            yield return new WaitForSeconds(0.4f);
            burnBox.enabled = false;
            yield return new WaitForSeconds(0.1f);
            burnTimer++;

            if(burnTimer >= 3)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
                foreach(Collider collider in hitColliders)
                {
                    StructureBehaviorScript newStruct = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                    if(newStruct && !nearbyStructs.Contains(newStruct))
                    {
                        nearbyStructs.Add(newStruct);
                    }
                }

                foreach(StructureBehaviorScript structure in nearbyStructs)
                {
                    if(structure && structure.IsFlammable() && !structure.onFire)
                    {
                        int r = Random.Range(0,10);
                        if(r > 4) structure.LitOnFire();
                        break;
                    }
                }
                burnTimer = 0;
                nearbyStructs.Clear();
            }
        }
    }
}
