using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanicSoundController : MonoBehaviour
{
    public AudioSource panicSound;
    public float fadeTime = 30;
    private float elapsedTime = 0.0f;
    private bool callOnce;

    void Start()
    {
        panicSound = GetComponent<AudioSource>();
        callOnce = true;
    }

    void Update()
    {
        if (SimController.hasStarted)
        {
            if (callOnce)
            {
                panicSound.Play();
                callOnce = false;
            }
            if (!callOnce && panicSound.isPlaying && elapsedTime < fadeTime)
            {
                panicSound.volume = 1 - elapsedTime / fadeTime;
                elapsedTime += 0.02f;
            }
        }
    }
}
