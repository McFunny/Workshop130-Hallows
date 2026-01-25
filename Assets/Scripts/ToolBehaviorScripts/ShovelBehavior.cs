using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Shovel")]
public class ShovelBehavior : ToolBehavior
{
    public InventoryItemData thisItem;
    ShovelAttack shovelAttack;
    public AudioClip swing, dig, chargeReady, parrySFX, parrySuccessSFX;
    StructureBehaviorScript interactedStructure;

    float coolDownMod = 1; //Multiplied to the tool use cooldown
    float animSpeedMod = 0; //Added to animation speed

    bool maxCharge = false;
    Coroutine swingingShovelCoroutine;
    Coroutine chargingCoroutine;

    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        if(!shovelAttack) shovelAttack = FindObjectOfType<ShovelAttack>();
        usingPrimary = true;
        
        //swing
        /*HandItemManager.Instance.PlayPrimaryAnimation();
        HandItemManager.Instance.toolSource.PlayOneShot(swing);*/
        PopupEvents.current.ShovelSwing();

        coolDownMod = 1; //Multiplied to the tool use cooldown
        animSpeedMod = 0; //Added to animation speed

        if(StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.Dare))
        {
            coolDownMod -= .35f;
            animSpeedMod += .7f;
        }
        else if(PlayerInteraction.Instance.stamina <= 50)
        {
            coolDownMod += .25f;
            animSpeedMod -= .3f;
        }
        if(PlayerInteraction.Instance.parrySuccess)
        {
            coolDownMod -= .35f;
            animSpeedMod += .7f;
            PlayerInteraction.Instance.parrySuccess = false;
        }

        toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
        //PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.42f * coolDownMod, 0.9f * coolDownMod)); //Disable charge and enable this for old behavior

        toolAnim.SetBool("IsCharging", true);
        //PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.0f, 0.0f)); //For the charge
        ItemUsed();

        //PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.35f * coolDownMod, 0.9f * coolDownMod)); //New anim values

        //PlayerInteraction.Instance.GetComponent<PlayerMovement>().ApplyForceToPlayer(40, PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward));
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 7, mask))
        {
            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null && structure.Interactable())
            {
                //play dig anim
                bool playAnim = false;
                structure.ToolInteraction(tool, out playAnim);
                if (playAnim)
                {
                    interactedStructure = structure;
                    usingSecondary = true;

                    //HandItemManager.Instance.PlaySecondaryAnimation();
                    toolAnim.Play("shoveldig");
                    HandItemManager.Instance.toolSource.PlayOneShot(dig);

                    coolDownMod = 1; //Multiplied to the tool use cooldown
                    animSpeedMod = 0; //Added to animation speed

                    if(StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.Dare))
                    {
                        coolDownMod -= .5f;
                        animSpeedMod += 1f;
                    }
                    else if(PlayerInteraction.Instance.stamina <= 50)
                    {
                        coolDownMod += .25f;
                        animSpeedMod -= .15f;
                    }
                    if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);

                    toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.8f * coolDownMod, 1.9f * coolDownMod));

                    /*
                    if(PlayerInteraction.Instance.stamina > 50)
                    {
                        toolAnim.SetFloat("AnimSpeed", 1f);
                        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 1f, 1.9f));
                        PlayerInteraction.Instance.StaminaChange(-2);
                    }
                    else
                    {
                        toolAnim.SetFloat("AnimSpeed", 0.75f);
                        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 1f * 1.25f, 1.9f * 1.25f));
                    }*/
                    PlayerMovement.restrictMovementTokens++;
                    PlayerCam.Instance.NewObjectOfInterest(structure.transform.position);

                }
                return;
            }
        }

        if(PlayerInteraction.Instance.stamina <= 50) return;
        if(!TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.Parry)) return; //Do the check for the trinket

        PlayerInteraction.Instance.StartCoroutine(ParryRoutine());
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ParryRoutine());

    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            if(swingingShovelCoroutine == null) 
            {
                swingingShovelCoroutine = HandItemManager.Instance.StartCoroutine(SwingShovel());
                chargingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeTimer());
            }
            /*
            usingPrimary = false;
            ShovelSwing();
            */
        }
        if (usingSecondary)
        {
            usingSecondary = false;
            PlayerMovement.restrictMovementTokens--;
            PlayerCam.Instance.ClearObjectOfInterest();
            if(interactedStructure) interactedStructure.DigAction();

            ScreenSplatSpawner.Instance.SpawnSplats(SplatType.Dirt, new Color(1,1,1,0.4f), Random.Range(3, 7));
        }

    }

    void ShovelSwing()
    {
        if(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == null || HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData != thisItem) return;
        shovelAttack.StartCoroutine(shovelAttack.Swing());
        if(maxCharge) shovelAttack.chargedSwing = true;
        else shovelAttack.chargedSwing = false;
        //PlayerMovement.limitMaxVelocity = true;
        //PlayerInteraction.Instance.GetComponent<PlayerMovement>().ApplyForceToPlayer(200, PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward));
    }

    ///////////Shovel Charge Functions////////////////

    IEnumerator SwingShovel()
    {
        yield return new WaitForSeconds(0.01f);
        PlayerInteraction.Instance.ToolUseToggle(true);
        yield return new WaitUntil(() => !InputManager.isCharging);
        //yield return new WaitForSeconds(0.01f);
        HandItemManager.Instance.StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
        toolAnim.SetBool("IsCharging", false);
        swingingShovelCoroutine = null;

        HandItemManager.Instance.toolSource.PlayOneShot(swing);

        float time = 0;
        if(maxCharge)
        {
            time = 0.35f * coolDownMod;
            toolAnim.Play("shovelChargedSwing");
            if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-2);
        }
        else
        {
            time = 0.4f * coolDownMod;
            toolAnim.Play("shovelbonk");
            PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);
        }

        //float time = 0.42f * coolDownMod;
        yield return new WaitForSeconds(time);
        PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);
        ShovelSwing();
        yield return new WaitForSeconds((0.85f * coolDownMod) - time);
        //if(!maxCharge) yield return new WaitForSeconds(0.2f);

        PlayerInteraction.Instance.ToolUseToggle(false);
        usingPrimary = false;
    }

    IEnumerator ChargeTimer()
    {
        maxCharge = false;
        yield return new WaitForSeconds(0.2f * coolDownMod);
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(PlayerInteraction.Instance.gameObject, 0.65f, "ShovelCharge", false));

        yield return new WaitForSeconds(0.6f * coolDownMod);
        if(InputManager.isCharging)
        {
            //HandItemManager.Instance.toolSource.PlayOneShot(chargeReady);
            AudioPoolManager.Instance.PlayClip(chargeReady, 0.4f);
            maxCharge = true;
            Debug.Log("Charged Up");
        }
    }

    IEnumerator ParryRoutine()
    {
        toolAnim.Play("shovelParry");
        AudioPoolManager.Instance.PlayClip(parrySFX, 0.8f);
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(PlayerInteraction.Instance.gameObject, 0.4f, "ShovelParry", false));
        yield return new WaitForSeconds(0.01f);
        while(PlayerInteraction.Instance.isParrying)
        {
            yield return null;
        }

        PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);

        if(PlayerInteraction.Instance.parrySuccess)
        {
            toolAnim.Play("shovelParrySuccess");
            AudioPoolManager.Instance.PlayClip(parrySuccessSFX, 0.8f);
        }

    }



}
