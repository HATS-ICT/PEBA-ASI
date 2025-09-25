using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class xOpenDoor : MonoBehaviour
{
    public bool isSliding = false;
    public bool isLeft = false;
    public bool isNegative = false;
    public bool doNotLockThis = false;
    [HideInInspector]
    public bool lockDoors = false;


    bool callOnce = true;
    Animator anim;
    void Start()
    {
        anim = transform.GetChild(0).GetComponent<Animator>();
        if (isLeft)
        {
            anim.SetBool("isLeft", true);
        }
        else
        {
            anim.SetBool("isLeft", false);
        }

        if (isNegative)
        {
            anim.SetBool("isNegative", true);
        }
        else
        {
            anim.SetBool("isNegative", false);
        }

        if (isSliding)
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
        if (SimController.doorsAreNowLocked && !doNotLockThis && callOnce)
        {
            anim.SetBool("open", false);
            //gameObject.transform.GetChild(0).GetComponent<Animator>().enabled = false;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            callOnce = false;
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (!SimController.doorsAreNowLocked || doNotLockThis)
        {
            if (col.CompareTag("Shooter") || col.CompareTag("Victim") || col.CompareTag("Player"))
            {
                anim.SetBool("open", true);
            }
        }
    }
}
