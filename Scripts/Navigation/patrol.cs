using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class patrol : MonoBehaviour
{
    public float speed = 1.4f;
    public List<GameObject> patrolPoints = new List<GameObject>();
    private bool hasStarted;

    NavMeshAgent agent;
    Animator anim;

    int destPoint = 0;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.speed = speed;
        hasStarted = false;
    }

    void Update()
    {

        StartCoroutine("StartTimer", 10);

        if (hasStarted == true) {
            if (!agent.pathPending && agent.remainingDistance < 0.01f)
            {
                agent.destination = patrolPoints[destPoint].transform.position;
                StartCoroutine("Stop", patrolPoints[Math.Max(0, destPoint - 1)].GetComponent<PatrolSpot>().waitTime);

                if (destPoint < patrolPoints.Count - 1)
                {
                    destPoint = destPoint + 1;
                }
                else
                {
                    agent.isStopped = true;
                    return;
                }
            }
            anim.SetFloat("velocity", agent.velocity.magnitude);
        }
        
    }

    IEnumerator Stop(float waitTime)
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(waitTime);
        agent.isStopped = false;
    }

    IEnumerator StartTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        hasStarted = true;
    }
}
