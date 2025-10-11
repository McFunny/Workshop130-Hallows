using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Kukri")]
public class KukriBehavior : ToolBehavior
{
    //public InventoryItemData thisItem;
    KukriAttack kukriAttack;
    public AudioClip swing, throwSFX, chargeReady;

    float coolDownMod = 1; //Multiplied to the tool use cooldown
    float animSpeedMod = 0; //Added to animation speed

    bool maxCharge = false;
    Coroutine throwingCoroutine;
    Coroutine chargingCoroutine;

    Coroutine swingCoroutine;

    public GameObject knifeProjectile;
    bool canSwing = true;
    int swings = 0;
    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary /*|| PlayerInteraction.Instance.toolCooldown*/) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        if(!kukriAttack) kukriAttack = FindObjectOfType<KukriAttack>();
        //usingPrimary = true;

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


        //HandItemManager.Instance.toolSource.PlayOneShot(swing);
        KnifeAttack(); //call the swing attack
        //PlayerInteraction.Instance.ToolUseToggle(true);
        //PlayerInteraction.Instance.ToolUseToggle(false);
        //usingPrimary = false;
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
        ItemUsed();

    }

    public override void ItemUsed()
    {
        if (usingSecondary)
        {
            toolAnim.Play("knifethrow");
            if(throwingCoroutine == null) 
            {
                throwingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeThrow());
                chargingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeTimer());
            }
        }

    }

    void KnifeAttack()
    {
        if(!canSwing) return;
        switch(swings)
        {
            case 0:
            if(swingCoroutine != null) HandItemManager.Instance.StopCoroutine(swingCoroutine);
            swingCoroutine = HandItemManager.Instance.StartCoroutine(SwingTiming(0.15f, 0.15f, 0.3f));
            toolAnim.SetTrigger("Attack");
            break;

            case 1:
            if(swingCoroutine != null) HandItemManager.Instance.StopCoroutine(swingCoroutine);
            swingCoroutine = HandItemManager.Instance.StartCoroutine(SwingTiming(0.35f, 0.2f, 0.3f));
            toolAnim.SetTrigger("Attack");
            break;

            case 2:
            if(swingCoroutine != null) HandItemManager.Instance.StopCoroutine(swingCoroutine);
            swingCoroutine = HandItemManager.Instance.StartCoroutine(SwingTiming(0.35f, 0.0f, 0.4f));
            toolAnim.SetTrigger("Attack");
            break;

            default:
            if(!PlayerInteraction.Instance.toolCooldown) swings = 0;
            break;
        }
    }

    IEnumerator SwingTiming(float attackBuffer, float swingCooldown, float swingWindow)
    {
        canSwing = false;
        swings++;
        PlayerInteraction.Instance.ToolUseToggle(true);
        yield return new WaitForSeconds(attackBuffer); //Time before the knife connects with the target
        HandItemManager.Instance.toolSource.PlayOneShot(swing);
        //Attack the thing in front
        kukriAttack.StartCoroutine(kukriAttack.Swing(swings));
        yield return new WaitForSeconds(swingCooldown); //Opening for the player to attack again
        if(swings < 3) canSwing = true;
        yield return new WaitForSeconds(swingWindow);
        canSwing = false;
        yield return new WaitForSeconds(0.4f); //Player missed the swing window, so now the knife is returning to idle
        PlayerInteraction.Instance.ToolUseToggle(false);
        swings = 0;
        canSwing = true;
    }


    ///////////Charge Functions////////////////

    IEnumerator ChargeThrow()
    {
        yield return new WaitForSeconds(0.01f);
        PlayerInteraction.Instance.ToolUseToggle(true);
        yield return new WaitUntil(() => !InputManager.isCharging);
        //Debug.Log("Throw");
        //yield return new WaitForSeconds(0.01f);
        HandItemManager.Instance.StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
        toolAnim.Play("knifeidle");
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
        yield return new WaitForSeconds(0.02f);
        ThrowKnife();
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
        HandItemManager.Instance.toolSource.PlayOneShot(throwSFX);

        Ray camRay = PlayerInteraction.Instance.mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 origin = camRay.origin;
        Vector3 dir = camRay.direction;

        GameObject newBullet = Instantiate(knifeProjectile, origin, Quaternion.identity);
        newBullet.transform.rotation = PlayerMovement.Instance.orientation.rotation;

        dir = dir + new Vector3(Random.Range(-0.02f,+0.02f), Random.Range(-0.02f,0.02f), Random.Range(-0.02f,0.02f));
        newBullet.GetComponent<Rigidbody>().AddForce(dir * 180);
        newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 30);

        HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
        PlayerInventoryHolder.Instance.UpdateInventory();
        PlayerInteraction.Instance.droppedKukri = true;
    }

}
