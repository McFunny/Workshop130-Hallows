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
        Destroy(gameObject);
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
