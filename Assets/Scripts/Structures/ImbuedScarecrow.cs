using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ImbuedScarecrow : StructureBehaviorScript
{
    public static UnityAction<GameObject> OnScarecrowAttract;

    public InventoryItemData recoveredItem;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        //StartCoroutine(AttractEnemies());
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            StartCoroutine(DugUp());
            success = true;
        }
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
        droppedItem.transform.position = transform.position;

        Destroy(this.gameObject);
    }

    IEnumerator AttractEnemies()
    {
        yield return new WaitForSeconds(2);
        do
        {
            yield return new WaitForSeconds(10);
            int x = Random.Range(0, 10);
            if (x <= 7)
            { 
                OnScarecrowAttract?.Invoke(this.gameObject);
                Debug.Log("SCARECROW ATTRACTING");
            }
        }
        while (gameObject.activeSelf);
    }

    /*private void OnDestroy()
    {
        SpawnInComponents();
        base.OnDestroy();
    }*/

    private void SpawnInComponents()
    {
        foreach (Transform child in this.transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                DestroyAfterTime destroyMe;
                destroyMe = child.GetComponent<DestroyAfterTime>();
                if (destroyMe != null)
                {
                    destroyMe.enabled = true;
                    destroyMe.destroy = true;
                }
                else
                {
                    Destroy(child.gameObject);
                }

            }
        }
        this.transform.DetachChildren();

    }

}
