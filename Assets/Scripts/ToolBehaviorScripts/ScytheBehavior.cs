using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Scythe")]
public class ScytheBehavior : ToolBehavior
{
    //
    public InventoryItemData thisItem;
    ScytheAttack scytheAttack;
    public AudioClip swing;

    public bool isUpgrade;
    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        if(!scytheAttack) scytheAttack = FindObjectOfType<ScytheAttack>();
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
        else if(PlayerInteraction.Instance.stamina <= 50)
        {
            coolDownMod += .25f;
            animSpeedMod -= .25f;
        }

        toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUseWithoutMovementReset(this, 0.6f * coolDownMod, 2f * coolDownMod));
        if(HandItemManager.Instance.scytheTrail) HandItemManager.Instance.scytheTrail.emitting = true;
    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            usingPrimary = false;
            ScytheSwing();
        }

    }

    void ScytheSwing()
    {
        if(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == null || HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData != thisItem) return;
        scytheAttack.upgradedSwing = isUpgrade;
        scytheAttack.StartCoroutine(scytheAttack.Swing());
        PlayerMovement.limitMaxVelocity = false;
        PlayerMovement.ignoreMovementInputs = true;
        //if moving backwards/still, do forward. else, do the direction of movement
        Vector2 moveInput = PlayerInteraction.Instance.controlManager.movement.action.ReadValue<Vector2>();
        //For not moving forward
        //if(moveInput.y < 0f || moveInput.x == 0f) PlayerInteraction.Instance.GetComponent<PlayerMovement>().ApplyForceToPlayer(300, PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward));
        //else PlayerInteraction.Instance.GetComponent<PlayerMovement>().ApplyForceToPlayer(600, PlayerInteraction.Instance.mainCam.transform.TransformDirection(moveInput.normalized));

        if((moveInput.y >= 0f && moveInput.x != 0f) || moveInput.y > 0f) 
            PlayerInteraction.Instance.GetComponent<PlayerMovement>().ApplyForceToPlayer(600, PlayerInteraction.Instance.mainCam.transform.TransformDirection(moveInput.normalized));
        else PlayerInteraction.Instance.GetComponent<PlayerMovement>().ApplyForceToPlayer(300, PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward));
    }
}
