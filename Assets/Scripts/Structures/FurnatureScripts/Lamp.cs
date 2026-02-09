using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : FurnitureBehaviorScript
{
    // Start is called before the first frame update
    public AudioSource source;
    public AudioClip onSound, offSound;

    public List<GameObject> lights = new List<GameObject>();
    private bool isOn = true;
    void Start()
    {
        base.Start();
        FurnitureStart();
    }

    public override void StructureInteraction()
    {
        bool success = false;

        if (isOn)
        {
            source.PlayOneShot(offSound);
            LightsOnOff(!isOn);
            success = true;
           
        }
        else if (!isOn)
        {
            source.PlayOneShot(onSound);
            LightsOnOff(!isOn);
            success = true;
        }

        print("Interaction Had");

    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(absentFromGrid) return;
        if (type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false)
        {
            //StartCoroutine(DugUp());
            success = true;
        }
    }

    public override void DigAction()
    {
        PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);

        Destroy(this.gameObject);
    }

    private void LightsOnOff(bool state)
    {
        for (int i = 0; i < lights.Count; i++)
        {
            lights[i].SetActive(state);
        }
        isOn = state;
    }

    public override void SaveVariables()
    {
        saveInt1 = Convert.ToInt32(isOn);
    }

    public override void LoadVariables()
    {
        isOn = Convert.ToBoolean(saveInt1);
        LightsOnOff(isOn);
    }
}
