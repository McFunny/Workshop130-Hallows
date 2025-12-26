using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Torch")]
public class TorchBehavior : ToolBehavior
{
    public InventoryItemData thisItem;
    FireFearTrigger fireScript;
    public AudioClip ignite, extinguish;
    public GameObject placedPrefab;

    Coroutine torchingCoroutine;
    Coroutine chargingCoroutine;

    ParticleSystem flameThrowerParticles;

    bool cancelledEarly = false;

    public bool isUpgrade;

    List<CreatureBehaviorScript> hitCreatures = new List<CreatureBehaviorScript>();
    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();

        //Debug.Log("Used");
        if(isUpgrade && PlayerInteraction.Instance.torchLit)
        {
            flameThrowerParticles = HandItemManager.Instance.flameThrowerParticles;
            if(flameThrowerParticles) flameThrowerParticles.Stop();
            BeginCharge();
            return;
        }
        
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null && structure.Interactable())
            {
                //torch the thing
                bool playAnim = false;

                if(structure.IsFlammable() && !structure.onFire && PlayerInteraction.Instance.torchLit)
                {
                    structure.LitOnFire();
                    playAnim = true;
                }
                else structure.ToolInteraction(tool, out playAnim);

                if(playAnim)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    if(structure.focalPoint != null ) PlayerCam.Instance.NewObjectOfInterest(structure.focalPoint.position);
                    return;
                } 
            }

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                if(interactSuccessful)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
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
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 

                if(PlayerInteraction.Instance.torchLit && enemy.canCorpseBreak && enemy.health <= 0 && enemy.fireVulnerable && !StatusEffectManager.Instance.FindStatusOnCreature(StatusEffectName.Fire, enemy))
                {
                    enemy.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), 20);

                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                }
            }
        }

        //For placing it down
        if (Physics.Raycast(player.position, fwd, out hit, 6, 1 << 7))
        {
            //place it on the ground
            Vector3 pos = StructureManager.Instance.CheckTile(hit.point);
            if(pos != new Vector3(0,0,0)) 
            {
                GameObject newStruct = StructureManager.Instance.SpawnStructureWithInstance(placedPrefab, pos);
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                HotbarDisplay.currentSlot.UpdateUISlot();
                HandItemManager.Instance.ClearHandModel();
            }
        } 
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();

        //Debug.Log("Used");

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null && structure.Interactable())
            {
                //torch the thing
                bool playAnim = false;

                if(structure.IsFlammable() && !structure.onFire && PlayerInteraction.Instance.torchLit)
                {
                    structure.LitOnFire();
                    playAnim = true;
                }
                else structure.ToolInteraction(tool, out playAnim);

                if(playAnim)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    if(structure.focalPoint != null ) PlayerCam.Instance.NewObjectOfInterest(structure.focalPoint.position);
                    return;
                } 
            }

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                if(interactSuccessful)
                {
                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
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
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 

                if(PlayerInteraction.Instance.torchLit && enemy.canCorpseBreak && enemy.health <= 0 && enemy.fireVulnerable && !StatusEffectManager.Instance.FindStatusOnCreature(StatusEffectName.Fire, enemy))
                {
                    enemy.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), 70);

                    HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 1f));
                    PlayerMovement.restrictMovementTokens++;
                    //PlayerInteraction.Instance.StaminaChange(-2);
                    usingPrimary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                }
            }
        }
        
        //For placing it down
        if (Physics.Raycast(player.position, fwd, out hit, 6, 1 << 7))
        {
            //place it on the ground
            Vector3 pos = StructureManager.Instance.CheckTile(hit.point);
            if(pos != new Vector3(0,0,0)) 
            {
                GameObject newStruct = StructureManager.Instance.SpawnStructureWithInstance(placedPrefab, pos);
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                HotbarDisplay.currentSlot.UpdateUISlot();
                HandItemManager.Instance.ClearHandModel();
            }
        } 
    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            usingPrimary = false;
            PlayerMovement.restrictMovementTokens--;
            PlayerCam.Instance.ClearObjectOfInterest();
        }
        if (usingSecondary)
        {
            usingSecondary = false;
            PlayerMovement.restrictMovementTokens--;
            PlayerCam.Instance.ClearObjectOfInterest();
        }

    }

    void BeginCharge()
    {
        if(torchingCoroutine == null) 
        {
            torchingCoroutine = HandItemManager.Instance.StartCoroutine(FlameThrower());
            chargingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeTimer());
        }
    }

    IEnumerator FlameThrower()
    {
        yield return new WaitForSeconds(0.01f);
        PlayerInteraction.Instance.ToolUseToggle(true);
        yield return new WaitUntil(() => !InputManager.isCharging);
        /*while(InputManager.isCharging)
        {
            yield return new WaitForSeconds(0.01f);
            if(holdingPour && PlayerInteraction.Instance.waterHeld > 0) QuickPour();
        }*/
        if(flameThrowerParticles) flameThrowerParticles.Stop();
        if(HandItemManager.Instance.torchSource) HandItemManager.Instance.torchSource.Stop();

        HandItemManager.Instance.StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
        toolAnim.SetBool("IsCharging", false);
        torchingCoroutine = null;
        PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);

        if(cancelledEarly) //Default behavior
        {
            PlayerInteraction.Instance.ToolUseToggle(false);
            SecondaryUse(player, tool);
            yield break;
        }

        yield return new WaitForSeconds(0.4f);
        if(flameThrowerParticles) flameThrowerParticles.Stop();
        PlayerInteraction.Instance.ToolUseToggle(false);

    }

    IEnumerator ChargeTimer()
    {
        //Play the anim and particles
        toolAnim.SetBool("IsCharging", true);
        cancelledEarly = true;
        yield return new WaitForSeconds(0.4f);
        cancelledEarly = false;
        HandItemManager.Instance.StartCoroutine(QuickIgniteRoutine());
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(PlayerInteraction.Instance.gameObject, 0.8f, "Torch", false));

        if(flameThrowerParticles) flameThrowerParticles.Play();
        if(HandItemManager.Instance.torchSource) HandItemManager.Instance.torchSource.Play();
        HandItemManager.Instance.toolSource.PlayOneShot(ignite);

        float burnTimeLeft = Random.Range(1f, 3.5f);
        float timeSpent = 0;
        while(InputManager.isCharging && PlayerInteraction.Instance.stamina > 0 && PlayerInteraction.Instance.torchLit)
        {
            yield return new WaitForSeconds(0.1f);
            timeSpent += .1f;
            if(timeSpent >= burnTimeLeft) HandItemManager.Instance.TorchFlameToggle(false);

            //Ensure particles and code are being run only when the player is looking down
        }
        if(flameThrowerParticles) flameThrowerParticles.Stop();
        if(HandItemManager.Instance.torchSource) HandItemManager.Instance.torchSource.Stop();
    }

    IEnumerator QuickIgniteRoutine()
    {
        float timeSpent = 0;
        int range = 6;
        while(InputManager.isCharging && PlayerInteraction.Instance.torchLit)
        {
            Vector3 fwd = player.TransformDirection(Vector3.forward);
            RaycastHit hit;
            if (Physics.Raycast(player.position, fwd, out hit, range, mask))
            {
                var structure = hit.collider.GetComponent<StructureBehaviorScript>();
                if (structure != null)
                {
                    if(structure.IsFlammable() && !structure.onFire && PlayerInteraction.Instance.torchLit)
                    {
                        structure.LitOnFire();
                    }
                    else if(PlayerInteraction.Instance.torchLit && !structure.IsFlammable()) structure.ToolInteraction(tool, out bool playAnim);
                }

                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                }
            } 

            if (Physics.Raycast(player.position, fwd, out hit, range, 1 << 9))
            {
                var enemy = hit.collider.GetComponentInParent<CreatureBehaviorScript>();
                if (enemy != null && !hitCreatures.Contains(enemy))
                {
                    if((enemy.fireVulnerable || (enemy.canCorpseBreak && enemy.health <= 0)))
                    {
                        int burnDuration = Random.Range(4, 8);
                        if(enemy.health <= 0) burnDuration += 20;
                        enemy.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), burnDuration);
                        enemy.TakeDamage(5);
                        enemy.PlayHitParticle(enemy.corpseParticleTransform.position);
                    }

                    else if(PlayerInteraction.Instance.torchLit) enemy.ToolInteraction(tool, out bool success);
                    hitCreatures.Add(enemy);
                }
            }
            yield return new WaitForSeconds(0.1f);
            timeSpent++;
            if(timeSpent >= 5)
            {
                timeSpent = 0;
                hitCreatures.Clear();
            }
        }
    }


}
