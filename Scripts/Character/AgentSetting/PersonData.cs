using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;

[System.Serializable]
public class Persona
{
    public string name;
    public string role;
    public string age;
    public string gender;
    public string pronouns;
    public string personality_traits;
    public string emotional_disposition;
    public string motivations_goals;
    public string communication_style;
    public string knowledge_scope;
    public string backstory;

    public override string ToString()
    {
        return $"Name: {name}, Role: {role}, Age: {age}, Gender: {gender}, Pronouns: {pronouns}";
    }

    public string ToJSONString()
    {
        return $"{{\"name\": \"{name}\", \"role\": \"{role}\", \"age\": \"{age}\", \"gender\": \"{gender}\", \"pronouns\": \"{pronouns}\", " +
               $"\"personality_traits\": \"{personality_traits}\", \"emotional_disposition\": \"{emotional_disposition}\", " +
               $"\"motivations_goals\": \"{motivations_goals}\", \"communication_style\": \"{communication_style}\", " +
               $"\"knowledge_scope\": \"{knowledge_scope}\", " +
               $"\"backstory\": \"{backstory}\"}}";
    }

    public string ToMarkdownString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Character Profile");
        sb.AppendLine($"**Name:** {name}");
        sb.AppendLine($"**Role:** {role}");
        sb.AppendLine($"**Age:** {age}");
        sb.AppendLine($"**Gender:** {gender}");
        sb.AppendLine($"**Pronouns:** {pronouns}");
        
        sb.AppendLine("\n## Personality");
        sb.AppendLine($"**Traits:** {personality_traits}");
        sb.AppendLine($"**Emotional Disposition:** {emotional_disposition}");
        sb.AppendLine($"**Motivations & Goals:** {motivations_goals}");
        
        sb.AppendLine("\n## Communication");
        sb.AppendLine($"**Style:** {communication_style}");
        sb.AppendLine($"**Knowledge:** {knowledge_scope}");
        
        sb.AppendLine("\n## Background");
        sb.AppendLine($"{backstory}");
        
        return sb.ToString();
    }
}

[System.Serializable]
public class MemoryEvent
{
    public long time;  // Stored as UNIX timestamp
    public string description;
}

public class Memory
{
    public List<MemoryEvent> events;
    private long startTimestamp;

    public Memory()
    {
        events = new List<MemoryEvent>();
        startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string GetReadableTime(long timestamp)
    {
        // Calculate how much time has passed since the event
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        TimeSpan timeDiff = DateTimeOffset.FromUnixTimeSeconds(currentTime) - DateTimeOffset.FromUnixTimeSeconds(timestamp);
        
        // For past events, timeDiff will be positive
        if (timeDiff.TotalSeconds < 60) return $"{Math.Floor(timeDiff.TotalSeconds)} seconds ago";
        if (timeDiff.TotalMinutes < 60) return $"{Math.Floor(timeDiff.TotalMinutes)} minutes ago";
        if (timeDiff.TotalHours < 24) return $"{Math.Floor(timeDiff.TotalHours)} hours ago";
        return $"{Math.Floor(timeDiff.TotalDays)} days ago";
    }

    public override string ToString()
    {
        return $"Memory: {string.Join(", ", events.Select(e => $"{GetReadableTime(e.time)}: {e.description}"))}";
    }

    public string ToJSONString()
    {
        return $"{{\"events\": [{string.Join(", ", events.Select(e => $"{{\"time\": \"{GetReadableTime(e.time)}\", \"description\": \"{e.description}\"}}"))}]}}";
    }

    public string ToMarkdownString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Memories");
        
        if (events != null && events.Count > 0)
        {
            foreach (var e in events)
            {
                sb.AppendLine($"- {GetReadableTime(e.time)}: {e.description}");
            }
        }
        else
        {
            sb.AppendLine("- No memories recorded yet.");
        }
        
        return sb.ToString();
    }
}


[System.Serializable]
public class Observation
{
    public string mood;
    public string current_movement_state;
    public bool isHiding;
    public Region location;
    public ShooterInfo shooter_info;
    public List<SurroundingPeople> surrounding_people;
    public List<NeighborRegions> neighbor_regions;
    public List<SurroundingInterestPoints> interest_points;
    public List<SurroundingDialogues> surrounding_conversation;
    public List<PendingEvent> pending_events;
    public List<string> available_action_ids;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Observation:");
        if (shooter_info != null)
        {
            sb.AppendLine($"  Shooter Info: Location: {shooter_info.regionId}, Distance: {shooter_info.distance}, " +
                          $"IsInLineOfSight: {shooter_info.isInLineOfSight}, Direction: {shooter_info.direction}");
            sb.AppendLine($" I am currently hiding: {isHiding}");
        }
        if (surrounding_people != null && surrounding_people.Count > 0)
        {
            sb.AppendLine("  Surrounding People:");
            foreach (var person in surrounding_people)
            {
                sb.AppendLine($"ID: {person.name}, Health Status: {person.health_status}");
            }
        }
        if (neighbor_regions != null && neighbor_regions.Count > 0)
        {
            sb.AppendLine("  Neighbor Regions:");
            foreach (var region in neighbor_regions)
            {
                sb.AppendLine($"ID: {region.id}, Description: {region.description}, Distance: {region.distance}m, Direction: {region.direction}");
            }
        }
        if (interest_points != null && interest_points.Count > 0)
        {
            sb.AppendLine("  Interest Points Inside the Current Region:");
            foreach (var point in interest_points)
            {
                sb.AppendLine($"ID: {point.id}, Distance: {point.distance}, Occupant: {point.occupant}");
            }
        }
        if (location != null)
        {
            sb.AppendLine($"  My Current Region Location: {location.regionId}, Region Description: {location.regionDescription}");
        }
        if (surrounding_conversation != null && surrounding_conversation.Count > 0)
        {
            sb.AppendLine("  Nearby Conversation:");
            foreach (var conversation in surrounding_conversation)
            {
                sb.AppendLine($"    {conversation}");
            }
        }
        if (pending_events != null && pending_events.Count > 0)
        {
            sb.AppendLine("  Pending Events:");
            foreach (var evt in pending_events)
            {
                sb.AppendLine($"    {evt.eventType}: {evt.description}");
            }
        }
        if (available_action_ids != null && available_action_ids.Count > 0)
        {
            sb.AppendLine("  Available Action IDs:");
            sb.AppendLine($"    {string.Join(", ", available_action_ids)}");
        }
        return sb.ToString();
    }
    
    public string ToJSONString()
    {
        string shooterJson = shooter_info != null 
            ? $"\"shooter_info\": {{\"regionId\": \"{shooter_info.regionId}\", \"distance\": \"{shooter_info.distance}m\", \"isInLineOfSight\": \"{shooter_info.isInLineOfSight}\", \"direction\": \"{shooter_info.direction}\"}}"
            : "\"shooter_info\": null";
            
        string peopleJson = surrounding_people != null && surrounding_people.Count > 0
            ? $"\"surrounding_people\": [{string.Join(", ", surrounding_people.Select(p => $"{{\"name\": \"{p.name.ToLower().Replace(' ', '_')}\", \"health_status\": \"{p.health_status}\"}}"))}]"
            : "\"surrounding_people\": []";
            
        string regionsJson = neighbor_regions != null && neighbor_regions.Count > 0
            ? $"\"neighbor_regions\": [{string.Join(", ", neighbor_regions.Select(r => $"{{\"id\": \"{r.id}\", \"description\": \"{r.description}\", \"distance\": \"{r.distance}m\", \"direction\": \"{r.direction}\"}}"))}]"
            : "\"neighbor_regions\": []";
            
        string interestPointsJson = interest_points != null && interest_points.Count > 0
            ? $"\"interest_points\": [{string.Join(", ", interest_points.Select(ip => $"{{\"id\": \"{ip.id}\", \"description\": \"{ip.description}\", \"distance\": \"{ip.distance}m\", \"occupant\": \"{ip.occupant}\"}}"))}]"
            : "\"interest_points\": []";
            
        string conversationsJson = surrounding_conversation != null && surrounding_conversation.Count > 0
            ? $"\"conversations\": [{string.Join(", ", surrounding_conversation.Select(d => $"{{\"speaker\": \"{d.speaker}\", \"content\": \"{d.content}\"}}"))}]"
            : "\"conversations\": []";
            
        string pendingEventsJson = pending_events != null && pending_events.Count > 0
            ? $"\"events\": [{string.Join(", ", pending_events.Select(e => $"{{\"event_type\": \"{e.eventType}\", \"description\": \"{e.description}\"}}"))}]"
            : "\"events\": []";
            
        string locationJson = location != null
            ? $"\"current_location\": {{\"id\": \"{location.regionId}\", \"description\": \"{location.regionDescription}\"}}"
            : "\"current_location\": null";
            
        string availableActionIdsJson = available_action_ids != null && available_action_ids.Count > 0
            ? $"\"available_action_ids\": [{string.Join(", ", available_action_ids.Select(id => $"\"{id}\""))}]"
            : "\"available_action_ids\": []";
            
        return "{\n" +
               $"  {locationJson},\n" +
               $"  \"mood\": \"{mood}\",\n" +
               $"  \"current_movement_state\": \"{current_movement_state}\",\n" +
               $"  \"isHiding\": {isHiding.ToString().ToLower()},\n" +
               $"  {shooterJson},\n" +
               $"  {peopleJson},\n" +
               $"  {regionsJson},\n" +
               $"  {interestPointsJson},\n" +
               $"  {conversationsJson},\n" +
               $"  {pendingEventsJson},\n" +
               $"  {availableActionIdsJson}\n" +
               "}";
    }

    public string ToMarkdownString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Current Observation");
        sb.AppendLine($"**Mood:** {mood}");
        sb.AppendLine($"**Movement State:** {current_movement_state}");
        
        if (location != null)
        {
            sb.AppendLine("\n## Current Location");
            sb.AppendLine($"**Region ID:** {location.regionId}");
            sb.AppendLine($"**Description:** {location.regionDescription}");
        }
        
        if (shooter_info != null)
        {
            sb.AppendLine("\n## Shooter Information");
            sb.AppendLine($"**Location:** {shooter_info.regionId}");
            sb.AppendLine($"**Distance:** {shooter_info.distance} meters");
            sb.AppendLine($"**In Line of Sight:** {(shooter_info.isInLineOfSight ? "Yes" : "No")}");
            sb.AppendLine($"**Direction:** {shooter_info.direction}");
            sb.AppendLine($"**I'm Currently Hiding:** {(isHiding ? "Yes" : "No")}");
        }
        
        if (surrounding_people != null && surrounding_people.Count > 0)
        {
            sb.AppendLine("\n## People Nearby");
            foreach (var person in surrounding_people)
            {
                sb.AppendLine($"- **{person.name}** - {person.health_status}");
            }
        }
        
        if (neighbor_regions != null && neighbor_regions.Count > 0)
        {
            sb.AppendLine("\n## Neighboring Regions");
            foreach (var region in neighbor_regions)
            {
                sb.AppendLine($"- **{region.id}**: {region.description} ({region.distance}m {region.direction})");
            }
        }
        
        if (interest_points != null && interest_points.Count > 0)
        {
            sb.AppendLine("\n## Points of Interest");
            foreach (var point in interest_points)
            {
                sb.AppendLine($"- **{point.id}**: {point.description} ({point.distance}m) {(string.IsNullOrEmpty(point.occupant) ? "" : $"- Occupied by {point.occupant}")}");
            }
        }
        
        if (surrounding_conversation != null && surrounding_conversation.Count > 0)
        {
            sb.AppendLine("\n## Nearby Conversations");
            foreach (var conversation in surrounding_conversation)
            {
                sb.AppendLine($"- **{conversation.speaker}:** \"{conversation.content}\"");
            }
        }
        
        if (pending_events != null && pending_events.Count > 0)
        {
            sb.AppendLine("\n## Recent Events");
            foreach (var evt in pending_events)
            {
                sb.AppendLine($"- **{evt.eventType}**: {evt.description}");
            }
        }
        
        if (available_action_ids != null && available_action_ids.Count > 0)
        {
            sb.AppendLine("\n## Available Actions");
            foreach (var action in available_action_ids)
            {
                sb.AppendLine($"- {action}");
            }
        }
        
        return sb.ToString();
    }
}

[System.Serializable]
public class ShooterInfo
{
    public string regionId;
    public string distance;
    public bool isInLineOfSight;
    public string direction;
    public bool isInSameRegion;
}

[System.Serializable]
public class NeighborRegions
{
    public string id;
    public int distance;
    public string description;
    public string direction;
}

[System.Serializable]
public enum HealthStatus
{
    Alive,
    Injured,
    Dead
}

[System.Serializable]
public class SurroundingPeople
{
    public string name;
    public HealthStatus health_status;
}

[System.Serializable]
public class SurroundingDialogues
{
    public string content;
    public string speaker;
    public float time;
    
    public override string ToString()
    {
        return $"{speaker}: \"{content}\"";
    }
}

[System.Serializable]
public class SurroundingInterestPoints
{
    public string id;
    public InterestPointType type;
    public string description;
    public int distance;
    public string occupant;
}


public class Action
{
    public ActionType actionType;
    public Vector3? targetLocation;
    public string dialogText;
    public MovementState movementState;

    public override string ToString()
    {
        return $"ActionType: {actionType}, TargetLocation: {targetLocation}, DialogText: {dialogText}, MovementState: {movementState}";
    }
}

public enum ActionType
{
    MoveToRegion,
    MoveToPerson,
    MoveToHideSpot,
    MoveToExit,
    StayStill,
    FightShooter
}

[System.Serializable]
public class PendingEvent
{
    public string eventType;  // e.g., "gunshot", "shooter_entered", "got_shot"
    public string description;
    public DateTimeOffset timestamp;
    
    public PendingEvent(string eventType, string description)
    {
        this.eventType = eventType;
        this.description = description;
        this.timestamp = DateTimeOffset.UtcNow;
    }
}


public enum TrainingLevel 
{
    High, 
    Low 
}

public enum FamiliarityLevel 
{ 
    High, 
    Low 
}

public enum ShooterPerceptionLevel 
{ 
    Unaware, 
    Vague, 
    Direct 
}