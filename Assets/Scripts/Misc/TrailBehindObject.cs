using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailBehindObject : MonoBehaviour
{
    public Transform objectToFollow;

    public float speed = 10;
    
    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        if(objectToFollow == null) Destroy(gameObject);
        else
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, objectToFollow.position, step);

        }
    }
}
