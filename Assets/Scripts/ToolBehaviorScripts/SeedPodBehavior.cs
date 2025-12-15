using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/SeedPod")]
public class SeedPodBehavior : ToolBehavior
{
    public InventoryItemData thisItem;
    public AudioClip throwSFX;

    public GameObject nutPrefab;
    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary /*|| PlayerInteraction.Instance.toolCooldown*/) return;
        if (!player) player = _player;
        tool = _tool;
        ItemUsed();
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);

            }
        }

    }

    public override void ItemUsed()
    {
        ThrowNut();

    }



    void ThrowNut()
    {
        HandItemManager.Instance.toolSource.PlayOneShot(throwSFX);

        Ray camRay = PlayerInteraction.Instance.mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 origin = camRay.origin;
        Vector3 dir = camRay.direction;
        origin += (dir.normalized * 3);

        GameObject newBullet = Instantiate(nutPrefab, origin, Quaternion.identity);
        newBullet.transform.rotation = PlayerMovement.Instance.orientation.rotation;

        newBullet.GetComponent<Rigidbody>().AddForce(dir * 1700);
        newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 500);

        Vector2 moveInput = PlayerInteraction.Instance.controlManager.movement.action.ReadValue<Vector2>();
        if((moveInput.y >= 0f && moveInput.x != 0f) || moveInput.y > 0f) 
            newBullet.GetComponent<Rigidbody>().AddForce(dir * 900);

        PlayerInventoryHolder.Instance.RemoveItemsFromBothInventories(thisItem, 1);
        PlayerInventoryHolder.Instance.UpdateInventory();
        PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);
    }

    public override void OnHolster()
    {
        HandItemManager.Instance.toolSource.PlayOneShot(throwSFX);

        Ray camRay = PlayerInteraction.Instance.mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 origin = camRay.origin;
        Vector3 dir = camRay.direction;
        origin += (dir.normalized * 3);

        GameObject newBullet = Instantiate(nutPrefab, origin, Quaternion.identity);
        newBullet.transform.rotation = PlayerMovement.Instance.orientation.rotation;

        newBullet.GetComponent<Rigidbody>().AddForce(dir * 200);
        newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 100);

        PlayerInventoryHolder.Instance.RemoveItemsFromBothInventories(thisItem, 1);
        PlayerInventoryHolder.Instance.UpdateInventory();
        PlayerMovement.Instance.RemoveSpeedMod(PlayerInteraction.Instance.gameObject);
    }

    public override void OnEquip()
    {
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(PlayerInteraction.Instance.gameObject, 0.65f, "SeedPod", false));
    }
}
