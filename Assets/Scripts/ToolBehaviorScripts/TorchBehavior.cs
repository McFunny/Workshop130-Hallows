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
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;

        //Debug.Log("Used");
        
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                //torch the thing
                bool playAnim = false;

                if(structure.IsFlammable() && !structure.onFire && PlayerInteraction.Instance.torchLit)
                {
                    structure.LitOnFire();
                    playAnim = true;
                }
                else structure.ToolInteraction(tool, out playAnim);

                if(playAnim)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
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
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                }

            }
        } 

        if (Physics.Raycast(player.position, fwd, out hit, 8, 1 << 9))
        {
            var enemy = hit.collider.GetComponentInParent<CreatureBehaviorScript>();
            if (enemy != null)
            {
                enemy.ToolInteraction(tool, out bool success);
                if(success)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
            }
        }
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;

        //Debug.Log("Used");
        
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                //torch the thing
                bool playAnim = false;

                if(structure.IsFlammable() && !structure.onFire && PlayerInteraction.Instance.torchLit)
                {
                    structure.LitOnFire();
                    playAnim = true;
                }
                else structure.ToolInteraction(tool, out playAnim);

                if(playAnim)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingSecondary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
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
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingSecondary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                }

            }
        } 

        if (Physics.Raycast(player.position, fwd, out hit, 8, 1 << 9))
        {
            var enemy = hit.collider.GetComponentInParent<CreatureBehaviorScript>();
            if (enemy != null)
            {
                enemy.ToolInteraction(tool, out bool success);
                if(success)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingSecondary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
            }
        }

        

    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            usingPrimary = false;
            PlayerMovement.restrictMovementTokens--;
            PlayerCam.Instance.ClearObjectOfInterest();
        }
        if (usingSecondary)
        {
            usingSecondary = false;
            PlayerMovement.restrictMovementTokens--;
            PlayerCam.Instance.ClearObjectOfInterest();
        }

    }


}
