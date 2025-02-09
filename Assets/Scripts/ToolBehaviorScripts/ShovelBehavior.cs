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
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        if(!shovelAttack) shovelAttack = FindObjectOfType<ShovelAttack>();
        usingPrimary = true;
        
        //swing
        HandItemManager.Instance.PlayPrimaryAnimation();
        HandItemManager.Instance.toolSource.PlayOneShot(swing);
        PopupEvents.current.ShovelSwing();
        if(PlayerInteraction.Instance.stamina > 25)
        {
            toolAnim.SetFloat("AnimSpeed", 1f);
            PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.55f, 0.9f));
        }
        else
        {
            toolAnim.SetFloat("AnimSpeed", 0.75f);
            PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.55f * 1.25f, 0.9f * 1.25f));
        }
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();

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
                    if(PlayerInteraction.Instance.stamina > 25)
                    {
                        toolAnim.SetFloat("AnimSpeed", 1f);
                        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 1f, 1.9f));
                        PlayerInteraction.Instance.StaminaChange(-2);
                    }
                    else
                    {
                        toolAnim.SetFloat("AnimSpeed", 0.75f);
                        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 1f * 1.25f, 1.9f * 1.25f));
                    }
                    PlayerMovement.restrictMovementTokens++;
                    PlayerCam.Instance.NewObjectOfInterest(structure.transform.position);

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
            PlayerCam.Instance.ClearObjectOfInterest();
        }

    }

    void ShovelSwing()
    {
        if(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == null || HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData != thisItem) return;
        shovelAttack.StartCoroutine(shovelAttack.Swing());
    }



}
