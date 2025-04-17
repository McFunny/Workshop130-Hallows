using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowPod : MonoBehaviour
{
    public List<GameObject> crows;

    public bool inWilderness = false;

    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
        foreach(GameObject crow in crows)
        {
            crow.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
            if(inWilderness) WildernessManager.Instance.allCreatures.Add(crow.GetComponentInChildren<CreatureBehaviorScript>());
            crow.transform.parent = null;
        }
        Destroy(this.gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        //crows.RemoveAll(item => item == null);
        //if(crows.Count == 0) Destroy(this.gameObject);
    }
}
