using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClampName : MonoBehaviour
{
    public Text nameLable;
    private bool hasStarted;

    private void Start()
    {
        hasStarted = false;
    }

    void Update()
    {
        StartCoroutine("StartTimer", 10);

        if (hasStarted)
        {
            nameLable.gameObject.SetActive(false);
        }
        else
        {
            Vector3 namePos = Camera.main.WorldToScreenPoint(this.transform.position);
            nameLable.transform.position = namePos;
        }
    }

    IEnumerator StartTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        hasStarted = true;
    }
}
