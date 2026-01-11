using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CookBook : FurnitureBehaviorScript
{
    public Animator anim;
    //private CodexRework codex;

    private void Start()
    {
        base.Start();
        FurnitureStart();

        StartCoroutine(DistanceCheck());
    }

    public override void StructureInteraction()
    {
        //INSERT CODE TO BRING UP COOK BOOK UI HERE
        //IT MAY BE WISE TO ADD A BUTTON IN THE UI TO TURN THIS STRUCTURE INTO AN ITEM, SO THE PLAYER DOES NOT HAVE TO USE SHOVEL TO REMOVE

        /*
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }
        */
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false)
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


    IEnumerator DistanceCheck()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(0.5f);
            if(Vector3.Distance(PlayerInteraction.Instance.transform.position, transform.position) < 10) anim.SetBool("IsOpen", true);
            else anim.SetBool("IsOpen", false);
        }
    }
}
