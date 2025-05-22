using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refinery : StructureBehaviorScript
{
    public InventoryItemData timberEar, gloomStalk;
    public InventoryItemData Wood, gloomBundles; //Cost to refine is lets just say 5 units of each
    
    public Transform itemDropTransform;
    
    public bool ownedByPlayer = false; //To indicate if this is the town one

    //public Animator anim;

    public int progress = 0;
    int maxProgress = 5;
    int maxContainedItems = 25;

    bool ignoreNextHour = false;
}
