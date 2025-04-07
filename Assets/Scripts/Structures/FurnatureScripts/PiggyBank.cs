using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PiggyBank : FurnitureBehaviorScript
{
    public int heldMints = 0;
    int maxMints = 1000;

    public TextMeshProUGUI moneyText;

    public ParticleSystem insertParticles;

    public InventoryItemData mints;

    public GameObject destructionParticles;

    void Start()
    {
        moneyText.text = heldMints + "/" + maxMints + "<sprite index=0>";
        OnDamage += Break;
        base.Start();
        FurnitureStart();
    }

    public override void StructureInteraction()
    {
        bool success = false;
        for(int i = 0; i < 50; i++)
        {
            if(PlayerInteraction.Instance.currentMoney > 0 && heldMints < maxMints)
            {
                PlayerInteraction.Instance.currentMoney--;
                heldMints++;
                success = true;
            }
        }

        if(!success) return;

        insertParticles.Play();

        moneyText.text = heldMints + "/" + maxMints + "<sprite index=0>";
        
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false && heldMints == 0)
        {
            StartCoroutine(DugUp());
            success = true;
        }
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);

        Destroy(this.gameObject);
    }

    void Break()
    {
        //drop mints and destroy self
        if(heldMints > 0)
        {
            mints.maxStackSize = heldMints;
            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(mints);
            droppedItem.transform.position = transform.position;
        }
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        OnDamage -= Break;
        Instantiate(destructionParticles, particleCenter.position, Quaternion.identity);
        base.OnDestroy();
    }

    public override void SaveVariables()
    {
        saveInt1 = heldMints;
    }

    public override void LoadVariables()
    {
        heldMints = saveInt1;
        moneyText.text = heldMints + "/" + maxMints + "<sprite index=0>";
    }
}
