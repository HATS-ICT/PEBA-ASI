using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;

public class SimulationLogger : MonoBehaviour
{
    public static SimulationLogger Instance { get; private set; }
    
    private Dictionary<string, AgentLogger> agentLoggers = new Dictionary<string, AgentLogger>();
    private string simulationFolderPath;
    private string agentLogsFolderPath; // New dedicated folder for agent logs
    private DateTime simulationStartTime;
    
    private List<LoggedPosition> shooterTrajectory = new List<LoggedPosition>();
    private List<LoggedPosition> playerTrajectory = new List<LoggedPosition>();
    private float nextLogTime = 0f;
    private GameObject shooter;
    private GameObject player;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Only initialize the simulation folder if logging is enabled
        if (SimConfig.LoggingEnabled)
        {
            InitializeLoggingFolders();
        }
    }
    
    private void InitializeLoggingFolders()
    {
        // Check if we're running as part of an ablation study
        if (SimConfig.IsDoingExperiment || SimConfig.IsOptimizationRun)
        {
            // Use the experiment path provided by the AblationStudyManager
            simulationFolderPath = PlayerPrefs.GetString("CurrentExperimentPath", "");
            Debug.Log($"SimulationLogger using experiment path: {simulationFolderPath}");
        }
        else
        {
            // Initialize the simulation folder as usual
            simulationStartTime = DateTime.Now;
            string simulationFolderName = $"Simulation_{simulationStartTime:yyyy-MM-dd_HH-mm-ss}";
            simulationFolderPath = Path.Combine(Application.persistentDataPath, "SimulationLogs", simulationFolderName);
        }
        
        // Create a dedicated subfolder for agent logs
        agentLogsFolderPath = Path.Combine(simulationFolderPath, "AgentLogs");
        
        // Create the directories
        Directory.CreateDirectory(simulationFolderPath);
        Directory.CreateDirectory(agentLogsFolderPath);
    }
    
    private void Start()
    {
        // Find the shooter and player
        shooter = GameObject.FindGameObjectWithTag("Shooter");
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Start logging positions
        StartCoroutine(LogPositionsRoutine());
    }
    
    private IEnumerator LogPositionsRoutine()
    {
        while (true)
        {
            if (SimulationManager.Instance.isSimulationRunning && SimConfig.LoggingEnabled)
            {
                LogShooterPosition();
                LogPlayerPosition();
            }
            
            // Wait for the next log interval
            yield return new WaitForSeconds(SimulationManager.Instance.positionLogInterval);
        }
    }
    
    private void LogShooterPosition()
    {
        if (shooter != null)
        {
            Transform shooterTransform = shooter.transform;
            shooterTrajectory.Add(new LoggedPosition
            {
                time = Time.time,
                x = shooterTransform.position.x,
                y = shooterTransform.position.y,
                z = shooterTransform.position.z,
                rotation_x = shooterTransform.forward.x,
                rotation_y = shooterTransform.forward.y,
                rotation_z = shooterTransform.forward.z
            });
        }
    }
    
    private void LogPlayerPosition()
    {
        if (player != null)
        {
            Transform playerTransform = player.transform;
            playerTrajectory.Add(new LoggedPosition
            {
                time = Time.time,
                x = playerTransform.position.x,
                y = playerTransform.position.y,
                z = playerTransform.position.z,
                rotation_x = playerTransform.forward.x,
                rotation_y = playerTransform.forward.y,
                rotation_z = playerTransform.forward.z
            });
        }
    }
    
    // Get or create a logger for an agent
    public AgentLogger GetLogger(string agentName, Persona persona)
    {
        if (!agentLoggers.ContainsKey(agentName))
        {
            agentLoggers[agentName] = new AgentLogger(agentName, persona);
        }
        return agentLoggers[agentName];
    }
    
    // Save all agent logs
    public void SaveAllLogs()
    {
        if (!SimConfig.LoggingEnabled)
        {
            Debug.Log("Logging is disabled. Skipping log saving.");
            return;
        }
        
        if (agentLoggers.Count == 0)
        {
            Debug.LogWarning("No agent loggers registered. Nothing to save.");
        }
        
        // Ensure the agent logs directory exists
        if (!Directory.Exists(agentLogsFolderPath))
        {
            Directory.CreateDirectory(agentLogsFolderPath);
        }
        
        int successCount = 0;
        foreach (var entry in agentLoggers)
        {
            try
            {
                entry.Value.SaveToFile(agentLogsFolderPath);
                successCount++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save log for agent {entry.Key}: {ex.Message}");
            }
        }
        
        // Save dialog history
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnSimulationEnd();
        }
        
        // Save simulation metadata
        SaveSimulationMetadata();
        
        // Save map data (regions and interest points)
        SaveMapData();
        
        // Save shooter and player trajectories
        SaveShooterTrajectory();
        SavePlayerTrajectory();
        
        // Save personas to JSON file
        SavePersonasToJson();
    }
    
    // Save personas to a JSON file
    private void SavePersonasToJson()
    {
        try
        {
            string personasPath = Path.Combine(simulationFolderPath, "personas.json");
            
            // Use the InitPersonas class to save the personas
            InitPersonas.SavePersonasToJson(personasPath);
            
            Debug.Log($"Saved personas to {personasPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save personas: {ex.Message}");
        }
    }
    
    // Save metadata about the simulation
    private void SaveSimulationMetadata()
    {
        try
        {
            string metadataPath = Path.Combine(simulationFolderPath, "simulation_metadata.json");
            
            // Get the VictimTraitDistributor to access its weights
            VictimTraitDistributor traitDistributor = FindObjectOfType<VictimTraitDistributor>();
            
            var metadata = new
            {
                start_time = simulationStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                end_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                duration_seconds = (DateTime.Now - simulationStartTime).TotalSeconds,
                agent_count = agentLoggers.Count,
                unity_version = Application.unityVersion,
                platform = Application.platform.ToString(),
                agent_logs_folder = agentLogsFolderPath,
                
                // Add LLM usage statistics
                llm_usage = new
                {
                    model = OpenAIUtils.model,
                    prompt_tokens = OpenAIUtils.totalTokens["prompt_tokens"],
                    cached_tokens = OpenAIUtils.totalTokens["cached_tokens"],
                    completion_tokens = OpenAIUtils.totalTokens["completion_tokens"],
                    total_requests = OpenAIUtils.totalRequests,
                    total_cost_usd = OpenAIUtils.ComputeTokenCost()
                },
                
                // Add SimConfig settings
                sim_config = new
                {
                    behavior_enforcing = SimConfig.BehaviorEnforcing.ToString(),
                    random_seed = SimConfig.RandomSeed,
                    use_random_seed = SimConfig.UseRandomSeed,
                    simulation_duration = SimConfig.SimulationDuration,
                    default_llm_model = SimConfig.DefaultLLMModel,
                    llm_temperature = SimConfig.LLMTemperature,
                    position_log_interval = SimConfig.PositionLogInterval,
                    logging_enabled = SimConfig.LoggingEnabled
                },
                
                // Add trait and behavior distributions if available
                trait_distributions = traitDistributor != null ? GetTraitDistributions(traitDistributor) : null,
                behavior_distributions = traitDistributor != null ? GetBehaviorDistributions(traitDistributor) : null
            };
            
            string json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            File.WriteAllText(metadataPath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save simulation metadata: {ex.Message}");
        }
    }
    
    // Helper method to get trait distributions data
    private List<object> GetTraitDistributions(VictimTraitDistributor distributor)
    {
        var traitDistributions = new List<object>();
        
        try
        {
            // Get trait weights using the public method
            float[] traitWeights = distributor.GetTraitWeights();
            
            // Access the settingsDistributions field using reflection (since it's private)
            var settingsField = typeof(VictimTraitDistributor).GetField("settingsDistributions", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (settingsField != null)
            {
                var settings = settingsField.GetValue(distributor) as VictimTraitDistributor.VictimTraitProfile[];
                
                if (settings != null)
                {
                    for (int i = 0; i < settings.Length; i++)
                    {
                        var setting = settings[i];
                        traitDistributions.Add(new
                        {
                            training_level = setting.trainingLevel.ToString(),
                            familiarity_level = setting.familiarityLevel.ToString(),
                            shooter_perception_level = setting.shooterPerceptionLevel.ToString(),
                            weight = setting.weight,
                            calculated_count = setting.calculatedCount
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get trait distributions: {ex.Message}");
        }
        
        return traitDistributions;
    }
    
    // Helper method to get behavior distributions data
    private List<object> GetBehaviorDistributions(VictimTraitDistributor distributor)
    {
        var behaviorDistributions = new List<object>();
        
        try
        {
            // Get behavior weights using the public method
            float[] behaviorWeights = distributor.GetBehaviorWeights();
            
            // Access the behaviorDistributions field using reflection (since it's private)
            var behaviorsField = typeof(VictimTraitDistributor).GetField("behaviorDistributions", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (behaviorsField != null)
            {
                var behaviors = behaviorsField.GetValue(distributor) as VictimTraitDistributor.BehaviorDistribution[];
                
                if (behaviors != null)
                {
                    for (int i = 0; i < behaviors.Length; i++)
                    {
                        var behavior = behaviors[i];
                        behaviorDistributions.Add(new
                        {
                            behavior_type = behavior.behaviorType,
                            instruction = behavior.instruction,
                            weight = behavior.weight,
                            calculated_count = behavior.calculatedCount
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get behavior distributions: {ex.Message}");
        }
        
        return behaviorDistributions;
    }
    
    // Save map data including regions and interest points
    private void SaveMapData()
    {
        try
        {
            string mapDataPath = Path.Combine(simulationFolderPath, "map_data.json");
            
            // Create a dictionary to hold both types of data
            var mapData = new Dictionary<string, object>();
            
            // Add regions data
            mapData["regions"] = GetRegionsData();
            
            // Add interest points data
            mapData["interest_points"] = GetInterestPointsData();
            
            // Add region connections data
            mapData["region_connections"] = GetRegionConnectionData();
            
            // Save to file
            string json = JsonConvert.SerializeObject(mapData, Formatting.Indented);
            File.WriteAllText(mapDataPath, json);
            
            // Debug.Log($"Saved map data to {mapDataPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save map data: {ex.Message}");
        }
    }
    
    // Get data about all regions in the scene
    private List<object> GetRegionsData()
    {
        var regionsData = new List<object>();
        
        try
        {
            Region[] allRegions = FindObjectsByType<Region>(FindObjectsSortMode.None);
            
            foreach (Region region in allRegions)
            {
                BoxCollider boxCollider = region.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    var regionData = new
                    {
                        id = region.regionId,
                        description = region.regionDescription,
                        position = new
                        {
                            x = region.transform.position.x,
                            y = region.transform.position.y,
                            z = region.transform.position.z
                        },
                        bounds = new
                        {
                            center = new
                            {
                                x = boxCollider.bounds.center.x,
                                y = boxCollider.bounds.center.y,
                                z = boxCollider.bounds.center.z
                            },
                            size = new
                            {
                                x = boxCollider.bounds.size.x,
                                y = boxCollider.bounds.size.y,
                                z = boxCollider.bounds.size.z
                            }
                        }
                    };
                    
                    regionsData.Add(regionData);
                }
            }
            
            // Debug.Log($"Collected data for {regionsData.Count} regions");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to collect regions data: {ex.Message}");
        }
        
        return regionsData;
    }
    
    // Get data about all interest points in the scene
    private List<object> GetInterestPointsData()
    {
        var interestPointsData = new List<object>();
        
        try
        {
            // Find all hiding spots and exit points
            GameObject[] hidingPlaces = GameObject.FindGameObjectsWithTag("HidingPlace");
            GameObject[] exitPlaces = GameObject.FindGameObjectsWithTag("ExitPlace");
            
            // Process hiding spots
            foreach (GameObject hidingPlace in hidingPlaces)
            {
                InterestPoint point = hidingPlace.GetComponent<InterestPoint>();
                if (point != null)
                {
                    var pointData = new
                    {
                        id = point.id,
                        type = point.type.ToString(),
                        description = point.description,
                        occupant = point.occupant,
                        position = new
                        {
                            x = hidingPlace.transform.position.x,
                            y = hidingPlace.transform.position.y,
                            z = hidingPlace.transform.position.z
                        }
                    };
                    
                    interestPointsData.Add(pointData);
                }
            }
            
            // Process exit points
            foreach (GameObject exitPlace in exitPlaces)
            {
                InterestPoint point = exitPlace.GetComponent<InterestPoint>();
                if (point != null)
                {
                    var pointData = new
                    {
                        id = point.id,
                        type = point.type.ToString(),
                        description = point.description,
                        position = new
                        {
                            x = exitPlace.transform.position.x,
                            y = exitPlace.transform.position.y,
                            z = exitPlace.transform.position.z
                        }
                    };
                    
                    interestPointsData.Add(pointData);
                }
            }
            
            // Debug.Log($"Collected data for {interestPointsData.Count} interest points");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to collect interest points data: {ex.Message}");
        }
        
        return interestPointsData;
    }
    
    // Get data about region connections
    private List<object> GetRegionConnectionData()
    {
        var connectionData = new List<object>();
        
        try
        {
            // Get all regions in the scene
            Region[] allRegions = FindObjectsByType<Region>(FindObjectsSortMode.None);
            Dictionary<string, Vector3> regionCenters = new Dictionary<string, Vector3>();
            
            // First, create a dictionary of region IDs to their center positions
            foreach (Region region in allRegions)
            {
                BoxCollider boxCollider = region.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    regionCenters[region.regionId] = boxCollider.bounds.center;
                }
            }
            
            // Now create connection data using the Region.regionConnectivity dictionary
            foreach (var entry in Region.regionConnectivity)
            {
                string sourceRegionId = entry.Key;
                
                // Skip if we don't have position data for this region
                if (!regionCenters.ContainsKey(sourceRegionId))
                    continue;
                    
                Vector3 sourcePos = regionCenters[sourceRegionId];
                
                // Process each connected region
                foreach (string targetRegionId in entry.Value)
                {
                    // Skip if we don't have position data for the target region
                    if (!regionCenters.ContainsKey(targetRegionId))
                        continue;
                        
                    Vector3 targetPos = regionCenters[targetRegionId];
                    
                    // Create a connection object
                    var connection = new
                    {
                        source = sourceRegionId,
                        target = targetRegionId,
                        source_position = new
                        {
                            x = sourcePos.x,
                            y = sourcePos.y,
                            z = sourcePos.z
                        },
                        target_position = new
                        {
                            x = targetPos.x,
                            y = targetPos.y,
                            z = targetPos.z
                        }
                    };
                    
                    connectionData.Add(connection);
                }
            }
            
            // Debug.Log($"Collected data for {connectionData.Count} region connections");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to collect region connection data: {ex.Message}");
        }
        
        return connectionData;
    }
    
    // Save shooter trajectory to file
    private void SaveShooterTrajectory()
    {
        try
        {
            string filePath = Path.Combine(simulationFolderPath, "shooter_traj.json");
            string json = JsonConvert.SerializeObject(shooterTrajectory, Formatting.Indented);
            File.WriteAllText(filePath, json);
            // Debug.Log($"Saved shooter trajectory to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save shooter trajectory: {ex.Message}");
        }
    }
    
    // Save player trajectory to file
    private void SavePlayerTrajectory()
    {
        try
        {
            string filePath = Path.Combine(simulationFolderPath, "human_traj.json");
            string json = JsonConvert.SerializeObject(playerTrajectory, Formatting.Indented);
            File.WriteAllText(filePath, json);
            // Debug.Log($"Saved player trajectory to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save player trajectory: {ex.Message}");
        }
    }
    
    // Get the current simulation folder path
    public string GetCurrentSimulationFolder()
    {
        return simulationFolderPath;
    }
    
    // Get the agent logs folder path
    public string GetAgentLogsFolder()
    {
        return agentLogsFolderPath;
    }
    
    // Public method to manually save logs (e.g., when simulation ends but application doesn't quit)
    public void EndSimulation()
    {
        SaveAllLogs();
    }
}

// Update the LoggedPosition class to include rotation data
[Serializable]
public class LoggedPosition
{
    public float time;
    public float x;
    public float y;
    public float z;
    public float rotation_x;
    public float rotation_y;
    public float rotation_z;
}