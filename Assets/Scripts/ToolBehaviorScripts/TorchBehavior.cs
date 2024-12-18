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
        }
        if (usingSecondary)
        {
            usingSecondary = false;
            //PlayerMovement.restrictMovementTokens--;
        }

    }


}
