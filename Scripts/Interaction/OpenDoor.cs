using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public bool isSliding = false;
    public bool isLeft = false;
    public bool isNegative = false;
    public bool doNotLockThis = false;

    [HideInInspector]
    public bool lockDoors = false;

    bool callOnce = true;
    Animator anim;
    Collider col1;
    Collider col2;

    void Start()
    {
        anim = transform.GetChild(0).GetComponent<Animator>();
        col1 = gameObject.GetComponent<BoxCollider>();
        col2 = transform.GetChild(0).GetComponent<BoxCollider>();

        if(isLeft)
        {
            anim.SetBool("isLeft", true);
        }
        else
        {
            anim.SetBool("isLeft", false);
        }

        if(isNegative)
        {
            anim.SetBool("isNegative", true);
        }
        else
        {
            anim.SetBool("isNegative", false);
        }

        if(isSliding)
        {
            anim.SetBool("isSliding", true);
        }
        else
        {
            anim.SetBool("isSliding", false);
        }

        anim.SetBool("open", false);
    }

    void Update()
    {
        if(SimController.doorsAreNowLocked && !doNotLockThis && callOnce)
        {
            anim.SetBool("open", false);
            col1.enabled = false;
            callOnce = false;
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Shooter") || col.CompareTag("Victim") || col.CompareTag("Player"))
        {
            anim.SetBool("open", true);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Shooter") || col.CompareTag("Victim") || col.CompareTag("Player"))
        {
            StartCoroutine("doorLockWait", 2);
        }
    }

    // Waiting time between the player/victims leave the collider and the door closes.
    IEnumerator doorLockWait(float timer)
    {
        yield return new WaitForSeconds(timer);
        anim.SetBool("open", false);
    }
}
