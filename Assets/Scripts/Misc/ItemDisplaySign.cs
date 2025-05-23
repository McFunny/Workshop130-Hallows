using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemDisplaySign : MonoBehaviour
{
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;
    public TextMeshProUGUI itemType;
    public TextMeshProUGUI shopNPC;
    private string lastSavedNPC;
    private Color c_default, c_tool, c_placeable, c_crop, c_consumable;
    public StructureShopDisplay structureDisplay;

    public ParticleSystem changeParticle;
    public Rigidbody signRB;

    private void Start()
    {
        itemName.text = "";
        itemDescription.text = "";
        itemType.text = "";
        shopNPC.text = "";
        lastSavedNPC = string.Empty;
        ToolTipScript toolTipScript = FindAnyObjectByType<ToolTipScript>();
        c_default = toolTipScript.c_default;
        c_tool = toolTipScript.c_tool;
        c_placeable = toolTipScript.c_placeable;
        c_crop = toolTipScript.c_crop;
        c_consumable = toolTipScript.c_consumable;

    }

    public void UpdateNPCName(NPC npc)
    {
        if (npc.dialogueText.speakerName != null)
        {
            shopNPC.text = npc.dialogueText.speakerName.ToString();
            lastSavedNPC = npc.dialogueText.speakerName.ToString();
            if(changeParticle) changeParticle.Play();
        }
        
    }

    public void ResetDisplay()
    {
        itemName.text = "";
        itemDescription.text = "";
        itemType.text = "";
        shopNPC.text = lastSavedNPC;
        if (structureDisplay != null) { structureDisplay.LeaveShop(); }
        if(changeParticle) changeParticle.Play();
    
    }

    public void LeaveShop()
    {
        itemName.text = "";
        itemDescription.text = "";
        itemType.text = "";
        shopNPC.text = "";
        if (structureDisplay != null) { structureDisplay.LeaveShop(); }
        if(changeParticle) changeParticle.Play();
    }

    public void DisplayItem(InventoryItemData itemData)
    {
        shopNPC.text = "";
        var type = itemData.GetType();
        if (itemData.staminaValue != 0)
        {
            itemType.text = "Consumable";
            itemType.color = c_consumable;
        }
        else if (type.Equals(typeof(ToolItem)))
        {
            itemType.text = "Tool";
            itemType.color = c_tool;
        }
        else if (type.Equals(typeof(PlaceableItem)))
        {
            itemType.text = "Structure";
            itemType.color = c_placeable;
        }
        else if (type.Equals(typeof(CropItem)))
        {
            itemType.text = "Seed";
            itemType.color = c_crop;
        }
        else
        {
            itemType.text = "Misc";
            itemType.color = c_default;
        }

        itemName.text = itemData.displayName;
        itemDescription.text = itemData.description;

        if (structureDisplay != null) { structureDisplay.DisplayStructure(itemData); }
        if(changeParticle) changeParticle.Play();
        if(signRB) signRB.AddForce(signRB.transform.forward * -60, ForceMode.Impulse);
    }

}
