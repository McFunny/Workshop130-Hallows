using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FleeingApothecary : MonoBehaviour
{
    bool released;

    public NavMeshAgent agent;

    public Animator anim;

    void Start()
    {
        anim.Play("trapped");
    }

    public void Released()
    {
        if(released) return;
        released = true;

        StartCoroutine(RunAway());
    }

    IEnumerator RunAway()
    {
        anim.Play("flee");
        agent.enabled = false;
        agent.enabled = true;
        agent.SetDestination(WagonManager.Instance.wildernessWagon.transform.position);
        yield return new WaitForSeconds(7);
        Destroy(gameObject);
    }
}
