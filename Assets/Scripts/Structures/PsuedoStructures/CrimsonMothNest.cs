using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimsonMothNest : StructureBehaviorScript
{
    public Rigidbody rb;

    LayerMask clearMask = 0;

    public CreatureObject mothData;

    public int heldWasps = 3;

    void Start()
    {
        OnDamage += HiveDrop;
        audioHandler = GetComponent<StructureAudioHandler>();
        StartCoroutine(ScanForPlayer());
    }

    void OnDestroy()
    {
        OnDamage -= HiveDrop;
    }

    IEnumerator ScanForPlayer()
    {
        while(true)
        {
            yield return new WaitForSeconds(2);
            if(Vector3.Distance(PlayerInteraction.Instance.transform.position, transform.position) < 15)
            {
                for(int i = 0; i < heldWasps; i++)
                {
                    print("Instantiated");
                    GameObject newMoth = Instantiate(mothData.objectPrefab, 
                    new Vector3(transform.position.x + Random.Range(-.5f, .5f), transform.position.y + Random.Range(-.5f, .5f), transform.position.z + Random.Range(-.5f, .5f)), Quaternion.identity);
                    newMoth.GetComponent<RubyWasp>().homeNest = this;
                }
                heldWasps = 0;
            }
        }
    }

    public override void HourPassed()
    {
        if(heldWasps < 3) heldWasps++;

        if(heldWasps == 3 && !TimeManager.Instance.isDay)
        {
            heldWasps--;
            Instantiate(mothData.objectPrefab, transform.position, Quaternion.identity).GetComponent<RubyWasp>().homeNest = this;
        }
    }

    public override void HitWithWater()
    {
        HiveDrop();
    }

    void HiveDrop()
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
            //GameObject droppedItem = ItemPoolManager.Instance.GrabItem(nut);
            //droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
            ParticlePoolManager.Instance.GrabCorpseParticle(CorpseParticleType.Yellow).transform.position = transform.position;

            for(int i = 0; i < heldWasps; i++)
            {
                Instantiate(mothData.objectPrefab, transform.position, Quaternion.identity).GetComponent<RubyWasp>().homeNest = this;
            }

            audioHandler.PlaySoundAtPoint(audioHandler.breakSound,transform.position);

            Destroy(gameObject);
        }
    }
}
