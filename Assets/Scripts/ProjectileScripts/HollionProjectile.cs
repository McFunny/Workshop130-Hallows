using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HollionProjectile : MonoBehaviour
{
    public float damageToCreature, damageToPlayer;

    private Rigidbody bulletRigidbody;

    public GameObject explosion; //unparent then enable

    bool detonated = false;

    public InventoryItemData berryItem;

    private void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9 || other.gameObject.layer == 10)
        {
            var creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if ((creature != null && !creature.shovelVulnerable) || detonated) return;
            detonated = true;
            Detonate();
        }

    }

    public void Detonate()
    {
        detonated = true;
        if(Random.Range(0, 20) == 19)
        {
            ItemPoolManager.Instance.GrabItem(berryItem).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            Destroy(gameObject);
            return;
        }
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) < 4f)
        {
            PlayerInteraction.Instance.StaminaChange(-damageToPlayer);
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 5f, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.TakeDamage(damageToCreature);
                creature.PlayHitParticle(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            }
        }

        /*Collider[] hitBullets = Physics.OverlapSphere(transform.position, 5f, 1 << 12);
        foreach(Collider collider in hitBullets)
        {
            var berry = collider.GetComponentInParent<HollionProjectile>();
            if (berry != null && !berry.detonated)
            {
                berry.Detonate();
            }
        }*/

        //unparent
        explosion.SetActive(true);
        explosion.transform.SetParent(null);

        Destroy(gameObject);
    }

    void OnEnable()
    {
        StartCoroutine(LifeTime());
    }

    void OnDisable()
    {
        StopCoroutine(LifeTime());
    }

    IEnumerator LifeTime()
    {
        float bulletLifetime = Random.Range(35,60);
        yield return new WaitForSeconds(bulletLifetime);
        if(!detonated) Detonate();
        //maybe plant a new one if the tile is free
    }
}
