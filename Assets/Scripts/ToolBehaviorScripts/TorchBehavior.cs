using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Torch")]
public class TorchBehavior : ToolBehavior
{
    public InventoryItemData thisItem;
    FireFearTrigger fireScript;
    public AudioClip ignite, extinguish;
    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || PlayerInteraction.Instance.stamina < 5) return;
        if (!player) player = _player;
        tool = _tool;
        
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                //torch the thing
                bool playAnim = false;

                if(structure.flammable && !structure.onFire)
                {
                    structure.LitOnFire();
                    playAnim = true;
                }
                else structure.ToolInteraction(tool, out playAnim);

                if(playAnim)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 0.2f));
                    PlayerMovement.restrictMovementTokens++;
                    PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    return;
                } 
            }

            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                if(interactSuccessful)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 0.2f));
                    PlayerMovement.restrictMovementTokens++;
                    PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                }

            }
        } 
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || PlayerInteraction.Instance.stamina < 5) return;
        if (!player) player = _player;
        tool = _tool;

        

    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            usingPrimary = false;
            PlayerMovement.restrictMovementTokens--;
        }
        if (usingSecondary)
        {
            usingSecondary = false;
            //PlayerMovement.restrictMovementTokens--;
        }

    }


}
