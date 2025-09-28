using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Hydrofly")]
public class HydroflyToolBehavior : ToolBehavior
{
    public InventoryItemData thisItem;
    public GameObject thrownPrefab;

    float speed = 100;


    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || TownGate.Instance.location == PlayerLocation.InTown) return;
        if (!player) player = _player;
        
        tool = _tool;
        usingPrimary = true;
        //Shoot
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.1f, 0.5f));
    }
    
    
    public override void SecondaryUse(Transform _player, ToolType _tool) //To Interact with the bug
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {
                bool playAnim = false;

                structure.ToolInteraction(tool, out playAnim);

                if(!playAnim) structure.ItemInteraction(thisItem);
            }

            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);

            }
        } 
    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            usingPrimary = false;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            ThrowBug();
        }
    }

    void ThrowBug()
    {
        Transform bulletStart = HandItemManager.Instance.bulletStart;

        GameObject newBug = ProjectilePoolManager.Instance.GrabHydroflyBullet();
        newBug.transform.position = bulletStart.position;
        newBug.transform.rotation = Quaternion.identity;
        Vector3 dir = bulletStart.forward;
        Rigidbody rb = newBug.GetComponent<Rigidbody>();
        rb.AddForce(dir * speed);
        rb.AddForce(Vector3.up * 50);
    }
}
