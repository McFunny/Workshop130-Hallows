using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PyreFlyHive : CreatureBehaviorScript//, IInteractable
{
    public bool ignited = true;
    public GameObject pyreFire;
    public Material ignitedMat, extinguishedMat;
    public MeshRenderer meshRenderer;

    public Transform flySpawn;
    public CreatureObject pyreFlyData;
    int fliesActive = 0;
    int maxFlies = 3;

    bool producedNectar = false;
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
            randomTime = Random.Range(7, 20);
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
                if(Random.Range(0, 10) > 3)
                {
                    yield return new WaitForSeconds(2.5f);
                    IgnitionToggle(true);
                }
            }

            if(cycles < 6) cycles++;
            if(cycles == 5) producedNectar = true;
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
            meshRenderer.material = ignitedMat;
            //mat changes
        }
        else
        {
            pyreFire.SetActive(false);
            meshRenderer.material = extinguishedMat;
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
        else success = false;
    }

}
