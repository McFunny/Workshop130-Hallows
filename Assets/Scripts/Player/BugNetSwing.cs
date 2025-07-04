using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugNetSwing : MonoBehaviour
{
    //public LayerMask hitDetection;
    public Collider collider;

    public AudioClip collectedItem, caught;

    bool cancelSwing; //Happens when the player hits a hard thing

    SpriteRenderer bugRenderer;

    //Maybe functionality to grabbin an item, so players can get stuff slightly out of reach


    InventoryItemData caughtBug;

    void Start()
    {
        collider.enabled = false;
        if(HandItemManager.Instance.bugNet) bugRenderer = HandItemManager.Instance.bugNet.GetComponentInChildren<SpriteRenderer>();

        caughtBug = null;
    }
    
    public IEnumerator Swing()
    {
        cancelSwing = false;

        caughtBug = null;
        collider.enabled = true;
        yield return new WaitForSeconds(0.02f);
        collider.enabled = false;
        if(PlayerInteraction.Instance.stamina > 50 && caughtBug) PlayerInteraction.Instance.StaminaChange(-1);

        yield return new WaitForSeconds(2f);
        ObtainBug();
    }

    void OnTriggerEnter(Collider other)
    {
        //Vector3 collisionPoint;

        if(cancelSwing || caughtBug) return;

        var bug = other.GetComponentInParent<BugBehaviorScript>();
        if (bug != null && caughtBug == null)
        {
            caughtBug = bug.bugItem;
            bug.Captured();
            HandItemManager.Instance.toolSource.PlayOneShot(caught);
            bugRenderer.sprite = bug.bugItem.icon;
            bug.bugData.amountCaught++;
            return;
        }

        var enemy = other.GetComponentInParent<CreatureBehaviorScript>();
        if (enemy != null)
        {
            PyreFly fly = enemy as PyreFly;
            if(fly)
            {
                if(fly.ignited)
                {
                    fly.TakeDamage(999);
                }
                else
                {
                    caughtBug = fly.bugItem;
                    HandItemManager.Instance.toolSource.PlayOneShot(caught);
                    bugRenderer.sprite = caughtBug.icon;
                    Destroy(fly.gameObject);
                }
                return;
            } 
        }
        
    }

    void ObtainBug()
    {
        if(caughtBug == null) return;
        ParticlePoolManager.Instance.GrabSparkParticle().transform.position = bugRenderer.transform.position;
        if(PlayerInventoryHolder.Instance.AddToInventory(caughtBug, 1) == false) ItemPoolManager.Instance.GrabItem(caughtBug).transform.position = PlayerInventoryHolder.Instance.transform.position;
        else FindObjectOfType<PlayerEffectsHandler>().ItemCollectSFX();
        bugRenderer.sprite = null;
        caughtBug = null;
    }
}
