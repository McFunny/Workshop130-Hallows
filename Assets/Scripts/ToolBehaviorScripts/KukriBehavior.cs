using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Kukri")]
public class KukriBehavior : ToolBehavior
{
    //public InventoryItemData thisItem;
    //ShovelAttack shovelAttack;
    public AudioClip swing, throwSFX, chargeReady;

    float coolDownMod = 1; //Multiplied to the tool use cooldown
    float animSpeedMod = 0; //Added to animation speed

    bool maxCharge = false;
    Coroutine throwingCoroutine;
    Coroutine chargingCoroutine;
    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        //if(!shovelAttack) shovelAttack = FindObjectOfType<ShovelAttack>();
        usingPrimary = true;

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

        toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);


        HandItemManager.Instance.toolSource.PlayOneShot(swing);
        //KnifeAttack(); //call the swing attack
        //PlayerInteraction.Instance.ToolUseToggle(true);
        //PlayerInteraction.Instance.ToolUseToggle(false);
        usingPrimary = false;
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        usingSecondary = true;

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

        toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
        //PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.42f * coolDownMod, 0.9f * coolDownMod)); //Disable charge and enable this for old behavior

        toolAnim.SetBool("IsCharging", true);
        //PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.0f, 0.0f)); //For the charge
        ItemUsed();

    }

    public override void ItemUsed()
    {
        if (usingSecondary)
        {
            if(throwingCoroutine == null) 
            {
                throwingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeThrow());
                chargingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeTimer());
            }
        }

    }


    ///////////Charge Functions////////////////

    IEnumerator ChargeThrow()
    {
        yield return new WaitForSeconds(0.01f);
        PlayerInteraction.Instance.ToolUseToggle(true);
        yield return new WaitUntil(() => !InputManager.isCharging);
        //yield return new WaitForSeconds(0.01f);
        HandItemManager.Instance.StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
        toolAnim.SetBool("IsCharging", false);
        throwingCoroutine = null;

        HandItemManager.Instance.toolSource.PlayOneShot(swing);

        if(!maxCharge)
        {
            yield return new WaitForSeconds(0.2f);
            PlayerInteraction.Instance.ToolUseToggle(false);
            usingSecondary = false;
            PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);
            yield break;
        }
        PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);
        //ThrowKnife();
        yield return new WaitForSeconds((0.45f * coolDownMod));

        PlayerInteraction.Instance.ToolUseToggle(false);
        usingSecondary = false;
    }

    IEnumerator ChargeTimer()
    {
        maxCharge = false;
        yield return new WaitForSeconds(0.1f * coolDownMod);
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(PlayerInteraction.Instance.gameObject, 0.6f));

        yield return new WaitForSeconds(0.6f * coolDownMod);
        if(InputManager.isCharging)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(chargeReady);
            maxCharge = true;
            Debug.Log("Charged Up");
        }
    }

    void ThrowKnife()
    {
        //remove the item, tick the thrown bool, then throwSFX it
    }

}
