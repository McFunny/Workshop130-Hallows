using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : StructureBehaviorScript
{
    //public InventoryItemData gloomStalk;

    public MeshRenderer brokenBox;

    public Material clearM, brokenM, veryBrokenM;

    public List<RepairItem> repairItems;

    public Transform mount;
    [HideInInspector] public bool catOnStruct;

    public GameObject damageObject1, damageObject2;

    //Bramble only
    public Transform thornAttackPos;
    public ParticleSystem thornParticles;
    bool cooldown = false;


    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        UpdateModel();
        OnDamage += UpdateModel;
        if(thornParticles) OnDamage += ThornAttack;
    }

    void Update()
    {
        if(!catOnStruct) base.Update();
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(health >= maxHealth) return;
        foreach(RepairItem r in repairItems)
        {
            if(r.item == item)
            {
                if(maxHealth <= r.repairAmount + health) health = maxHealth;
                else health += r.repairAmount;
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();
                UpdateModel();

                PlayHitEffect();
                return;
            }
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !absentFromGrid && !catOnStruct)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
    }

    void ThornAttack()
    {
        if(thornParticles == null || thornAttackPos == null || cooldown) return;

        StartCoroutine(ThornCooldown());

        audioHandler.PlaySound(audioHandler.activatedSound);

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 1.7f, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.TakeDamage(10);
                creature.PlayHitParticle(creature.transform.position);
            }
        }
    }

    IEnumerator ThornCooldown()
    {
        cooldown = true;
        yield return new WaitForSeconds(1);
        cooldown = false;
    }

    void UpdateModel()
    {
        if(health > (maxHealth/3) * 2)
        {
            brokenBox.material = clearM;
            if(damageObject1)
            {
                damageObject1.SetActive(false);
                damageObject2.SetActive(false);
            }
        }
        else if(health > maxHealth/3)
        {
            brokenBox.material = brokenM;
            if(damageObject1)
            {
                damageObject1.SetActive(false);
                damageObject2.SetActive(true);
            }
            
        }
        else
        {
            brokenBox.material = veryBrokenM;
            if(damageObject1)
            {
                damageObject1.SetActive(true);
                damageObject2.SetActive(true);
            }
        }
    }

    public override bool RepairWithSealant(int amount)
    {
        if(!repairableWithGlue || health == maxHealth) return false;
        health += amount;
        if(health > maxHealth) health = maxHealth;
        UpdateModel();
        return true;
    }

    void OnDestroy()
    {
        OnDamage -= UpdateModel;
        if(thornParticles) OnDamage -= ThornAttack;
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
    }

    public override void LoadVariables()
    {
        //UpdateModel();
    }
}
