using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : StructureBehaviorScript
{
    public InventoryItemData recoveredItem, gloomStalk;

    public MeshRenderer brokenBox;

    public Material clearM, brokenM, veryBrokenM;


    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        UpdateModel();
        OnDamage += UpdateModel;
    }

    void Update()
    {
        base.Update();
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item == gloomStalk && health < maxHealth)
        {
            health += maxHealth/3;
            if(health > maxHealth) health = maxHealth;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            UpdateModel();
            return;
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            StartCoroutine(DugUp());
            success = true;
        }
    }

    IEnumerator DugUp()
    {
        yield return  new WaitForSeconds(1);
        if(health > (maxHealth/3) * 2)
        {
            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
            droppedItem.transform.position = transform.position;
        }
        Destroy(this.gameObject);
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
        if (!gameObject.scene.isLoaded) return; 
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        base.OnDestroy();
    }
}
