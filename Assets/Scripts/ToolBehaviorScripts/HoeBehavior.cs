using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Hoe")]
public class HoeBehavior : ToolBehavior
{
    Vector3 pos;
    UntilledTile tile;
    public GameObject farmTile;
    public AudioClip swing;

    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || PlayerInteraction.Instance.stamina < 5)
        {
            return;
        } 
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();

        //till ground
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;

        tile = null;

        if(Physics.Raycast(player.position, fwd, out hit, 7, mask))
        {

            //tile = hit.collider.GetComponent<UntilledTile>();
            //if (tile != null)
            //{
                //play hoe anim
            //    HandItemManager.Instance.PlayPrimaryAnimation();
                //HandItemManager.Instance.toolSource.PlayOneShot(swing);
            //    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 1.5f, 0f));
            //    PlayerMovement.restrictMovementTokens++;

            //    return;
            //}


            pos = StructureManager.Instance.CheckTile(hit.point);
            if(pos != new Vector3(0,0,0)) 
            {
                usingPrimary = true;
                HandItemManager.Instance.PlayPrimaryAnimation();
                HandItemManager.Instance.toolSource.PlayOneShot(swing);
                if(PlayerInteraction.Instance.stamina > 25)
                {
                    toolAnim.SetFloat("AnimSpeed", 1f);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.4f, 1.7f));
                    PlayerInteraction.Instance.StaminaChange(-2);
                }
                else
                {
                    toolAnim.SetFloat("AnimSpeed", 0.75f);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.4f * 1.25f, 1.7f * 1.25f));
                }
                PlayerMovement.restrictMovementTokens++;
            }

        }
    }

    public override void SecondaryUse(Transform player, ToolType _tool)
    {
        //swing?
    }

    public override void ItemUsed() 
    { 
        PlayerInteraction.Instance.StartCoroutine(ExtraLag());
        if(tile) tile.ToolInteraction(tool, out bool playAnim);
        else StructureManager.Instance.SpawnStructure(farmTile, pos);
    }

    IEnumerator ExtraLag()
    {
        yield return new WaitForSeconds(1.1f);
        usingPrimary = false;
        PlayerMovement.restrictMovementTokens--;
    }


}
