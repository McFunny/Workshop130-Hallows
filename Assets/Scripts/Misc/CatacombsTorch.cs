using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatacombsTorch : StructureBehaviorScript
{
    public GameObject fire;
    public bool startActive;
    public bool fireAlwaysActive;
    public bool disableHighlight;
    void Start()
    {
        if (!startActive) { fire.SetActive(false); }
    }


    public override void ToolInteraction(ToolType type, out bool success)
    {
        print("Interacted");
        if (type == ToolType.Torch)
        {
            print("Torch");
            if (PlayerInteraction.Instance.torchLit && fire.activeInHierarchy == false)
            {
                fire.SetActive(true);
                audioHandler.PlaySound(audioHandler.activatedSound);
                success = true;
            }
            else if ((fire.activeInHierarchy == true && !PlayerInteraction.Instance.torchLit) || (fireAlwaysActive && !PlayerInteraction.Instance.torchLit))
            {
                HandItemManager.Instance.TorchFlameToggle(true);
                success = true;
            }
            else success = false;
            return;
        }
        else success = false;

    }
}
