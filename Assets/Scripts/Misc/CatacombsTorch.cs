using System;
using UnityEngine;

public class CatacombsTorch : StructureBehaviorScript
{
    public GameObject fire;
    public int ID = -1;
    public bool startActive;
    public bool fireAlwaysActive;
    public bool disableHighlight;
    public bool ignoreForAchievement = false;
    public AudioSource source;

    public bool IsLit => fireAlwaysActive || (fire != null && fire.activeInHierarchy);

    void Start()
    {
        if (fire == null)
        {
            return;
        }

        if (fireAlwaysActive) fire.SetActive(true);
        else fire.SetActive(startActive);
    }

    public void SetLitFromLoad(bool lit)
    {
        if (fire == null) return;
        if (fireAlwaysActive) lit = true;
        fire.SetActive(lit);
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        //print("Interacted");
        if (type == ToolType.Torch)
        {
            //print("Torch");
            if (PlayerInteraction.Instance.torchLit && fire != null && !fire.activeInHierarchy)
            {
                fire.SetActive(true);
                if (source != null) source.Play();
                CatacombsTorchManager.Instance.CheckIfAllTorchesLit();
                success = true;
            }
            else if (((fire != null && fire.activeInHierarchy) || fireAlwaysActive) && !PlayerInteraction.Instance.torchLit)
            {
                HandItemManager.Instance.TorchFlameToggle(true);
                success = true;
            }
            else success = false;
            return;
        }
        else if (type == ToolType.Pyrefly && ((fire != null && fire.activeInHierarchy) || fireAlwaysActive) && !PlayerInteraction.Instance.pyreflyLit)
        {
            HandItemManager.Instance.PyreflyFlameToggle(true);
            success = true;
        }
        else success = false;
    }



    public TorchEntry ExportTorchData()
    {
        return new TorchEntry
        {
            savedID = ID,
            savedIsLit = IsLit
        };
    }

    public void ImportTorchData(TorchEntry entry)
    {
        if (entry.savedID != ID) return;
        SetLitFromLoad(entry.savedIsLit);
    }
}
