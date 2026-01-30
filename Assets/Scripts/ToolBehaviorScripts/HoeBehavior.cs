using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Hoe")]
public class HoeBehavior : ToolBehavior
{
    Vector3 pos;
    public GameObject farmTile;
    public AudioClip swing, chargeReady;
    public GameObject placedPrefab;

    float coolDownMod = 1; //Multiplied to the tool use cooldown
    float animSpeedMod = 0; //Added to animation speed

    bool maxCharge = false;
    bool upgradedCharge = false;
    Coroutine swingingHoeCoroutine;
    Coroutine chargingCoroutine;

    public bool isUpgrade;

    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown)
        {
            return;
        } 
        if (!player) player = _player;
        tool = _tool;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();

        //till ground
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if(Physics.Raycast(player.position, fwd, out hit, 7f, mask))
        {

            pos = StructureManager.Instance.CheckTile(hit.point);
            if(pos != new Vector3(0,0,0) && StructureManager.Instance.ValidateGridType(pos, GridType.Farm)) 
            {
                usingPrimary = true;
                //HandItemManager.Instance.PlayPrimaryAnimation();
                //HandItemManager.Instance.toolSource.PlayOneShot(swing);

                coolDownMod = 1; //Multiplied to the tool use cooldown
                animSpeedMod = 0; //Added to animation speed


                if(StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.Dare))
                {
                    coolDownMod -= .35f;
                    animSpeedMod += .5f;
                }
                else if(PlayerInteraction.Instance.stamina <= 50)
                {
                    coolDownMod += .25f;
                    animSpeedMod -= .25f;
                }
 
                toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                toolAnim.SetBool("IsCharging", true);
                BeginHoeCharge();
                return;
                ////

                /*
                if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-2);

                toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
                PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.4f * coolDownMod, 1.1f * coolDownMod));

                
                PlayerMovement.restrictMovementTokens++;
                PlayerCam.Instance.NewObjectOfInterest(pos);
                */
            }

        }
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        //For placing it down
        if (Physics.Raycast(player.position, fwd, out hit, 6, 1 << 7))
        {
            //place it on the ground
            Vector3 pos = StructureManager.Instance.CheckTile(hit.point);
            if(pos != new Vector3(0,0,0) && StructureManager.Instance.ValidateGridType(pos, GridType.Farm)) 
            {
                GameObject newStruct = StructureManager.Instance.SpawnStructureWithInstance(placedPrefab, pos);
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                HotbarDisplay.currentSlot.UpdateUISlot();
                HandItemManager.Instance.ClearHandModel();
                return;
            }
        } 

        if (Physics.Raycast(player.position, fwd, out hit, 6, 1 << 6))
        {
            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null && structure.Interactable())
            {
                //Use Tool to interact with structure (Probably just the tool rack)
                bool success = false;
                structure.ToolInteraction(tool, out success);
                if(success) return;
            }
        }
    }

    public override void ItemUsed() 
    { 
        Debug.Log("Using item");
        PopupEvents.current.TillGround(); // Sends message to the PopupEvents to tell it when to close certain popups
        PlayerInteraction.Instance.StartCoroutine(ExtraLag());
        StructureManager.Instance.SpawnStructure(farmTile, pos);
        if(maxCharge)
        {
            maxCharge = false;

            PlayerInteraction.Instance.StartCoroutine(SpawnTiles());
        }
        PlayerCam.Instance.ClearObjectOfInterest();
        ScreenSplatSpawner.Instance.SpawnSplats(SplatType.Dirt, new Color(1,1,1,0.4f), Random.Range(1, 5));
    }

    IEnumerator SpawnTiles()
    {
        //Vector3 playerPos;
        Vector3 currentPos;
        List<Vector3> targets = new List<Vector3>();
        int targetAmount = 2;
        if(upgradedCharge)
        {
            upgradedCharge = false;
            targetAmount += 2;
        }
        //playerPos = StructureManager.Instance.GetTileCenter(player.position);
        targets = StructureManager.Instance.ShowTargets(pos, StructureManager.Instance.GetDirection(player), targetAmount, false);
        if(targets.Count > 0)
        {
            for(int i = 0; i < targets.Count; i++)
            {
                yield return new WaitForSeconds(0.3f);
                currentPos = StructureManager.Instance.CheckTile(targets[i]);
                if(currentPos != Vector3.zero) StructureManager.Instance.SpawnStructure(farmTile, currentPos);
            }
        }
    }

    IEnumerator ExtraLag()
    {
        yield return new WaitForSeconds(0.8f * coolDownMod);
        usingPrimary = false;
        PlayerMovement.restrictMovementTokens--;
    }

    void BeginHoeCharge()
    {
        if(swingingHoeCoroutine == null) 
        {
            swingingHoeCoroutine = HandItemManager.Instance.StartCoroutine(HoeCharge());
            chargingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeTimer());
        }
    }

    IEnumerator HoeCharge()
    {
        yield return new WaitForSeconds(0.01f);
        PlayerInteraction.Instance.ToolUseToggle(true);
        yield return new WaitUntil(() => !InputManager.isCharging);
        PlayerInteraction.Instance.ToolUseToggle(false);
        toolAnim.SetBool("IsCharging", false);
        //yield return new WaitForSeconds(0.01f);
        HandItemManager.Instance.StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
        swingingHoeCoroutine = null;
        PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);

        //Check if we can still till
        bool success = false;
        RaycastHit hit;
        if(Physics.Raycast(player.position, player.TransformDirection(Vector3.forward), out hit, 7f, mask))
        {
            pos = StructureManager.Instance.CheckTile(hit.point);
            if(pos != new Vector3(0,0,0) && StructureManager.Instance.ValidateGridType(pos, GridType.Farm)) success = true;
        }
        if(!success)
        {
            usingPrimary = false;
            toolAnim.Play("hoeidle");
            yield break;
        }

        
        //HandItemManager.Instance.PlayPrimaryAnimation();
        toolAnim.Play("hoeswinging");
        HandItemManager.Instance.toolSource.PlayOneShot(swing);
        if(PlayerInteraction.Instance.stamina > 50)
        {
            PlayerInteraction.Instance.overrideDamagePulse = true;
            if(maxCharge) PlayerInteraction.Instance.StaminaChange(-5);
            else
            {
                if(isUpgrade) PlayerInteraction.Instance.StaminaChange(-1);
                else PlayerInteraction.Instance.StaminaChange(-3);
            }
        }
        toolAnim.SetFloat("AnimSpeed", 1f + animSpeedMod);
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.4f * coolDownMod, 1.1f * coolDownMod));
        PlayerMovement.restrictMovementTokens++;
        PlayerCam.Instance.NewObjectOfInterest(pos);
    }

    IEnumerator ChargeTimer()
    {
        maxCharge = false;
        upgradedCharge = false;
        yield return new WaitForSeconds(0.2f * coolDownMod);
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(PlayerInteraction.Instance.gameObject, 0.6f, "HoeCharge", false));

        yield return new WaitForSeconds(0.5f * coolDownMod);
        if(InputManager.isCharging)
        {
            AudioPoolManager.Instance.PlayClip(chargeReady, 0.8f);
            maxCharge = true;
            //Debug.Log("Charged Up");
        }
        if(!isUpgrade) yield break;
        yield return new WaitForSeconds(0.8f * coolDownMod);
        if(InputManager.isCharging)
        {
            AudioPoolManager.Instance.PlayClip(chargeReady, 0.8f);
            upgradedCharge = true;
            //Debug.Log("Charged Up");
        }
    }


}
