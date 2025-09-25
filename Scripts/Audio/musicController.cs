using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class musicController : MonoBehaviour
{
    private AudioSource audio;

    void Start()
    {
        audio = GetComponent<AudioSource>();     
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            audio.Play();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            audio.Stop();
        }
    }
}
