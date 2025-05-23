using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopStall : MonoBehaviour
{
    public List<StoreItem> storeItems;
    public ItemDisplaySign displaySign;
    public BarterSign barterSign;
    
    void OnTriggerEnter(Collider npc)
    {
        var npcScript = npc.gameObject.GetComponent<NPC>();
        if(npcScript != null)
        {
            npcScript.assignedStall = this;
        }
        
    }


}
