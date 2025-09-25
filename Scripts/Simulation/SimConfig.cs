using UnityEngine;

/// <summary>
/// Central configuration class for simulation-wide constants and settings.
/// This allows for easier maintenance and configuration of global parameters.
/// 
/// NOTE: This is a static configuration class. To change values, edit this file directly.
/// These settings are not exposed to the Unity Inspector.
/// </summary>
public static class SimConfig
{
    // Behavior Enforcement Configuration
    public enum BehaviorEnforcementMode
    {
        NoEnforcing,  // Agents make their own decisions without enforced behavior patterns
        Implicit,    // Use agent settings distribution (training, familiarity, perception)
        Explicit     // Use explicit behavior distribution (run, hide, freeze, etc.)
    }
    
    public static BehaviorEnforcementMode BehaviorEnforcing = BehaviorEnforcementMode.NoEnforcing;
    
    // Random Seed Configuration
    public static int RandomSeed = 42; // Default seed value
    public static bool UseRandomSeed = true; // Set to true to use the specified seed
    
    // Simulation Configuration
    public static float SimulationDuration = 60f; // Duration of the simulation in seconds
    public static bool IsDoingExperiment = false;
    public static bool IsOptimizationRun = false;
    
    // AI Configuration
    public static string DefaultLLMModel = "google/gemini-2.5-flash-preview";
    // public static string DefaultLLMModel = "deepseek/deepseek-chat";
    public static float LLMTemperature = 0f;
    public static int ConversationTurnLimit = 3;  // Maximum number of turns beteen user and assistant, 1 turn is back and forth
    
    // Agent Configuration
    public static int DefaultAgentHealth = 3;
    public static bool UseFixedInitPersonas = true; // New setting to control persona initialization
    
    // Movement speeds for different states
    public static float StayStillSpeed = 0f;
    public static float WalkSpeed = 2.5f;
    public static float SprintSpeed = 5f;
    
    // NavMesh agent settings
    public static float AgentRadius = 0.3f;
    public static int AgentAvoidancePriority = 50;
    
    // Target location settings
    public static float TargetReachTimeout = 10.0f;
    public static float TargetReachThreshold = 0.5f;
    
    // Observation Configuration
    public static float NearbyPeopleRadius = 3.0f;
    public static int ConversationLimit = 5;
    
    // Logging Configuration
    public static string ChatLogsFileName = "ChatLogsRaw.txt";
    public static float PositionLogInterval = 0.5f; // log position every 0.5 second
    public static bool LoggingEnabled = true; // New setting to enable/disable logging
}