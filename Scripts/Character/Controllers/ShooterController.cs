using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ShooterController : MonoBehaviour
{
    public List<GameObject> patrolPoints = new List<GameObject>();
    public GameObject startingPoint;
    public float speed = 2.5f;
    public float shootTime = .2f;
    public float reloadTime = 0.5f;
    public int clipSize = 30;
    public GameObject muzzleFlash;

    [Header("Debug")]
    [SerializeField] private bool showShootingLines = false;
    [SerializeField] private bool showPatrolPath = false;

    NavMeshAgent agent;
    Animator anim;
    ParticleSystem flash;
    LineOfSight los;
    GameObject shootAt;
    AudioSource shootingSound;

    int curClip = 0;
    int destPoint = 0;
    bool startShooting = true;
    int count = 0;
    bool isReloading = false;

    void Start()
    {
        los = GetComponent<LineOfSight>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.speed = speed;
        curClip = clipSize;
        flash = muzzleFlash.GetComponent<ParticleSystem>();
        shootingSound = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (SimController.hasStarted)
        {
            //take care of null here
            if (los.visibleTargets.Count == 0 || isReloading)
            {
                anim.SetBool("hasVisibleTargets", false);
                flash.Stop();
            }
            else if (los.visibleTargets.Count != 0 && !isReloading)
            {
                anim.SetBool("hasVisibleTargets", true);
                flash.Play();
                if (!shootingSound.isPlaying)
                {
                    shootingSound.Play();
                }
            }

            Kill();

            Patrol();
        }
        anim.SetFloat("velocity", agent.velocity.magnitude);
    }

    void Kill()
    {
        if (startShooting)
        {
            InvokeRepeating("Shoot", 0, shootTime);
            startShooting = false;
        }

        if (curClip <= 0 && !isReloading)
        {
            StartCoroutine("Reload");
        }
    }

    IEnumerator Reload()
    {
        CancelInvoke();
        isReloading = true;
        anim.SetTrigger("reload");
        agent.speed = 0;
        yield return new WaitForSeconds(reloadTime);
        agent.speed = speed;
        startShooting = true;
        curClip = clipSize;
        isReloading = false;
    }

    // Control the shooter to shoot victims.
    void Shoot()
    {
        if (los.visibleTargets.Count == 0)
        {
            return;
        }

        count++;

        shootAt = los.visibleTargets[UnityEngine.Random.Range(0, los.visibleTargets.Count)].gameObject;
        curClip--;

        shootAt.GetComponent<VictimController>().DamageThis();
    }


    //Shows who is getting shot at with a red line (shooting) or a green line (reloading)
    void OnDrawGizmos()
    {
        if (showShootingLines && shootAt != null)
        {
            if (isReloading)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawLine(transform.position, shootAt.transform.position);
        }

        // Shows the path that the shooter takes.
        if (showPatrolPath)
        {
            Gizmos.color = Color.magenta;
            for (int i = 1; i < patrolPoints.Count; i++)
            {
                Gizmos.DrawLine(patrolPoints[i].transform.position, patrolPoints[i - 1].transform.position);
            }
        }
    }


    // Control the movement of the shooter.
    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.01f && !isReloading)
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
    }

    //Used to pause the shooter at a certain patrol point to make it seem that he is looking.
    IEnumerator Stop(float waitTime)
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(waitTime);
        agent.isStopped = false;
    }
}
