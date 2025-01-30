using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ImbuedScarecrow : StructureBehaviorScript
{
    public static UnityAction<GameObject> OnScarecrowAttract;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        //StartCoroutine(AttractEnemies());
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
