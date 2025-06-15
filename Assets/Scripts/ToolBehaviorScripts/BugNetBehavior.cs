using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/BugNet")]
public class BugNetBehavior : ToolBehavior
{
    public InventoryItemData thisItem;
    BugNetSwing netAttack;
    public AudioClip swing;
    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        if(!netAttack) netAttack = FindObjectOfType<BugNetSwing>();
        usingPrimary = true;
        
        //swing
        HandItemManager.Instance.PlayPrimaryAnimation();
        HandItemManager.Instance.toolSource.PlayOneShot(swing);

        float coolDownMod = 1; //Multiplied to the tool use cooldown
        float animSpeedMod = 0; //Added to animation speed

        if(StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.Dare))
        {
            coolDownMod -= .35f;
            animSpeedMod += .7f;
        }
        /*else if(PlayerInteraction.Instance.stamina <= 50)
        {
            coolDownMod += .25f;
            animSpeedMod -= .3f;
        }*/

        toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, 3.6f * coolDownMod));

        //PlayerMovement.limitMaxVelocity = false;
        //PlayerInteraction.Instance.GetComponent<PlayerMovement>().ApplyForceToPlayer(40, PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward));
    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            usingPrimary = false;
            NetSwing();
        }

    }

    void NetSwing()
    {
        if(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == null || HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData != thisItem) return;
        netAttack.StartCoroutine(netAttack.Swing());

    }
}
