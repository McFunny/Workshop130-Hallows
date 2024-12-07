using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Shovel")]
public class ShovelBehavior : ToolBehavior
{
    public InventoryItemData thisItem;
    ShovelAttack shovelAttack;
    public AudioClip swing, dig;
    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || PlayerInteraction.Instance.stamina < 5) return;
        if (!player) player = _player;
        tool = _tool;
        if(!shovelAttack) shovelAttack = FindObjectOfType<ShovelAttack>();
        usingPrimary = true;
        //swing
        HandItemManager.Instance.PlayPrimaryAnimation();
        HandItemManager.Instance.toolSource.PlayOneShot(swing);
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.55f, 1.2f));
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || PlayerInteraction.Instance.stamina < 5) return;
        if (!player) player = _player;
        tool = _tool;

        Debug.Log("Secondary");

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 7, mask))
        {
            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {
                //play dig anim
                bool playAnim = false;
                structure.ToolInteraction(tool, out playAnim);
                if (playAnim)
                {
                    Debug.Log("Found");
                    usingSecondary = true;
                    HandItemManager.Instance.PlaySecondaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(dig);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 1f, 1.4f));
                    PlayerMovement.restrictMovementTokens++;
                    PlayerInteraction.Instance.StaminaChange(-2);

                }
            }
        }

    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            usingPrimary = false;
            ShovelSwing();
        }
        if (usingSecondary)
        {
            usingSecondary = false;
            PlayerMovement.restrictMovementTokens--;
        }

    }

    void ShovelSwing()
    {
        if(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == null || HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData != thisItem) return;
        shovelAttack.StartCoroutine(shovelAttack.Swing());
    }



}
