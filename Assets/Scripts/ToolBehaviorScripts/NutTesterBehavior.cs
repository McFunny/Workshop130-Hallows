using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/NutTester")]
public class NutTesterBehavior : ToolBehavior
{
    public AudioClip blipSFX;

    bool holding = false;

    Coroutine scanningCoroutine;
    Coroutine chargingCoroutine;

    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        //Maybe give it a use to stun robots/ghosts, or check enemy hp
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        BeginCharge();
    }

    void BeginCharge()
    {
        if(scanningCoroutine == null) 
        {
            scanningCoroutine = HandItemManager.Instance.StartCoroutine(Scan());
            chargingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeTimer());
        }
    }

    IEnumerator Scan()
    {
        yield return new WaitForSeconds(0.01f);
        PlayerInteraction.Instance.ToolUseToggle(true);
        yield return new WaitUntil(() => !InputManager.isCharging);


        HandItemManager.Instance.StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
        toolAnim.Play("MoveToIdle");
        scanningCoroutine = null;
        PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);

        yield return new WaitForSeconds(0.4f);
        PlayerInteraction.Instance.ToolUseToggle(false);

    }

    IEnumerator ChargeTimer()
    {
        //Play the anim and particles
        toolAnim.Play("MoveToUse");
        yield return new WaitForSeconds(0.4f);
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(PlayerInteraction.Instance.gameObject, 0.8f, "NutTester", false));
        float timeBetweenScans = 0.5f;
        while(InputManager.isCharging)
        {
            yield return new WaitForSeconds(timeBetweenScans);
            //Scan Functionality Here: Refresh scan target via casting a ray
            HandItemManager.Instance.toolSource.PlayOneShot(blipSFX);
        }
    }
}
