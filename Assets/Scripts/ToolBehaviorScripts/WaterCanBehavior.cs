using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/WaterCan")]
public class WaterCanBehavior : ToolBehavior
{
    public AudioClip refill, pour, empty;

    bool holdingPour = false;
    bool skipPour = false;

    bool wateredCreature; //To add a delay to hitting a creature with water
    //bool puttingCanAway = false;
    Coroutine wateringCoroutine;
    Coroutine chargingCoroutine;

    List<StructureBehaviorScript> wateredStructures = new List<StructureBehaviorScript>();

    ParticleSystem pourParticles;

    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        if(!pourParticles) pourParticles = HandItemManager.Instance.waterCanParticles;
        if(pourParticles) pourParticles.Stop();
        //water
        //PrimaryUse();
        BeginCharge();
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
                    HandItemManager.Instance.toolSource.PlayOneShot(refill);
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
                    if(PlayerInteraction.Instance.stamina > 50f) PlayerInteraction.Instance.StaminaChange(-2);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, .9f * coolDownMod));
                    if(structure.focalPoint != null ) PlayerCam.Instance.NewObjectOfInterest(structure.focalPoint.position);
                    else PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
                else HandItemManager.Instance.toolSource.PlayOneShot(empty);
            }

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                if(interactSuccessful)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(pour);
                    HandItemManager.Instance.toolSource.PlayOneShot(refill);
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
                    if(PlayerInteraction.Instance.stamina > 50f) PlayerInteraction.Instance.StaminaChange(-2);

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
                    HandItemManager.Instance.toolSource.PlayOneShot(refill);
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
                    if(PlayerInteraction.Instance.stamina > 50f) PlayerInteraction.Instance.StaminaChange(-2);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, .9f * coolDownMod));
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
            }
        }
    }

    void PrimaryUse() //Behavior as if the player used left click on a structure
    {
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
                else structure.ToolInteraction(tool, out playAnim);
                if(playAnim)
                {
                    //HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(pour);
                    HandItemManager.Instance.toolSource.PlayOneShot(refill);
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
                    if(PlayerInteraction.Instance.stamina > 50f) PlayerInteraction.Instance.StaminaChange(-1);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    toolAnim.Play("wateringcan");
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, .9f * coolDownMod));

                    if(structure.focalPoint != null ) PlayerCam.Instance.NewObjectOfInterest(structure.focalPoint.position);
                    else PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
                else HandItemManager.Instance.toolSource.PlayOneShot(empty);
            }

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                if(interactSuccessful)
                {
                    //HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(pour);
                    HandItemManager.Instance.toolSource.PlayOneShot(refill);
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
                    if(PlayerInteraction.Instance.stamina > 50f) PlayerInteraction.Instance.StaminaChange(-2);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    toolAnim.Play("wateringcan");
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
                    //HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(pour);
                    HandItemManager.Instance.toolSource.PlayOneShot(refill);
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
                    if(PlayerInteraction.Instance.stamina > 50f) PlayerInteraction.Instance.StaminaChange(-2);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    toolAnim.Play("wateringcan");
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.5f * coolDownMod, .9f * coolDownMod));
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
            }
        }

        //if(puttingCanAway)
        //{
            //puttingCanAway = false;
            //PlayerInteraction.Instance.ToolUseToggle(true);
            HandItemManager.Instance.StartCoroutine(ExtraLag());
        //}
    }

    public override void ItemUsed() 
    { 
        PlayerMovement.restrictMovementTokens--;
        PlayerCam.Instance.ClearObjectOfInterest();
    }

    void BeginCharge()
    {
        if(wateringCoroutine == null) 
        {
            wateringCoroutine = HandItemManager.Instance.StartCoroutine(WaterPour());
            chargingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeTimer());
        }
    }

    IEnumerator WaterPour()
    {
        yield return new WaitForSeconds(0.01f);
        PlayerInteraction.Instance.ToolUseToggle(true);
        yield return new WaitUntil(() => !InputManager.isCharging);
        /*while(InputManager.isCharging)
        {
            yield return new WaitForSeconds(0.01f);
            if(holdingPour && PlayerInteraction.Instance.waterHeld > 0) QuickPour();
        }*/
        if(pourParticles) pourParticles.Stop();

        HandItemManager.Instance.StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
        toolAnim.SetBool("IsCharging", false);
        wateringCoroutine = null;
        PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);
        wateredStructures.Clear();

        if(!holdingPour) //Default pour
        {
            //PlayerInteraction.Instance.ToolUseToggle(false);
            PrimaryUse();
            yield break;
        }

        yield return new WaitForSeconds(0.7f);
        if(pourParticles) pourParticles.Stop();
        PlayerInteraction.Instance.ToolUseToggle(false);

    }

    IEnumerator QuickPourRoutine()
    {
        while(InputManager.isCharging && holdingPour)
        {
            if(wateredCreature)
            {
                wateredCreature = false;
                yield return new WaitForSeconds(1.1f);
            }
            if(holdingPour && PlayerInteraction.Instance.waterHeld > 0 && CanPour()) QuickPour();
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ChargeTimer()
    {
        //Play the anim and particles
        toolAnim.SetBool("IsCharging", true);
        holdingPour = false;
        yield return new WaitForSeconds(0.4f);
        holdingPour = true;
        HandItemManager.Instance.StartCoroutine(QuickPourRoutine());
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(PlayerInteraction.Instance.gameObject, 0.8f, "WateringCan", false));

        skipPour = false;
        float timeBetweenPours = 1.5f;
        while(InputManager.isCharging && PlayerInteraction.Instance.stamina > 0 && PlayerInteraction.Instance.waterHeld > 0)
        {
            yield return new WaitForSeconds(timeBetweenPours);
            if(skipPour)
            {
                skipPour = false;
                continue;
            }
            if(CanPour())PlayerInteraction.Instance.waterHeld--;

            //Ensure particles and code are being run only when the player is looking down
        }
        if(pourParticles) pourParticles.Stop();
    }

    bool CanPour() //Checks player eyeline
    {
        Debug.Log(player.eulerAngles.x);
        if((player.eulerAngles.x >= 25 && player.eulerAngles.x <= 90) || player.eulerAngles.x == 0) 
        {
            if(pourParticles) pourParticles.Play();
            return true;
        }
        else 
        {
            if(pourParticles) pourParticles.Stop();
            return false;
        }
    }


    void QuickPour() //Code called when holding the pour
    {
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        bool consumeWater = false;
        if (Physics.Raycast(player.position, fwd, out hit, 6, mask))
        {
            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {
                if(structure.onFire || !wateredStructures.Contains(structure))
                {
                    FarmLand tile = structure as FarmLand;
                    if(tile && !structure.onFire)
                    {
                        wateredStructures.Add(structure);
                        if(tile.GetCropStats().waterLevel == 10) return;
                    }
                    else if(structure as IWaterHolder == null) wateredStructures.Add(structure);
                    structure.HitWithWater();
                    consumeWater = true;
                }
            }
        }
        if (Physics.Raycast(player.position, fwd, out hit, 6, 1 << 9))
        {
            var enemy = hit.collider.GetComponentInParent<CreatureBehaviorScript>();
            if (enemy != null && enemy.health > 0)
            {
                enemy.HitWithWater();
                consumeWater = true;
                wateredCreature = true;
            }
        }
        if(consumeWater)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(pour);
            HandItemManager.Instance.toolSource.PlayOneShot(refill);
            PlayerInteraction.Instance.waterHeld--;
            skipPour = true;
        }
    }

    IEnumerator ExtraLag()
    {
        yield return new WaitForSeconds(0.4f);
        PlayerInteraction.Instance.ToolUseToggle(false);
    }


}
