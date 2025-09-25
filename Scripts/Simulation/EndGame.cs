using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class EndGame : MonoBehaviour
{
    private string parID;
    private string sesID;
    private bool first;

    void Start()
    {
        first = true;
        parID = PlayerPrefs.GetString("parID");
        sesID = PlayerPrefs.GetString("sesID");
    }

    private void Update()
    {
        if (Time.timeSinceLevelLoad >= 115 && first == true)
        {
            first = false;
            StartCoroutine(SendTextToFile());
            SceneManager.LoadScene(3);
        }
    }


    IEnumerator SendTextToFile()
    {
        WWWForm form = new WWWForm();
        form.AddField("end", "true");
        form.AddField("parID", parID);
        form.AddField("sesID", sesID);
        WWW www = new WWW("https://ilabhdbe.azurewebsites.net/fromunity.php", form);
        yield return www;
    }
}
