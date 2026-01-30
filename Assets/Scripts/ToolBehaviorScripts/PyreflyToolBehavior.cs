using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Pyrefly")]
public class PyreflyToolBehavior : ToolBehavior
{
    public InventoryItemData thisItem;
    FireFearTrigger fireScript;
    public AudioClip ignite, extinguish;
    public GameObject thrownPrefab;

    float speed = 100;


    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || TownGate.Instance.location == PlayerLocation.InTown) return;
        if (!player) player = _player;

        if(!PlayerInteraction.Instance.pyreflyLit) return;
        
        tool = _tool;
        usingPrimary = true;
        //Shoot
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.1f, 0.5f));
    }
    
    
    public override void SecondaryUse(Transform _player, ToolType _tool) //To Ignite the bug
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;

        if(PlayerInteraction.Instance.pyreflyLit) return;

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                bool playAnim = false;

                structure.ToolInteraction(tool, out playAnim);

                if(playAnim)
                {
                    ////HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, .5f));
                    PlayerMovement.restrictMovementTokens++;
                    usingSecondary = true;
                    if(structure.focalPoint != null ) PlayerCam.Instance.NewObjectOfInterest(structure.focalPoint.position);
                    return;
                } 
            }

            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                if(interactSuccessful)
                {
                    //HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, .5f));
                    PlayerMovement.restrictMovementTokens++;
                    usingSecondary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                }

            }

            var enemy = hit.collider.GetComponentInParent<CreatureBehaviorScript>();
            if (enemy != null)
            {
                enemy.ToolInteraction(tool, out bool success);
                if(success)
                {
                    //HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(ignite);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, .5f));
                    PlayerMovement.restrictMovementTokens++;
                    usingSecondary = true;
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
            }
        } 


        if(!PlayerInteraction.Instance.pyreflyLit && TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.Pyrecharge))
        {
            TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.Pyrecharge);
            HandItemManager.Instance.PyreflyFlameToggle(true);
            HandItemManager.Instance.toolSource.PlayOneShot(ignite);
            PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.2f, 0.5f));
        }
    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            usingPrimary = false;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            HandItemManager.Instance.PyreflyFlameToggle(false);
            ThrowBug();
        }
        if (usingSecondary)
        {
            usingSecondary = false;
            PlayerMovement.restrictMovementTokens--;
            PlayerCam.Instance.ClearObjectOfInterest();
        }
    }

    void ThrowBug()
    {
        Transform bulletStart = HandItemManager.Instance.bulletStart;

        GameObject newBug = ProjectilePoolManager.Instance.GrabPyreflyBullet();
        newBug.transform.position = bulletStart.position;
        newBug.transform.rotation = Quaternion.identity;
        Vector3 dir = bulletStart.forward;
        Rigidbody rb = newBug.GetComponent<Rigidbody>();
        rb.AddForce(dir * speed);
        rb.AddForce(Vector3.up * 50);
    }
}