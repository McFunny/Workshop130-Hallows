using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedShooter : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;

    public Transform turretHead, bulletOrigin;

    float maxAmmo = 10; //Dont allow any more seeds to be added to the item list after there are 10 entrants
    float range = 20;

    //Maybe add a large button on the back, where when the player interacts with this, it can be turned on and off

    public CreatureObject[] targettableCreatures; //No crows, no wraiths, no murdermancers

    CreatureBehaviorScript currentTarget;

    //Use DOT products to determine if it is facing it's target, then fire a raycast to check for any obstacles, then fire
    //Maybe add a feature where a seed can plant itself on the tile a target is hit on
    //Give it a chance to misfire, shooting off its course
    //Seeds should deal roughly 20 damage
    //To reduce its power, maybe make it unable to fire over barricades

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }

    void Update()
    {
        base.Update();
    }

    void CheckForTargets()
    {
        //
    }

    IEnumerator TargetRefreshCooldown()
    {
        yield return new WaitForSeconds(1);
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
        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
        droppedItem.transform.position = transform.position;
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop seeds
    }
}
