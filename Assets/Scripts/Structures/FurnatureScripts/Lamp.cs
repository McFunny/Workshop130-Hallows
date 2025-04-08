using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : FurnitureBehaviorScript
{
    // Start is called before the first frame update

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
            LightsOnOff(!isOn);
            success = true;
           
        }
        else if (!isOn)
        {
            LightsOnOff(isOn);
            success = true;
        }




    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if (type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false)
        {
            StartCoroutine(DugUp());
            success = true;
        }
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);

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
