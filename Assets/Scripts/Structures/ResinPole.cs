using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResinPole : StructureBehaviorScript
{
    //Use a trigger collider to stick nearby bugs
    //Structure when placed has no nectar

    public int nectarDurability = 0;
    int maxNectarDurability = 10;
    public GameObject nectarObject;

    public List<CreatureObject> attractableCreatures;
    public RepairItem nectar;

    float attractionRange = 10;

    //public List<CreatureBehaviorScript> heldCreatures = new List<CreatureBehaviorScript>();

    public List<ResinSockets> sockets;

    public ParticleSystem stickParticles; // move the object to the right transform and play it

    void Start()
    {
        base.Start();
        StartCoroutine(AttractCreatures());
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(nectarDurability >= maxNectarDurability) return;
        if(nectar.item == item)
        {
            if(maxNectarDurability <= nectar.repairAmount + nectarDurability) nectarDurability = maxNectarDurability;
            else nectarDurability += nectar.repairAmount;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            UpdateNectar();
            audioHandler.PlaySound(audioHandler.itemInteractSound);
        }
    }

    void UpdateNectar()
    {
        if(nectarDurability > 0) nectarObject.SetActive(true);
        else nectarObject.SetActive(false);
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
    }

    void HourPassed()
    {
        for(int i = 0; i < sockets.Count; i++)
        {
            if(sockets[i].creature == null) continue;
            if(nectarDurability > 1 && Random.Range(0,10) > 5 && MainMenuScript.currentFileMode != FileMode.Cozy) nectarDurability--;
        }
        if(nectarDurability <= 0) //Release the bugs
        {
            nectarDurability = 0;
            UpdateNectar();
            for(int i = 0; i < sockets.Count; i++)
            {
                if(sockets[i].creature == null) continue;

                PyreFly fly = sockets[i].creature as PyreFly;
                if(fly)
                {
                    bool ignited = fly.ignited;
                    GameObject newFly = Instantiate(fly.creatureData.objectPrefab, transform.position, Quaternion.identity);
                    newFly.GetComponent<PyreFly>().IgnitionToggle(ignited);
                }
                else
                {
                    Instantiate(sockets[i].creature.creatureData.objectPrefab, transform.position, Quaternion.identity);
                }

                Destroy(sockets[i].creature.gameObject);
            }
        }
    }

    void StickEnemy(CreatureBehaviorScript c)
    {
        for(int i = 0; i < sockets.Count; i++)
        {
            if(sockets[i].creature != null) continue;

            if(c.OnStun(999) == false) return;

            sockets[i].creature = c;
            c.transform.position = sockets[i].transform.position;
            stickParticles.transform.position = sockets[i].transform.position;
            stickParticles.Play();
            if(nectarDurability > 1 && Random.Range(0,10) > 7) nectarDurability--;
            c.persistAfterNewDay = true;

            return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        CreatureBehaviorScript c = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();

        if(c && attractableCreatures.Contains(c.creatureData) && nectarDurability > 0) StickEnemy(c);
    }

    IEnumerator AttractCreatures()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(Random.Range(5, 10));
            Collider[] nearbyCreatures = Physics.OverlapSphere(transform.position, attractionRange, 1 << 9);
            foreach(Collider collider in nearbyCreatures)
            {
                CreatureBehaviorScript c = collider.gameObject.GetComponentInParent<CreatureBehaviorScript>();

                if(c && attractableCreatures.Contains(c.creatureData) && Random.Range(0,10) > 3) c.NewPriorityTarget(this);
            }
        }
    }

    public override void LoadVariables()
    {
        nectarDurability = saveInt1;
        UpdateNectar();
    }

    public override void SaveVariables()
    {
        saveInt1 = nectarDurability;
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        structureUIVariables.valueGroups[1].value = nectarDurability;
        structureUIVariables.valueGroups[1].maxValue = maxNectarDurability;
        return structureUIVariables.valueGroups;
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 

        for(int i = 0; i < sockets.Count; i++)
        {
            if(sockets[i].creature == null) continue;

            PyreFly fly = sockets[i].creature as PyreFly;
            if(fly)
            {
                bool ignited = fly.ignited;
                GameObject newFly = Instantiate(fly.creatureData.objectPrefab, transform.position, Quaternion.identity);
                newFly.GetComponent<PyreFly>().IgnitionToggle(ignited);
            }
            else
            {
                Instantiate(sockets[i].creature.creatureData.objectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(sockets[i].creature.gameObject);
        }

        if(nectarDurability > 0)
        {
            if(Random.Range(0,10) <= nectarDurability) ItemPoolManager.Instance.GrabItem(nectar.item).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        }
    }
}
[System.Serializable]
public class ResinSockets
{
    public Transform transform;
    public CreatureBehaviorScript creature;
}
