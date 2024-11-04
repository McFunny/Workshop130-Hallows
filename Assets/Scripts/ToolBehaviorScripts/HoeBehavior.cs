using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Hoe")]
public class HoeBehavior : ToolBehavior
{
    Vector3 pos;
    UntilledTile tile;
    public GameObject farmTile;

    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown)
        {
            return;
        } 
        if (!player) player = _player;
        tool = _tool;

        //till ground
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;

        tile = null;

        if(Physics.Raycast(player.position, fwd, out hit, 5, mask))
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
                //HandItemManager.Instance.toolSource.PlayOneShot(swing);
                PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.4f, 1.9f));
                PlayerMovement.restrictMovementTokens++;
                PlayerInteraction.Instance.StaminaChange(-2);
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
        yield return new WaitForSeconds(1.3f);
        usingPrimary = false;
        PlayerMovement.restrictMovementTokens--;
    }


}
