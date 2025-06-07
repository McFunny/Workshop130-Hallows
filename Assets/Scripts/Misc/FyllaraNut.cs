using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FyllaraNut : StructureBehaviorScript
{
    float damageToPlayer = 25;
    float damageToCreature = 75;
    bool hasDealtDamage = false;

    public InventoryItemData nut;

    public Rigidbody rb;

    LayerMask clearMask = 0;

    void Start()
    {
        OnDamage += TreeNutDrop;
        audioHandler = GetComponent<StructureAudioHandler>();
    }

    void OnDestroy()
    {
        OnDamage -= TreeNutDrop;
    }

    public override void HitWithWater()
    {
        TreeNutDrop();
    }

    void TreeNutDrop()
    {
        if(rb.useGravity == true) return;
        GetComponent<Collider>().excludeLayers = clearMask;
        transform.parent = null;
        rb.useGravity = true;
        Vector3 dir3 = Random.onUnitSphere;
        dir3 = new Vector3(dir3.x, transform.position.y, dir3.z);
        rb.AddForce(dir3 * 5);
    }

    void OnTriggerEnter(Collider other)
    {
        if(!rb.useGravity) return;
        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {
            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(nut);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

            audioHandler.PlaySoundAtPoint(audioHandler.breakSound,transform.position);

            Destroy(gameObject);
        }
        if(hasDealtDamage) return;
        if(other.gameObject.CompareTag("Player"))
        {
            PlayerInteraction.Instance.StaminaChange(-damageToPlayer);
            hasDealtDamage = true;
            return;
        }

        if(other.gameObject.layer == 9)
        {
            var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.TakeDamage(damageToCreature);
                creature.PlayHitParticle(new Vector3(0,0,0));

                hasDealtDamage = true;
            }
        }
    }
}
