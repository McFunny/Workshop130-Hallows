using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/CritterItem")]
public class CritterItemBehavior : ItemBehavior
{
    public override void OnRecieve(InventoryItemData recievedItem)
    {
        //PlayerInteraction.Instance.playerUpgrades.GainInventoryUpgrade();
        CritterItem c = recievedItem as CritterItem;
        if(c)
        {
            if(c.petOverride)
            {
                GameSaveData.Instance.mm_soldPet = true;
                switch(c.petType)
                {
                    case PetType.Cat:
                    GameSaveData.Instance.catRef.gameObject.SetActive(true);
                    break;

                    case PetType.Grub:
                    GameSaveData.Instance.grubRef.gameObject.SetActive(true);
                    break;

                    case PetType.Dog:
                    GameSaveData.Instance.dogRef.gameObject.SetActive(true);
                    break;
                }
            }
            else
            {
                Instantiate(c.critterRef.critterData.objectPrefab, BarnManager.Instance.barnSource.position, Quaternion.identity);
            }
        }
    }
}
