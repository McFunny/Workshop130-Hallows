using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrakhenEgg : StructureBehaviorScript
{
    public CreatureObject krakhen;
    public InventoryItemData gunPowder;

    public Rigidbody rb;

    bool hatched;

    void Start()
    {
        audioHandler = GetComponent<StructureAudioHandler>();
        StartCoroutine(HatchEgg());
    }

    public override void HitWithWater()
    {
        TakeDamage(10);
    }

    IEnumerator HatchEgg()
    {
        yield return new WaitForSeconds(Random.Range(40, 60));
        Instantiate(krakhen.objectPrefab, transform.position, Quaternion.identity);
        hatched = true;
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if(!rb.useGravity) return;
        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = transform.position;
        audioHandler.PlaySoundAtPoint(audioHandler.breakSound, transform.position);

        int r = Random.Range(1, 3);
        if(hatched) r -= Random.Range(1, 4);
        for(int i = 0; i < r; ++i)
        {
            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(gunPowder);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }
    }
}
