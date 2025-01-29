using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MenuWalk : MonoBehaviour
{
    public Animator anim;

    public NavMeshAgent agent;

    public Transform start, end;

    public float timeToStartMax, timeToStartMin;
    public float coolDown;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        while(gameObject.activeSelf)
        {
            float t = Random.Range(timeToStartMin, timeToStartMax);
            if(anim) anim.SetBool("IsRunning", true);
            yield return new WaitForSeconds(t);
            transform.position = start.position;
            agent.SetDestination(end.position);
            yield return new WaitForSeconds(coolDown);
        }
    }
}
