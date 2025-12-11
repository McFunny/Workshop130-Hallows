using UnityEngine;

public class DebugButtonID : MonoBehaviour
{
    PlayerInventoryHolder playerInv;
    public InventoryItemData data;
    public CreatureObject creature;
    public CreatureVariant variant; // if you're listing variants

    void Start()
    {
        playerInv = FindFirstObjectByType<PlayerInventoryHolder>();
    }

    public void ClearPayload()
    {
        data = null;
        creature = null;
        variant = null;
    }

    public void SpawnItem()
    {
        int count = Mathf.Max(1, DebugUI.SpawnCount);

       
        if (data)
        {
            if(data.itemBehavior) data.itemBehavior.OnRecieve(data);
            playerInv.AddToInventory(data, count);
            return;
        }

        
        GameObject prefab = null;
        if (variant != null && variant.prefab != null) prefab = variant.prefab;
        else if (creature != null) prefab = creature.objectPrefab;

        if (prefab == null)
        {
            
            return;
        }

        var player = PlayerMovement.Instance ?? FindFirstObjectByType<PlayerMovement>();
        if (player == null || player.orientation == null)
        {
          
            return;
        }

        Vector3 fwd = player.orientation.forward;
        Vector3 origin = player.transform.position;

       
        Vector3 spawnPos = origin + fwd * 30f;
        if (Physics.Raycast(spawnPos + Vector3.up * 2f, Vector3.down, out var hit, 5f))
            spawnPos = hit.point;

        Quaternion rot = Quaternion.LookRotation(new Vector3(fwd.x, 0f, fwd.z), Vector3.up);

       
        for (int i = 0; i < count; i++)
            Instantiate(prefab, spawnPos, rot);
    }

}
