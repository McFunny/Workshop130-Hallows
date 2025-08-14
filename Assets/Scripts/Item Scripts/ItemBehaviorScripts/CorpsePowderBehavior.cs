using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/CorpsePowder")]
public class CorpsePowderBehavior : ItemBehavior
{
    public GameObject powderPrefab;
    public override void UseItem(out bool consumeItem)
    {
        Vector3 itemPos = PlayerInteraction.Instance.transform.position;
        Instantiate(powderPrefab, itemPos, Quaternion.identity);

        consumeItem = true;
    }
}
