using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Thurible : StructureBehaviorScript
{
    //public FireFearTrigger fireTrigger;
    public GameObject fire, leafObject;

    public ParticleSystem leafParticles;

    public InventoryItemData leafItem;

    public TextMeshProUGUI leafText;

    public int leafCount = 0;
    int maxLeafCount = 5; //Each count is worth max flame

    public float flameLeft; //if 0, fire is gone
    float maxFlame = 60; //Max flame per level

    public GameObject laventSource;

    public AudioSource loopSource;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        StartCoroutine(FireDrain());
        StartCoroutine(ScareBugs());
        flameLeft = 0;
        fire.SetActive(false);
    }

    void Update()
    {
        base.Update();

        //leafText.text = leafCount + "/" + maxLeafCount;
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        if(type == ToolType.Torch)
        {
            if(PlayerInteraction.Instance.torchLit && flameLeft <= 0 && leafCount > 0)
            {
                LeafChange(-1);
                flameLeft = maxFlame;
                fire.SetActive(true);
                leafParticles.Play();
                //audioHandler.PlaySound(audioHandler.activatedSound);
                laventSource.SetActive(true);
                loopSource.Play();
                success = true;
            }
            else success = false;
            return;
        }
        else if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && flameLeft > 0)
        {
            PlayerInteraction.Instance.waterHeld--;
            HitWithWater();
            success = true;
        }
        else if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
        else success = false;
        
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if (item == leafItem && leafCount < maxLeafCount)
        {
            LeafChange(1);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
        }
    }

    public override void HitWithWater()
    {
        if(flameLeft <= 0) return;
        StopCoroutine(FireDrain());
        flameLeft = 0;
        ExtinguishFlame();
    }

    IEnumerator FireDrain()
    {
        int r;
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(1);
            flameLeft -= 1;
            if(flameLeft <= 0) flameLeft = 0;
            if(flameLeft == 0 && fire.activeSelf)
            {
                if(leafCount == 0) ExtinguishFlame();
                else 
                {
                    LeafChange(-1);
                    flameLeft = maxFlame;
                }
            }
        }
    }

    IEnumerator ScareBugs()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(0.5f);
            if(flameLeft == 0) continue;

            float range = 9f;

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, range, 1 << 9);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null)
                {
                    creature.NearLaventLeaf(laventSource);
                }
            }

            Collider[] hitBugs = Physics.OverlapSphere(transform.position, range, 1 << 21);
            foreach(Collider collider in hitBugs)
            {
                var bug = collider.GetComponentInParent<BugBehaviorScript>();
                if (bug != null)
                {
                    bug.NearLaventLeaf(transform.position);
                }
            }
        }
    }

    void LeafChange(int amount)
    {
        leafCount += amount;
        if(leafCount > 0) leafObject.SetActive(true);
        else leafObject.SetActive(false);
    }

    void ExtinguishFlame()
    {
        ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = fire.transform.position;
        fire.SetActive(false);
        audioHandler.PlaySound(audioHandler.miscSounds1[0]);
        leafParticles.Stop();
        laventSource.SetActive(false);
        loopSource.Stop();
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop seeds
        GameObject droppedItem;
        for(int i = 0; i < leafCount; i++)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(leafItem);
            droppedItem.transform.position = focalPoint.position;
        }
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        structureUIVariables.valueGroups[1].value = flameLeft;
        structureUIVariables.valueGroups[1].maxValue = maxFlame;

        structureUIVariables.valueGroups[2].value = leafCount;
        structureUIVariables.valueGroups[2].maxValue = maxLeafCount;
        return structureUIVariables.valueGroups;
    }
}
