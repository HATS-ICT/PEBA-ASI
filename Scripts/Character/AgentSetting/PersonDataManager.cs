using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
// When the shooter is present, run to exit as fast as possible.
public class PersonDataManager : MonoBehaviour
{
    public Persona persona;
    public Memory memory = new Memory { events = new List<MemoryEvent>() };
    public string currentMood = "neutral";
    public string agentSpecificInstruction = "";
    public string currentMovementState = "stay_still";
    
    public TrainingLevel trainingLevel = TrainingLevel.Low;
    public FamiliarityLevel familiarityLevel = FamiliarityLevel.Low;
    public ShooterPerceptionLevel shooterPerceptionLevel = ShooterPerceptionLevel.Direct;

    public static PersonaType defaultPersonaType = PersonaType.Office;

    public AgentLogger logger;

    public int health = 1;
    public string healthStatus = "Alive";

    public string prompt_template = "Persona: {persona}\n\nMemory: {memory}\n\nCurrent Observation: {observation}";

    private List<object> conversationHistory = new List<object>();
    
    private bool isDestroyed = false;

    private static List<int> assignedPersonaIndices = new List<int>();

    private void Start()
    {
        if (SimConfig.UseFixedInitPersonas)
        {
            AssignPersonaFromInitList();
        }
        else
        {
            persona = PersonaGenerator.GeneratePersona(PersonaGenerator.GenerationType.Random);
        }

        currentMood = "neutral";
        if (persona == null)
        {
            Debug.LogError("Persona is null");
        }

        memory.events = new List<MemoryEvent>();
        
        if (SimulationLogger.Instance != null && persona != null)
        {
            logger = SimulationLogger.Instance.GetLogger(persona.name, persona);
            
            logger.LogAgentTraits(trainingLevel, familiarityLevel, shooterPerceptionLevel);
        }
        else
        {
            Debug.LogError($"Failed to initialize logger: SimulationLogger.Instance={SimulationLogger.Instance}, persona={persona}");
        }
        
        InvokeRepeating("LogCurrentPosition", 0, SimConfig.PositionLogInterval);
    }

    private void AssignPersonaFromInitList()
    {
        List<Persona> personaList = defaultPersonaType == PersonaType.School ? 
            InitPersonas.SchoolPersonas : InitPersonas.OfficePersonas;

        if (assignedPersonaIndices.Count >= personaList.Count)
        {
            assignedPersonaIndices.Clear();
            Debug.Log($"All {defaultPersonaType} personas have been assigned. Resetting assignment tracking.");
        }
        
        int randomIndex;
        do
        {
            randomIndex = UnityEngine.Random.Range(0, personaList.Count);
        } while (assignedPersonaIndices.Contains(randomIndex));
        
        assignedPersonaIndices.Add(randomIndex);
        
        persona = personaList[randomIndex];
    }

    public static void SetDefaultPersonaType(PersonaType type)
    {
        defaultPersonaType = type;
        Debug.Log($"Default persona type set to: {type}");
        
        ClearAssignedPersonaIndices();
    }

    public static void ClearAssignedPersonaIndices()
    {
        assignedPersonaIndices.Clear();
    }

    private void LogCurrentPosition()
    {
        if (isDestroyed) return;
        
        if (logger != null)
        {
            Vector3 position = transform.position;
            Vector3 forward = transform.forward;
            logger.LogPosition(position, forward, health, healthStatus);
        }
    }

    public void UpdateMemory(string description)
    {
        if (isDestroyed) return;
        
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        memory.events.Add(new MemoryEvent { time = currentTime, description = description });
        
        if (logger != null)
        {
            logger.LogMemory(description);
        }
    }

    private string GetReadableTime(long unixTimestamp)
    {
        TimeSpan timeDiff = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);

        if (timeDiff.TotalMinutes < 1) return "just now";
        if (timeDiff.TotalMinutes < 60) return $"last {Math.Floor(timeDiff.TotalMinutes)} minutes";
        if (timeDiff.TotalHours < 24) return $"last {Math.Floor(timeDiff.TotalHours)} hours";
        return $"last {Math.Floor(timeDiff.TotalDays)} days";
    }

    public async Task<Action> PlanAction(Observation observation)
    {
        if (isDestroyed) return null;
        
        if (logger != null)
        {
            logger.LogObservation(observation);
        }
        
        bool observesShooter = SimController.shootingHasStarted && 
                               shooterPerceptionLevel != ShooterPerceptionLevel.Unaware;
        
        bool isWellTrained = trainingLevel == TrainingLevel.High;
        bool isFamiliar = familiarityLevel == FamiliarityLevel.High;
        bool enforceBehavior = !string.IsNullOrEmpty(agentSpecificInstruction);

        var navigationManager = GetComponent<NavigationManager>();
        Region currentRegion = navigationManager.currentRegion;
        
        string buildingMapString = Region.GetBuildingMap(currentRegion.regionId, transform.position);
        
        string systemPrompt = PromptFormatter.FormatSystemPrompt(
            observesShooter, 
            isWellTrained, 
            isFamiliar, 
            enforceBehavior, 
            agentSpecificInstruction,
            buildingMapString
        );
        
        string userPrompt = PromptFormatter.FormatUserPrompt(prompt_template, persona, memory, observation);
        
        try
        {
            string response = await OpenAIUtils.GetOneTimeResponse(
                systemPrompt,
                userPrompt,
                conversationHistory,
                persona.name
            );
            
            if (isDestroyed) return null;
            
            conversationHistory.Add(new { role = "user", content = userPrompt });
            conversationHistory.Add(new { role = "assistant", content = response });
            
            while (conversationHistory.Count > SimConfig.ConversationTurnLimit * 2)
            {
                conversationHistory.RemoveAt(0);
                conversationHistory.RemoveAt(0);
            }
            
            var jsonResponse = JsonConvert.DeserializeObject<ActionResponse>(response);

            string actionId = jsonResponse.action.action_id;
            
            if (!observation.available_action_ids.Contains(actionId))
            {
                Debug.LogWarning($"AI selected an unavailable action: {actionId}. Defaulting to stay_still.");
                actionId = "stay_still";
            }
            
            ActionType actionType;
            Vector3? targetLocation = null;
            
            if (actionId == "stay_still")
            {
                actionType = ActionType.StayStill;
            }
            else if (actionId == "fight_the_shooter")
            {
                actionType = ActionType.FightShooter;
                if (navigationManager != null) {
                    targetLocation = navigationManager.GetShooterLocation();
                    if (targetLocation == null)
                    {
                        Debug.LogError("Target shooter location is null");
                    }
                }
            }
            else if (Region.regionDescriptions.ContainsKey(actionId))
            {
                actionType = ActionType.MoveToRegion;
                if (navigationManager != null) {
                    targetLocation = navigationManager.GetRegionLocationByID(actionId, true);
                    if (targetLocation == null)
                    {
                        Debug.LogError("Target move_to_region is null");
                    }
                }
            }
            else if (actionId.StartsWith("hide_spot_"))
            {
                actionType = ActionType.MoveToHideSpot;
                if (navigationManager != null) {
                    targetLocation = navigationManager.GetInterestPointLocationByID(actionId);
                    if (targetLocation == null)
                    {
                        Debug.LogError("Target move_to_hide_spot is null");
                    }
                }
            }
            else if (actionId.StartsWith("exit_"))
            {
                actionType = ActionType.MoveToExit;
                if (navigationManager != null) {
                    targetLocation = navigationManager.GetInterestPointLocationByID(actionId);
                    if (targetLocation == null)
                    {
                        Debug.LogError("Target move_to_exit is null");
                    }
                }
            }
            else
            {
                actionType = ActionType.MoveToPerson;
                if (navigationManager != null) {
                    targetLocation = navigationManager.GetPersonLocationByNameID(actionId);
                    if (targetLocation == null)
                    {
                        Debug.LogError("Target move_to_person is null");
                    }
                }
            }

            if (isDestroyed) return null;

            currentMood = jsonResponse.update.mood;
            currentMovementState = jsonResponse.action.movement;
            if (actionId != "stay_still")
            {
                currentMovementState = "walk";
            }
            UpdateMemory(jsonResponse.update.memory);

            Action action = new Action
            {
                actionType = actionType,
                movementState = DetermineMovementState(currentMovementState),
                dialogText = jsonResponse.action.utterance,
                targetLocation = targetLocation
            };
            
            if (logger != null && !isDestroyed)
            {
                logger.LogAction(action, targetLocation, jsonResponse.thought);
            }
            
            return action;
        }
        catch (Exception e)
        {
            if (!isDestroyed)
            {
                Debug.LogError($"Failed to parse AI response: {e.Message}");
            }
            
            return new Action
            {
                actionType = ActionType.StayStill,
                dialogText = "",
                targetLocation = null
            };
        }
    }

    private MovementState DetermineMovementState(string movementState)
    {
        if (string.IsNullOrEmpty(movementState))
            return MovementState.StayStill;
            
        switch (movementState.ToLower())
        {
            case "stay_still":
                return MovementState.StayStill;
            case "walk":
                return MovementState.Walk;
            case "sprint":
                return MovementState.Sprint;
            default:
                return MovementState.StayStill;
        }
    }

    private class ActionResponse
    {
        public string thought { get; set; }
        public ActionDetails action { get; set; }
        public UpdateDetails update { get; set; }
    }

    private class ActionDetails
    {
        [JsonProperty("vocal_mode")]
        public string vocal_mode { get; set; }
        [JsonProperty("utterance")]
        public string utterance { get; set; }
        [JsonProperty("movement")]
        public string movement { get; set; }
        [JsonProperty("action_id")]
        public string action_id { get; set; }
    }

    private class UpdateDetails
    {
        [JsonProperty("mood")]
        public string mood { get; set; }
        [JsonProperty("memory")]
        public string memory { get; set; }
    }

    public void LogPosition(Vector3 position, Vector3 forward, int health, string healthStatus)
    {
        if (logger != null)
        {
            logger.LogPosition(position, forward, health, healthStatus);
        }
    }

    public void LogPosition(Vector3 position, Vector3 forward)
    {
        if (logger != null)
        {
            logger.LogPosition(position, forward, health, healthStatus);
        }
    }

    public void LogPosition(Vector3 position)
    {
        if (logger != null)
        {
            logger.LogPosition(position, transform.forward, health, healthStatus);
        }
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void ResetConversationHistory()
    {
        conversationHistory.Clear();
        isDestroyed = false;
    }
}

public enum PersonaType
{
    School,
    Office
}
