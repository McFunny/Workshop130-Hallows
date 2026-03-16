using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimsonMothNest : StructureBehaviorScript
{
    public Rigidbody rb;

    LayerMask clearMask = 0;

    public CreatureObject mothData;

    public int heldWasps = 3;
    public int outsideWasps;

    public InventoryItemData nectar, comb;

    bool yielditems = false;
    bool releasing = false;

    void Start()
    {
        OnDamage += HiveDrop;
        audioHandler = GetComponent<StructureAudioHandler>();
        StartCoroutine(ScanForPlayer());
        yielditems = true;

        if(!TimeManager.Instance.isDay) ReleaseWasps();
    }

    void OnDestroy()
    {
        OnDamage -= HiveDrop;
        TimeManager.OnHourlyUpdate -= HourPassed;
        if (!gameObject.scene.isLoaded || !yielditems) return; 
        GameObject droppedItem;
        Rigidbody itemRB;
        int r = Random.Range(-1,2);
        for(int i = 0; i < r; i++)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(nectar);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            Vector3 dir3 = Random.onUnitSphere;
            dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(dir3 * 20);
            itemRB.AddForce(Vector3.up * 50);
        }

        r = Random.Range(3,6);
        for(int i = 0; i < r; i++)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(comb);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            Vector3 dir3 = Random.onUnitSphere;
            dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(dir3 * 20);
            itemRB.AddForce(Vector3.up * 50);
        }
    }

    IEnumerator ScanForPlayer()
    {
        while(true)
        {
            yield return new WaitForSeconds(2);
            if(Vector3.Distance(PlayerInteraction.Instance.transform.position, transform.position) < 11)
            {
                for(int i = 0; i < heldWasps; i++)
                {
                    print("Instantiated");
                    GameObject newMoth = Instantiate(mothData.objectPrefab, 
                    new Vector3(transform.position.x + Random.Range(-.5f, .5f), transform.position.y + Random.Range(-.5f, .5f), transform.position.z + Random.Range(-.5f, .5f)), Quaternion.identity);
                    newMoth.GetComponent<RubyWasp>().homeNest = this;
                    if(i >= 3) i = heldWasps;
                }
                heldWasps = 0;
            }
        }
    }

    public override void HourPassed()
    {
        if(Random.Range(0,10) > 6 && heldWasps + outsideWasps < 3) heldWasps++;

        if(heldWasps > 0 && !TimeManager.Instance.isDay)
        {
            ReleaseWasps();
        }
    }

    void ReleaseWasps()
    {
        for(int i = 0; i < heldWasps; i++)
        {
            Instantiate(mothData.objectPrefab, transform.position, Quaternion.identity).GetComponent<RubyWasp>().homeNest = this;
            if(i >= 3) break;
        }
        heldWasps = 0;
    }

    IEnumerator ReleaseWaspsOverTime()
    {
        releasing = true;
        for(int i = 0; i < heldWasps; i++)
        {
            Instantiate(mothData.objectPrefab, transform.position, Quaternion.identity).GetComponent<RubyWasp>().homeNest = this;
            if(i >= 3) break;
            yield return new WaitForSeconds(Random.Range(0.1f, 0.7f));
        }
        heldWasps = 0;
        releasing = false;
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

        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = transform.position;
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

            if(!releasing) StartCoroutine(ReleaseWaspsOverTime());

            audioHandler.PlaySoundAtPoint(audioHandler.breakSound,transform.position);

            Destroy(gameObject);
        }
    }
}
