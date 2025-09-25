using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class AgentLogger
{
    private string agentName;
    private Persona persona;
    private List<LoggedObservation> observations = new List<LoggedObservation>();
    private List<LoggedAction> actions = new List<LoggedAction>();
    private List<LoggedMemory> memories = new List<LoggedMemory>();
    private List<LoggedPosition> trajectory = new List<LoggedPosition>();
    private AgentTraits agentTraits;
    private DateTime simulationStartTime;
    private string finalStatus = "Alive";
    
    public AgentLogger(string agentName, Persona persona)
    {
        this.agentName = SanitizeFileName(agentName);
        this.persona = persona;
        simulationStartTime = DateTime.Now;
    }
    
    private string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }
    
    public void LogPosition(Vector3 position, Vector3 forward, int health, string healthStatus)
    {
        trajectory.Add(new LoggedPosition
        {
            time = GetSimulationTime(),
            x = position.x,
            y = position.y,
            z = position.z,
            rotation_x = forward.x,
            rotation_y = forward.y,
            rotation_z = forward.z,
            health = health,
            health_status = healthStatus
        });
    }
    
    public void LogPosition(Vector3 position, Vector3 forward)
    {
        LogPosition(position, forward, 1, "Alive");
    }
    
    public void LogMemory(string description)
    {
        memories.Add(new LoggedMemory
        {
            time = GetSimulationTime(),
            description = description
        });
    }
    
    public void LogObservation(Observation observation)
    {
        observations.Add(new LoggedObservation
        {
            time = GetSimulationTime(),
            observation = observation
        });
    }
    
    public void LogAction(Action action, Vector3? targetLocation, string plan = null)
    {
        actions.Add(new LoggedAction
        {
            time = GetSimulationTime(),
            action_type = action.actionType.ToString(),
            movement_state = action.movementState.ToString(),
            dialog_text = action.dialogText,
            plan = plan,
            target_location = targetLocation.HasValue 
                ? new Vector3Data { x = targetLocation.Value.x, y = targetLocation.Value.y, z = targetLocation.Value.z } 
                : null
        });
    }
    
    public void LogAgentTraits(TrainingLevel trainingLevel, FamiliarityLevel familiarityLevel, ShooterPerceptionLevel shooterPerceptionLevel)
    {
        agentTraits = new AgentTraits
        {
            training_level = trainingLevel.ToString(),
            familiarity_level = familiarityLevel.ToString(),
            shooter_perception_level = shooterPerceptionLevel.ToString()
        };
    }
    
    private float GetSimulationTime()
    {
        return (float)(DateTime.Now - simulationStartTime).TotalSeconds;
    }

    public void SetFinalStatus(string status)
    {
        if (status == "Escaped" || status == "Dead" || status == "Alive")
        {
            finalStatus = status;
        }
        else
        {
            Debug.LogWarning($"Invalid final status: {status}. Using 'Alive' as default.");
            finalStatus = "Alive";
        }
    }
    
    public void SaveToFile(string folderPath)
    {
        if (!SimConfig.LoggingEnabled)
        {
            return;
        }
        
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            string sanitizedName = new string(agentName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
            string filePath = Path.Combine(folderPath, $"{sanitizedName}.json");
            
            var logData = new AgentLogData
            {
                persona = persona,
                traits = agentTraits,
                observations = observations,
                actions = actions,
                memories = memories,
                trajectory = trajectory,
                final_status = finalStatus
            };
            
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new UnityContractResolver(),
                Formatting = Formatting.Indented
            };
            
            string json = JsonConvert.SerializeObject(logData, settings);
            
            File.WriteAllText(filePath, json);
            
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save agent log for {agentName}: {ex.Message}");
            Debug.LogException(ex);
        }
    }
    
    
    [Serializable]
    private class AgentLogData
    {
        public Persona persona;
        public AgentTraits traits;
        public List<LoggedObservation> observations;
        public List<LoggedAction> actions;
        public List<LoggedMemory> memories;
        public List<LoggedPosition> trajectory;
        public string final_status;
    }
    
    [Serializable]
    private class LoggedObservation
    {
        public float time;
        public Observation observation;
    }
    
    [Serializable]
    private class LoggedAction
    {
        public float time;
        public string action_type;
        public string movement_state;
        public string dialog_text;
        public string plan;
        public Vector3Data target_location;
    }
    
    [Serializable]
    private class LoggedMemory
    {
        public float time;
        public string description;
    }
    
    [Serializable]
    private class LoggedPosition
    {
        public float time;
        public float x;
        public float y;
        public float z;
        public float rotation_x;
        public float rotation_y;
        public float rotation_z;
        public int health;
        public string health_status;
    }
    
    [Serializable]
    private class Vector3Data
    {
        public float x;
        public float y;
        public float z;
    }
    
    [Serializable]
    private class AgentTraits
    {
        public string training_level;
        public string familiarity_level;
        public string shooter_perception_level;
    }
}

public class UnityContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        
        if (property.PropertyType == typeof(GameObject) ||
            property.PropertyType == typeof(Transform) ||
            property.PropertyType == typeof(MonoBehaviour) ||
            property.PropertyType == typeof(Component) ||
            (property.PropertyType != null && property.PropertyType.IsSubclassOf(typeof(Component))))
        {
            property.ShouldSerialize = instance => false;
        }
        
        return property;
    }
}