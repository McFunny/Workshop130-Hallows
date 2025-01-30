using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowPod : MonoBehaviour
{
    public List<GameObject> crows;

    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
        foreach(GameObject crow in crows)
        {
            crow.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
        }
    }
    // Update is called once per frame
    void Update()
    {
        crows.RemoveAll(item => item == null);
        if(crows.Count == 0) Destroy(this.gameObject);
    }
}
