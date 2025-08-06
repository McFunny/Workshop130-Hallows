using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : StructureBehaviorScript
{
    public InventoryItemData gloomStalk;

    public MeshRenderer brokenBox;

    public Material clearM, brokenM, veryBrokenM;

    public List<RepairItem> repairItems;

    public Transform mount;
    [HideInInspector] public bool catOnStruct;

    [System.Serializable]
    public class RepairItem
    {
        public InventoryItemData item;
        public int repairAmount;
    }


    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        UpdateModel();
        OnDamage += UpdateModel;
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
                return;
            }
        }

        /*if(item == gloomStalk && health < maxHealth)
        {
            if(maxHealth <= 40) health = maxHealth;
            else health += maxHealth/3;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            UpdateModel();
            return;
        }*/
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

    void UpdateModel()
    {
        if(health > (maxHealth/3) * 2)
        {
            brokenBox.material = clearM;
        }
        else if(health > maxHealth/3)
        {
            brokenBox.material = brokenM;
        }
        else
        {
            brokenBox.material = veryBrokenM;
        }
    }

    void OnDestroy()
    {
        OnDamage -= UpdateModel;
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
    }

    public override void LoadVariables()
    {
        //UpdateModel();
    }
}
