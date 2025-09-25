using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCrouching : MonoBehaviour
{
    CharacterController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            controller.height = 1;
            this.transform.localScale = new Vector3(1, 0.5f, 1);
        }   

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (this.transform.position.x >= -20.8f && this.transform.position.x <= -19.3f && this.transform.position.z >= -52.5f && this.transform.position.z <= -51.2f) {}
            else if (this.transform.position.x >= -42.6f && this.transform.position.x <= -41.2f && this.transform.position.z >= -35.6f && this.transform.position.z <= -34.6f) { }
            else if (this.transform.position.x >= -2.9f && this.transform.position.x <= -2f && this.transform.position.z >= -20.9f && this.transform.position.z <= -19.9f) { }

            else
            {
                controller.height = 1.70f;
                this.transform.localScale = new Vector3(1, 1, 1);
            }

        }
    }
}
