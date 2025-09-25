using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }
    
    [Header("Simulation Settings")]
    public bool isSimulationRunning = false;
    [Tooltip("How often to log positions (in seconds)")]
    public float positionLogInterval = 0.3f; // Log positions every second by default
    
    // Add an event that will be triggered when the simulation ends
    [HideInInspector]
    public UnityEvent onSimulationEnded = new UnityEvent();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize random seed if specified
            if (SimConfig.UseRandomSeed)
            {
                Random.InitState(SimConfig.RandomSeed);
                Debug.Log($"Random seed initialized to: {SimConfig.RandomSeed}");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Make sure SimulationLogger exists
        if (SimulationLogger.Instance == null)
        {
            gameObject.AddComponent<SimulationLogger>();
        }
        
        // Start the simulation
        StartSimulation();
    }
    
    public void StartSimulation()
    {
        isSimulationRunning = true;
        
        // Optional: Start a timer to end the simulation after a set duration
        if (SimConfig.SimulationDuration > 0)
        {
            StartCoroutine(EndSimulationAfterDelay(SimConfig.SimulationDuration));
        }
    }
    
    public void EndSimulation()
    {
        if (!isSimulationRunning) return;
        
        isSimulationRunning = false;
        
        // Save all agent logs if logging is enabled
        if (SimulationLogger.Instance != null && SimConfig.LoggingEnabled)
        {
            SimulationLogger.Instance.EndSimulation();
        }
        
        Debug.Log("Simulation ended");
        
        // Invoke the event to notify listeners that the simulation has ended
        onSimulationEnded.Invoke();
        
        // Add this to actually stop the simulation if not part of an ablation study
        if (!SimConfig.IsDoingExperiment && !SimConfig.IsOptimizationRun)
        {
            #if UNITY_EDITOR
            // In editor, we can stop play mode
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            // In a build, we can quit the application
            Application.Quit();
            #endif
        }
    }
    
    private IEnumerator EndSimulationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndSimulation();
    }
    
    // Get the current simulation folder
    public string GetCurrentSimulationFolder()
    {
        if (SimulationLogger.Instance != null)
        {
            return SimulationLogger.Instance.GetCurrentSimulationFolder();
        }
        return null;
    }
}