using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/WaterCan")]
public class WaterCanBehavior : ToolBehavior
{
    public AudioClip refill, pour;
    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        //water
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {
                //play water anim
                bool playAnim = false;
                if(structure.onFire && PlayerInteraction.Instance.waterHeld > 0  && structure.GetComponent<FarmLand>() == null)
                {
                    playAnim = true;
                    structure.Extinguish();
                    PlayerInteraction.Instance.waterHeld--;
                    //return;
                }
                else structure.ToolInteraction(tool, out playAnim);
                if(playAnim)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(pour);
                    PlayerMovement.restrictMovementTokens++;

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
                    if(PlayerInteraction.Instance.stamina > 5f) PlayerInteraction.Instance.StaminaChange(-2);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, .9f * coolDownMod));

                    /*if(PlayerInteraction.Instance.stamina > 50)
                    {
                        toolAnim.SetFloat("AnimSpeed", 1f);
                        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.8f, 1.0f));
                        PlayerInteraction.Instance.StaminaChange(-2);
                    }
                    else
                    {
                        toolAnim.SetFloat("AnimSpeed", 0.75f);
                        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.8f * 1.25f, 1.0f * 1.25f));
                    }*/
                    if(structure.focalPoint != null ) PlayerCam.Instance.NewObjectOfInterest(structure.focalPoint.position);
                    else PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
            }

            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                if(interactSuccessful)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(pour);
                    PlayerMovement.restrictMovementTokens++;

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
                    if(PlayerInteraction.Instance.stamina > 5f) PlayerInteraction.Instance.StaminaChange(-2);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, .9f * coolDownMod));

                    interactable.ReturnFocalPoint(out Transform focalPoint);
                    PlayerCam.Instance.NewObjectOfInterest(focalPoint.position);
                    return;
                }

            }
        }
        if (Physics.Raycast(player.position, fwd, out hit, 8, 1 << 9))
        {
            var enemy = hit.collider.GetComponentInParent<CreatureBehaviorScript>();
            if (enemy != null)
            {
                enemy.ToolInteraction(tool, out bool success);
                if(success)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(pour);
                    PlayerMovement.restrictMovementTokens++;
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
                    if(PlayerInteraction.Instance.stamina > 5f) PlayerInteraction.Instance.StaminaChange(-2);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, .9f * coolDownMod));
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
            }
        }
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || PlayerInteraction.Instance.stamina < 5f) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        //water
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {
                //play water anim
                bool playAnim = false;
                if(structure.onFire && PlayerInteraction.Instance.waterHeld > 0 && structure.GetComponent<FarmLand>() == null)
                {
                    playAnim = true;
                    structure.Extinguish();
                    PlayerInteraction.Instance.waterHeld--;
                }
                else if(structure.GetComponent<WaterBarrel>())
                {
                    structure.GetComponent<WaterBarrel>().ManualFill(out playAnim);
                }
                else if(structure.GetComponent<BirdBath>())
                {
                    structure.GetComponent<BirdBath>().ManualFill(out playAnim);
                }
                else structure.ToolInteraction(tool, out playAnim);

                if(playAnim)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(pour);
                    PlayerMovement.restrictMovementTokens++;

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
                    if(PlayerInteraction.Instance.stamina > 5f) PlayerInteraction.Instance.StaminaChange(-2);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, .9f * coolDownMod));
                    if(structure.focalPoint != null ) PlayerCam.Instance.NewObjectOfInterest(structure.focalPoint.position);
                    else PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
            }

            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                if(interactSuccessful)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(pour);
                    PlayerMovement.restrictMovementTokens++;
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
                    if(PlayerInteraction.Instance.stamina > 5f) PlayerInteraction.Instance.StaminaChange(-2);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, .9f * coolDownMod));
                    interactable.ReturnFocalPoint(out Transform focalPoint);
                    PlayerCam.Instance.NewObjectOfInterest(focalPoint.position);
                    return;
                }

            }
        }
        if (Physics.Raycast(player.position, fwd, out hit, 8, 1 << 9))
        {
            var enemy = hit.collider.GetComponentInParent<CreatureBehaviorScript>();
            if (enemy != null)
            {
                enemy.ToolInteraction(tool, out bool success);
                if(success)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(pour);
                    PlayerMovement.restrictMovementTokens++;
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
                    if(PlayerInteraction.Instance.stamina > 5f) PlayerInteraction.Instance.StaminaChange(-2);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, .9f * coolDownMod));
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
            }
        }
    }

    public override void ItemUsed() 
    { 
        PlayerMovement.restrictMovementTokens--;
        PlayerCam.Instance.ClearObjectOfInterest();
    }


}
