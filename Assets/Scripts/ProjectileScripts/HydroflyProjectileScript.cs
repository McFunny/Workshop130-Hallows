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

    public GameObject[] thingsToTurnOff;
    bool canCollide = true;
    public TrailRenderer trail;

    private void Awake()
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
        AudioPoolManager.Instance.PlayClipAtPosition(explodeSFX, transform.position, 0.7f, 40);
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) < 6f)
        {
            StatusEffectManager.Instance.RemoveStatusOnPlayer(StatusEffectName.Fire);
        }
        Collider[] hitStructures = Physics.OverlapSphere(transform.position, 3f, 1 << 6);

        List<IWaterHolder> waterHolders = new List<IWaterHolder>();
        int structuresHit = 0;
        foreach(Collider collider in hitStructures)
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            if(structure)
            {
                IWaterHolder wHolder = structure as IWaterHolder;
                if(wHolder != null) waterHolders.Add(wHolder);
                else
                {
                    structure.HitWithWater();
                    structuresHit++;
                }
            }
        }

        for(int s = 0; s < waterHolders.Count; ++s) //Fills up water holds more effectively if there is not too many of them in the vicinity
        {
            if(waterHolders[s] != null)
            {
                if(waterHolders.Count < 3 && structuresHit < 6) for(int i = 0; i < 3; ++i) waterHolders[s].GivenWater();
                else waterHolders[s].GivenWater();
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
        else StartCoroutine(TurnOff());//gameObject.SetActive(false);
    }

    void OnEnable()
    {
        exploding = false;
        StartCoroutine(LifeTime());

        foreach(GameObject thing in thingsToTurnOff)
        {
            thing.SetActive(true);
        }
    }

    void OnDisable()
    {
        StopCoroutine(LifeTime());
        bulletRigidbody.velocity = Vector3.zero;
        bulletRigidbody.angularVelocity = Vector3.zero;
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(bulletLifetime);
        if(gameObject.activeSelf && !exploding) Explode();
    }

    IEnumerator TurnOff()
    {
        canCollide = false;
        foreach(GameObject thing in thingsToTurnOff)
        {
            thing.SetActive(false);
        }
        bulletRigidbody.velocity = Vector3.zero;
        bulletRigidbody.angularVelocity = Vector3.zero;
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }
}
