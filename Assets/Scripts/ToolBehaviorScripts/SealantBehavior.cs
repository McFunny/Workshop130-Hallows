using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Sealant")]
public class SealantBehavior : ToolBehavior
{
    Vector3 pos;
    public GameObject trapPrefab;
    public AudioClip applySFX, placeSFX;
    public int healthRestored = 10;

    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        return; //Trap Unimplemented Yet
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown)
        {
            return;
        } 
        if (!player) player = _player;
        tool = _tool;

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        //For placing it down
        if (Physics.Raycast(player.position, fwd, out hit, 6, 1 << 7))
        {
            //place it on the ground
            Vector3 pos = StructureManager.Instance.CheckTile(hit.point);
            if(pos != new Vector3(0,0,0) && StructureManager.Instance.ValidateGridType(pos, GridType.Farm)) 
            {
                GameObject newStruct = StructureManager.Instance.SpawnStructureWithInstance(trapPrefab, pos);
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                HotbarDisplay.currentSlot.UpdateUISlot();
            }
        } 
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        
        if (/*usingPrimary || usingSecondary ||*/ PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;


        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if(Physics.Raycast(player.position, fwd, out hit, 7f, mask))
        {
            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null && structure.RepairWithSealant(healthRestored))
            {
                //usingSecondary = true;
                HandItemManager.Instance.toolSource.PlayOneShot(placeSFX);

                PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0, 0.2f));

                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();
                return;
            }

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                return;
            }


        }
    }

    public override void ItemUsed() 
    { 
        usingSecondary = false;
    }
}
