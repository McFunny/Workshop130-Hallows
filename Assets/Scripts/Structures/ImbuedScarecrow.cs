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

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !TimeManager.Instance.stopTime)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
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

    private void OnDestroy()
    {
        base.OnDestroy();
        if(Tutorial.Instance) Tutorial.Instance.KillScarecrow();
    }

}
