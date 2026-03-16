using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlueSpikeTrap : StructureBehaviorScript
{
    public AudioClip contactSFX;

    public bool isSlime;
    
    void OnTriggerEnter(Collider other)
    {
        CreatureBehaviorScript c = other.GetComponentInParent<CreatureBehaviorScript>();
        if(c && c.shovelVulnerable && (c as ICritter) == null)
        {
            c.TakeDamage(20);
            AudioPoolManager.Instance.PlayClipAtPosition(contactSFX, transform.position);
            ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = c.transform.position;
            c.PlayHitParticle(Vector3.zero);
            health--;
            if(isSlime) ParticlePoolManager.Instance.GrabSlimeSplashParticle().transform.position = transform.position;
        } 

        if(other.gameObject.layer == 10)
        {
            if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.HareBoots)) return;

            PlayerInteraction.Instance.StaminaChange(-6);
            AudioPoolManager.Instance.PlayClipAtPosition(contactSFX, transform.position);
            health--;
            if(isSlime) ParticlePoolManager.Instance.GrabSlimeSplashParticle().transform.position = transform.position;
        }
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

    public override void HitWithWater()
    {
        if(isSlime) TakeDamage(99);
        else TakeDamage(1);
    }
}
