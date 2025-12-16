using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydroflyProjectileScript : MonoBehaviour
{
    public AudioClip explodeSFX;

    public float bulletLifetime = 3;

    private Rigidbody bulletRigidbody;

    bool exploding = false;

    public GameObject bigSplashEffect;

    public bool destroyOnUse = false;

    private void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }


    void OnTriggerEnter(Collider other)
    {
        print("Collided with: " + other.gameObject + ". Am I already exploding? " + exploding);

        if(exploding) return;
        exploding = true;
        Explode();

    }

    void Explode()
    {
        bigSplashEffect.transform.position = transform.position;
        bigSplashEffect.transform.rotation = Quaternion.identity;
        bigSplashEffect.SetActive(false);
        bigSplashEffect.SetActive(true);
        bigSplashEffect.transform.parent = null;
        AudioPoolManager.Instance.PlayClipAtPosition(explodeSFX, transform.position);
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) < 6f)
        {
            StatusEffectManager.Instance.RemoveStatusOnPlayer(StatusEffectName.Fire);
        }
        Collider[] hitStructures = Physics.OverlapSphere(transform.position, 3f, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            if(structure)
            {
                structure.HitWithWater();
            }
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 4f, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null)
            {
                creature.HitWithWater();
            }
        }

        if(destroyOnUse) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    void OnEnable()
    {
        exploding = false;
        StartCoroutine(LifeTime());
    }

    void OnDisable()
    {
        StopCoroutine(LifeTime());
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(bulletLifetime);
        if(gameObject.activeSelf && !exploding) Explode();
    }
}
