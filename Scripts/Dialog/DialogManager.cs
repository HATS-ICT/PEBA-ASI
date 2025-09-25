using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;


public class DialogMessage
{
    public string uid;
    public float time;
    public Vector3 location;
    public string content;
    public string speaker;

    public DialogMessage(string speaker, string content, Vector3 location)
    {
        this.uid = Guid.NewGuid().ToString();
        this.time = Time.time;
        this.location = location;
        this.content = content;
        this.speaker = speaker;
    }
}

public class DialogManager : MonoBehaviour
{
    private const float DIALOG_RADIUS = 3f;
    private const float DIALOG_TIME_WINDOW = 3f;

    private static DialogManager _instance;
    public static DialogManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // First check if there's an existing instance in the scene
                _instance = FindObjectOfType<DialogManager>();
                
                // If not, create a new GameObject
                if (_instance == null)
                {
                    GameObject go = new GameObject("DialogManager");
                    _instance = go.AddComponent<DialogManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private List<DialogMessage> dialogHistory = new List<DialogMessage>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddDialog(string speaker, string content, Vector3 location)
    {
        // agent can be silent sometimes
        if (string.IsNullOrEmpty(content))
        {
            return;
        }
        DialogMessage message = new DialogMessage(speaker, content, location);
        dialogHistory.Add(message);
    }

    public List<DialogMessage> GetDialogHistory()
    {
        return new List<DialogMessage>(dialogHistory);
    }

    public void ClearHistory()
    {
        dialogHistory.Clear();
    }

    public List<SurroundingDialogues> GetSurroundingDialogue(Vector3 location, int limit = int.MaxValue)
    {
        List<DialogMessage> recentDialogs = dialogHistory.FindAll(d => 
            d.time > Time.time - DIALOG_TIME_WINDOW && 
            Vector3.Distance(d.location, location) < DIALOG_RADIUS
        );

        return recentDialogs
            .OrderByDescending(d => d.time)
            .Take(limit)
            .Select(d => new SurroundingDialogues
            {
                content = d.content,
                speaker = d.speaker,
                time = d.time
            })
            .ToList();
    }

    public void SaveDialogHistory(string folderPath)
    {
        try
        {
            string filePath = System.IO.Path.Combine(folderPath, "dialog.json");
            
            var serializableDialogs = dialogHistory.Select(d => new 
            {
                uid = d.uid,
                time = d.time,
                location = new { x = d.location.x, y = d.location.y, z = d.location.z },
                content = d.content,
                speaker = d.speaker
            }).ToList();
            
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(serializableDialogs, settings);
            System.IO.File.WriteAllText(filePath, json);
            Debug.Log($"Saved dialog history to {filePath} with {dialogHistory.Count} messages");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save dialog history: {ex.Message}");
        }
    }

    public void OnSimulationEnd()
    {
        if (SimulationLogger.Instance != null)
        {
            SaveDialogHistory(SimulationLogger.Instance.GetCurrentSimulationFolder());
        }
    }
}

[System.Serializable]
public class DialogHistoryWrapper
{
    public DialogMessage[] messages;

    public DialogHistoryWrapper(List<DialogMessage> dialogHistory)
    {
        messages = dialogHistory.ToArray();
    }
}