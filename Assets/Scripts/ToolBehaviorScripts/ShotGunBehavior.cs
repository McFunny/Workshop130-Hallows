using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/ShotGun")]
public class ShotGunBehavior : ToolBehavior
{
    public InventoryItemData bulletItem;
    public GameObject bullet;
    public AudioClip shoot, reload;

    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        usingPrimary = true;
        //Shoot
        //HandItemManager.Instance.PlayPrimaryAnimation();
        //HandItemManager.Instance.toolSource.PlayOneShot(swing);
        //PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.55f, 1.5f));
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        //

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
        }

    }

  
}
