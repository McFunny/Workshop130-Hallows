using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Hoe")]
public class HoeBehavior : ToolBehavior
{
    Vector3 pos;
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
        StructureManager structManager = FindObjectOfType<StructureManager>();

        if(Physics.Raycast(player.position, fwd, out hit, 5, 1 << 7))
        {
            pos = StructureManager.Instance.CheckTile(hit.point);
            if(pos != new Vector3(0,0,0)) 
            {
                //usingPrimary = true;
                HandItemManager.Instance.PlayPrimaryAnimation();
                //HandItemManager.Instance.toolSource.PlayOneShot(swing);
                PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 1.5f, 0f));
                PlayerMovement.restrictMovementTokens++;
                //HAVE PLAYER NOT BE ABLE TO TURN
            }

        }
    }

    public override void SecondaryUse(Transform player, ToolType _tool)
    {
        //swing?
    }

    public override void ItemUsed() 
    { 
        usingPrimary = false;
        PlayerMovement.restrictMovementTokens--;
        StructureManager.Instance.SpawnStructure(farmTile, pos);
    }


}
