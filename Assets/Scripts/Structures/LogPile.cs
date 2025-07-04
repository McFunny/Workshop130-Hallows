using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogPile : StructureBehaviorScript
{
    int woodToDrop = 10;

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(Dig());
            success = true;
        }
    }

    public override void DigAction()
    {
        audioHandler.PlaySoundAtPoint(audioHandler.interactSound, transform.position);
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        base.OnDestroy();

        if (!gameObject.scene.isLoaded) return; 

        GameObject droppedItem;
        for(int i = 0; i < woodToDrop; i++)
        {
            //if(Random.Range(0, 100) > 95) droppedItem = ItemPoolManager.Instance.GrabItem(gold);
            /*else */droppedItem = ItemPoolManager.Instance.GrabItem(itemForm);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            Vector3 dir3 = Random.onUnitSphere;
            dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
            Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(dir3 * 35);
            itemRB.AddForce(Vector3.up * 50);
        }
    }

}
