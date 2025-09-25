using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingSoundController : MonoBehaviour
{
    public AudioSource startingSound;

    void Start()
    {
        startingSound = GetComponent<AudioSource>();
        startingSound.loop = true;
    }

    void Update()
    {
        if (SimController.hasStarted)
        {
            startingSound.loop = false;
            //startingSound.Stop();
        }

    }
}
