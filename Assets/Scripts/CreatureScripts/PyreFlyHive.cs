using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PyreFlyHive : CreatureBehaviorScript//, IInteractable
{
    public bool ignited = true;
    public GameObject pyreFire;
    public Material ignitedMat, extinguishedMat, ignitedHoneyMat, extinguishedHoneyMat;
    public MeshRenderer meshRenderer;

    public Transform flySpawn;
    public CreatureObject pyreFlyData;
    int fliesActive = 0;
    int maxFlies = 3;

    bool producedNectar = false;
    public InventoryItemData nectar;
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        StartCoroutine(FlySpawn());
        //OnSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0) isDead = true;
    }

    public override void OnSpawn()
    {
        StartCoroutine(SpawnJump());
    }

    IEnumerator SpawnJump()
    {
        rb.isKinematic = false;
        Vector3 dir = (transform.position - StructureManager.Instance.GetRandomTile()).normalized;

        rb.AddForce(Vector3.up * 400);
        rb.AddForce(-dir * 550);

        yield return new WaitForSeconds(3f);
        rb.isKinematic = true;
    }

    public void FlyLost()
    {
        if(fliesActive > 0) fliesActive--;
    }

    IEnumerator FlySpawn()
    {
        float randomTime = 0;
        int cycles = 0;
        while(!isDead)
        {
            randomTime = Random.Range(13, 25);
            yield return new WaitForSeconds(randomTime);
            if(fliesActive < maxFlies && !TimeManager.Instance.isDay)
            {
                PyreFly newFly = Instantiate(pyreFlyData.objectPrefab, transform.position, Quaternion.identity).GetComponent<PyreFly>();
                if(!ignited) newFly.IgnitionToggle(false);
                newFly.homeHive = this;
                fliesActive++;
                
            }

            if(!ignited)
            {
                if(Random.Range(0, 10) > 4)
                {
                    yield return new WaitForSeconds(2.5f);
                    IgnitionToggle(true);
                }
            }

            if(cycles < 8) cycles++;
            if(cycles == 7)
            {
                producedNectar = true;
                if(ignited) meshRenderer.material = ignitedHoneyMat;
                else meshRenderer.material = extinguishedHoneyMat;
            }
        }
    }

    bool FireSourcePresent() 
    {
        foreach (var structure in structManager.allStructs)
        {
            Brazier brazier = structure as Brazier;
            if (brazier && brazier.flameLeft > 0)
            {
                return true;
            }
        }
        return false;
    }

    public void IgnitionToggle(bool IsIgnited)
    {
        if(ignited == IsIgnited) return;
        ignited = IsIgnited;

        if(ignited)
        {
            pyreFire.SetActive(true);
            if(producedNectar) meshRenderer.material = ignitedHoneyMat;
            else meshRenderer.material = ignitedMat;
        }
        else
        {
            pyreFire.SetActive(false);
            if(producedNectar) meshRenderer.material = extinguishedHoneyMat;
            else meshRenderer.material = extinguishedMat;
            ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = flySpawn.position;
            effectsHandler.MiscSound();
        }

    }

    public override void HitWithWater()
    {
        IgnitionToggle(false);
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && ignited)
        {
            PlayerInteraction.Instance.waterHeld--;
            IgnitionToggle(false);
            success = true;
        }
        else if(type == ToolType.Torch && !PlayerInteraction.Instance.torchLit && ignited)
        {
            HandItemManager.Instance.TorchFlameToggle(true);
            success = true;
        }
        else success = false;
    }

    public void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return; 
        base.OnDestroy();
        if(!producedNectar) return;
        GameObject droppedItem;
        Rigidbody itemRB;
        int r = Random.Range(1,5);
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
    }

}
