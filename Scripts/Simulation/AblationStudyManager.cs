using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AblationStudyManager : MonoBehaviour
{
    [Header("Ablation Study Settings")]
    public string ablationStudyId = "Ablation";
    public bool runAblationStudy = true;
    public float defaultSimulationDuration = 10f;
    
    [Header("Experiment Configurations")]
    public List<ExperimentConfig> experimentConfigs = new List<ExperimentConfig>();
    
    private string ablationStudyFolderPath;
    private int currentExperimentIndex = 0;
    private bool isRunningExperiment = false;
    
    // Make this static so it persists across scene loads
    private static AblationStudyManager _instance;
    
    [System.Serializable]
    public class ExperimentConfig
    {
        public string experimentName;
        [Tooltip("Behavior enforcement mode for this experiment")]
        public SimConfig.BehaviorEnforcementMode behaviorEnforcing;
        [Tooltip("Random seed for this experiment")]
        public int randomSeed = 100;
        [Tooltip("Duration of the simulation in seconds")]
        public float simulationDuration = 20f;
    }
    
    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // Add default experiment configurations if none are defined
        if (experimentConfigs.Count == 0)
        {
            // Add ExplicitEnforcing experiment
            // experimentConfigs.Add(new ExperimentConfig
            // {
            //     experimentName = "NoEnforcing",
            //     behaviorEnforcing = SimConfig.BehaviorEnforcementMode.NoEnforcing,
            //     randomSeed = 42,
            //     simulationDuration = defaultSimulationDuration
            // });
            // experimentConfigs.Add(new ExperimentConfig
            // {
            //     experimentName = "ExplicitEnforcing",
            //     behaviorEnforcing = SimConfig.BehaviorEnforcementMode.Explicit,
            //     randomSeed = 42,
            //     simulationDuration = defaultSimulationDuration
            // });

            experimentConfigs.Add(new ExperimentConfig
            {
                experimentName = "ImplicitEnforcing",
                behaviorEnforcing = SimConfig.BehaviorEnforcementMode.Implicit,
                randomSeed = 42,
                simulationDuration = defaultSimulationDuration
            });
        }
        
        if (runAblationStudy && experimentConfigs.Count > 0)
        {
            // Create the main ablation study folder
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            ablationStudyFolderPath = Path.Combine(Application.persistentDataPath, "AblationStudies", $"{ablationStudyId}_{timestamp}");
            Directory.CreateDirectory(ablationStudyFolderPath);
            
            // Start the first experiment
            StartNextExperiment();
        }
        else
        {
            Debug.LogWarning("Ablation study not started. Either runAblationStudy is false or no experiment configs are defined.");
        }
    }
    
    private void StartNextExperiment()
    {
        if (currentExperimentIndex < experimentConfigs.Count)
        {
            ExperimentConfig config = experimentConfigs[currentExperimentIndex];
            Debug.Log($"Starting experiment {currentExperimentIndex + 1}/{experimentConfigs.Count}: {config.experimentName}");
            
            // Create experiment folder
            string experimentFolderPath = Path.Combine(ablationStudyFolderPath, config.experimentName);
            Directory.CreateDirectory(experimentFolderPath);
            
            // Clear any existing experiment path from PlayerPrefs
            PlayerPrefs.DeleteKey("CurrentExperimentPath");
            
            // Set the new experiment path
            PlayerPrefs.SetString("CurrentExperimentPath", experimentFolderPath);
            PlayerPrefs.Save(); // Force save to ensure it's persisted
            
            Debug.Log($"Set experiment path to: {experimentFolderPath}");
            
            // Configure simulation settings
            SimConfig.BehaviorEnforcing = config.behaviorEnforcing;
            SimConfig.RandomSeed = config.randomSeed;
            SimConfig.SimulationDuration = config.simulationDuration;
            
            // Reset simulation state using SimController's method
            SimController.ResetSimulationState();
            
            // Load the simulation scene
            isRunningExperiment = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(2); // Load the simulation scene (index 2)
        }
        else
        {
            Debug.Log("All experiments completed!");
            // Optionally quit the application when all experiments are done
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Verify the experiment path is still set correctly
        string currentPath = PlayerPrefs.GetString("CurrentExperimentPath", "");
        Debug.Log($"Current experiment path after scene load: {currentPath}");
        
        // Find the SimulationManager in the loaded scene
        SimulationManager simulationManager = FindObjectOfType<SimulationManager>();
        if (simulationManager != null)
        {
            // Configure the SimulationManager to notify us when the simulation ends
            simulationManager.onSimulationEnded.RemoveAllListeners(); // Clear previous listeners
            simulationManager.onSimulationEnded.AddListener(OnExperimentCompleted);
            
            // Make sure the simulation is started
            simulationManager.StartSimulation();
            Debug.Log($"Simulation started with duration: {SimConfig.SimulationDuration}s");
        }
        else
        {
            Debug.LogError("SimulationManager not found in the loaded scene!");
            OnExperimentCompleted(); // Move to next experiment anyway
        }
    }
    
    private void OnExperimentCompleted()
    {
        if (!isRunningExperiment) return;
        
        Debug.Log($"Experiment {experimentConfigs[currentExperimentIndex].experimentName} completed");
        isRunningExperiment = false;
        currentExperimentIndex++;
        
        // Wait a moment before starting the next experiment
        StartCoroutine(StartNextExperimentAfterDelay(1f));
    }
    
    private IEnumerator StartNextExperimentAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextExperiment();
    }
} 