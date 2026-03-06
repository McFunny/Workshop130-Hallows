using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlaskProjectile : MonoBehaviour
{
    public GameObject cloudPrefab;

    public AudioClip impactSFX;
    
    public float bulletLifetime = 8;

    private Rigidbody bulletRigidbody;

    bool exploding = false;

    public GameObject[] thingsToTurnOff;
    bool canCollide = true;
    public TrailRenderer trail;

    private void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }


    void OnTriggerEnter(Collider other)
    {
        if(exploding) return;
        exploding = true;
        Explode();

    }

    void Explode()
    {
        AudioPoolManager.Instance.PlayClipAtPosition(impactSFX, transform.position);
        
        Instantiate(cloudPrefab, transform.position, Quaternion.identity);

        //gameObject.SetActive(false);
        StartCoroutine(TurnOff());
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

    IEnumerator TurnOff()
    {
        canCollide = false;
        foreach(GameObject thing in thingsToTurnOff)
        {
            thing.SetActive(false);
        }
        bulletRigidbody.isKinematic = true;
        bulletRigidbody.velocity = Vector3.zero;
        bulletRigidbody.angularVelocity = Vector3.zero;
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
