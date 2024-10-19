using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Shovel")]
public class ShovelBehavior : ToolBehavior
{
    public LayerMask mask;
    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        usingPrimary = true;
        //swing
        HandItemManager.Instance.PlayPrimaryAnimation();
        //HandItemManager.Instance.toolSource.PlayOneShot(swing);
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.7f, 1.5f));
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 4, 1 << 6))
        {
            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                //play water anim
                bool playAnim = false;
                structure.ToolInteraction(tool, out playAnim);
                if (playAnim)
                {
                    HandItemManager.Instance.PlaySecondaryAnimation();
                    //HandItemManager.Instance.toolSource.PlayOneShot(dig);

                }
            }
        }
        //usingSecondary = true;
        //dig

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
            ShovelSwing();
        }

    }

    void ShovelSwing()
    {
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 4, mask))
        {
            //DONT MAKE THIS INSTANT, ALIGN IT WITH ANIMATION
            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                structure.health -= 2;
            }

            var creature = hit.collider.GetComponent<CreatureBehaviorScript>();
            if (creature != null)
            {
                creature.TakeDamage(25);
            }
        }
    }

    void ShovelDig()
    {
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 4, 1 << 6))
        {
            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                //play water anim
                bool playAnim = false;
                structure.ToolInteraction(tool, out playAnim);
                if (playAnim)
                {
                    HandItemManager.Instance.PlaySecondaryAnimation();
                    //HandItemManager.Instance.toolSource.PlayOneShot(dig);

                }
            }
        }
    }


}
