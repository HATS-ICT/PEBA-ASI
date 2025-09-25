using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq; // Required for Linq operations like Select
using System.Threading.Tasks;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;
using System.Text;

public class BehaviorOptimizer : MonoBehaviour
{
    [Header("Optimizer Settings")]
    public string optimizerId = "BehaviorOptimizer";
    public int numberOfIterations = 5;
    public int simulationSceneIndex = 2; // Ensure this is the correct index of your main simulation scene
    public float defaultSimulationDuration = 60f;
    public PersonaType personaType = PersonaType.Office;
    
    private const int EMPTY_SCENE_INDEX = 9; // Empty scene used for unloading simulation

    private string optimizerBaseFolderPath;
    private string currentIterationLogPath;
    private int currentIteration = 0;
    private bool isRunningIteration = false;
    private bool isEvaluating = false;

    private static BehaviorOptimizer _instance;

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

    private void Start()
    {
        Debug.Log("BehaviorOptimizer Started.");
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        optimizerBaseFolderPath = Path.Combine(Application.persistentDataPath, "OptimizationRuns", $"{optimizerId}_{timestamp}");
        Directory.CreateDirectory(optimizerBaseFolderPath);
        Debug.Log($"Optimizer base folder created at: {optimizerBaseFolderPath}");

        SimConfig.IsOptimizationRun = true; 
        StartNextIteration();
    }

    private void StartNextIteration()
    {
        if (currentIteration < numberOfIterations)
        {
            Debug.Log($"Starting iteration {currentIteration + 1}/{numberOfIterations}");

            currentIterationLogPath = Path.Combine(optimizerBaseFolderPath, $"Iteration_{currentIteration + 1}");
            Directory.CreateDirectory(currentIterationLogPath);
            
            PlayerPrefs.DeleteKey("CurrentExperimentPath"); 
            PlayerPrefs.SetString("CurrentExperimentPath", currentIterationLogPath);
            PlayerPrefs.Save();
            Debug.Log($"Set current iteration log path to: {currentIterationLogPath}");

            UpdateAgentSettings();

            SimConfig.SimulationDuration = defaultSimulationDuration;
            SimConfig.BehaviorEnforcing = SimConfig.BehaviorEnforcementMode.Implicit;
            // Ensure a consistent seed for comparable runs with different params, or vary it if intended.
            // SimConfig.RandomSeed = YourSeedChoice; // Example: can be fixed or iteration-dependent
            SimController.ResetSimulationState(); 

            isRunningIteration = true;
            SceneManager.sceneLoaded += OnSimulationSceneLoaded;
            SceneManager.LoadScene(simulationSceneIndex);
        }
        else
        {
            Debug.Log("All optimization iterations completed!");
            SimConfig.IsOptimizationRun = false; 
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }

    private void UpdateAgentSettings()
    {
        UnityEngine.Debug.Log("Mutating agent settings for the next iteration...");
        
        // Set the persona type for this optimization run
        PersonDataManager.SetDefaultPersonaType(personaType);
        UnityEngine.Debug.Log($"Set persona type for optimization: {personaType}");
        
        if (currentIteration == 0)
        {
            // First iteration - use default personas
            UnityEngine.Debug.Log("First iteration - using default personas");
            InitPersonas.SetJsonFilePath("");  // Reset to use default personas
        }
        else
        {
            // Subsequent iterations - run the Python optimizer and use updated personas
            string previousIterationPath = Path.Combine(optimizerBaseFolderPath, $"Iteration_{currentIteration}");
            string updatedPersonasPath = Path.Combine(previousIterationPath, "personas_updated.json");
            
            // Check if we need to run the optimizer
            if (!File.Exists(updatedPersonasPath))
            {
                UnityEngine.Debug.Log($"Running agent optimizer for iteration {currentIteration + 1}...");
                RunAgentOptimizer(previousIterationPath);
            }
            
            // Check if the updated personas file exists
            if (File.Exists(updatedPersonasPath))
            {
                UnityEngine.Debug.Log($"Using optimized personas from: {updatedPersonasPath}");
                InitPersonas.SetJsonFilePath(updatedPersonasPath);
                
                // Copy the updated personas to the next iteration folder for reference
                string nextIterationPath = Path.Combine(optimizerBaseFolderPath, $"Iteration_{currentIteration + 1}");
                Directory.CreateDirectory(nextIterationPath);
                File.Copy(updatedPersonasPath, Path.Combine(nextIterationPath, "personas_initial.json"), true);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Updated personas file not found at {updatedPersonasPath}. Using default personas.");
                InitPersonas.SetJsonFilePath("");  // Reset to use default personas
            }
        }
        
        // Reset the assigned persona indices to ensure fresh assignment for this iteration
        PersonDataManager.ClearAssignedPersonaIndices();
    }

    private void RunAgentOptimizer(string simulationFolderPath)
    {
        try
        {
            // Get the path to the Python script
            string pythonScriptPath = GetPythonScriptPath("rewrite_persona.py");
            if (string.IsNullOrEmpty(pythonScriptPath))
            {
                UnityEngine.Debug.LogError("Python script not found: rewrite_persona.py");
                return;
            }
            
            UnityEngine.Debug.Log($"Using Python script at: {pythonScriptPath}");
            
            // Extract the optimization run and iteration from the path
            string optimizationRunName = Path.GetFileName(Path.GetDirectoryName(simulationFolderPath));
            string iterationName = Path.GetFileName(simulationFolderPath);
            
            UnityEngine.Debug.Log($"Extracted optimization run: {optimizationRunName}, iteration: {iterationName}");
            
            // Create process to run the Python script
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = GetPythonExecutablePath();
            startInfo.Arguments = $"\"{pythonScriptPath}\" --run \"{optimizationRunName}\" --iteration \"{iterationName}\"";
            
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            
            UnityEngine.Debug.Log($"Running command: {startInfo.FileName} {startInfo.Arguments}");
            
            Process process = new Process();
            process.StartInfo = startInfo;
            
            // Capture output
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();
            
            process.OutputDataReceived += (sender, e) => {
                if (e.Data != null)
                {
                    output.AppendLine(e.Data);
                    UnityEngine.Debug.Log($"Python: {e.Data}");
                }
            };
            
            process.ErrorDataReceived += (sender, e) => {
                if (e.Data != null)
                {
                    error.AppendLine(e.Data);
                    
                    // Check if this is a progress bar output
                    if (e.Data.Contains("%") || e.Data.Contains("it/s"))
                    {
                        // UnityEngine.Debug.Log($"Python Progress: {e.Data}");
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"Python Error: {e.Data}");
                    }
                }
            };
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            
            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"Python script exited with code {process.ExitCode}. Error: {error}");
            }
            else
            {
                UnityEngine.Debug.Log("Agent optimizer completed successfully.");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error running agent optimizer: {ex.Message}");
        }
    }

    private string GetPythonScriptPath(string scriptName)
    {
        // Try to find the Python script in the project directory
        string[] possibleLocations = new string[]
        {
            // In the root of the project
            Path.Combine(Application.dataPath, "..", scriptName),
            // In the Scripts folder
            Path.Combine(Application.dataPath, "Scripts", scriptName),
            // In the Python folder
            Path.Combine(Application.dataPath, "Python", scriptName),
            // In the StreamingAssets folder
            Path.Combine(Application.streamingAssetsPath, scriptName)
        };
        
        foreach (string path in possibleLocations)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }
        
        return null;
    }

    private string GetPythonExecutablePath()
    {
        // Use the specific virtual environment python executable
        if (Application.platform == RuntimePlatform.WindowsEditor || 
            Application.platform == RuntimePlatform.WindowsPlayer)
        {
            return @"path\to\python.exe";
        }
        else
        {
            // For macOS and Linux, you might need to adjust this path accordingly
            return "python3"; // Default fallback for non-Windows platforms
        }
    }

    private void OnSimulationSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSimulationSceneLoaded;
        Debug.Log($"Simulation scene {scene.name} loaded for iteration {currentIteration + 1}.");
        
        string pathCheck = PlayerPrefs.GetString("CurrentExperimentPath", "NOT_SET");
        Debug.Log($"Log path in PlayerPrefs after scene load: {pathCheck}");

        SimulationManager simulationManager = FindObjectOfType<SimulationManager>();
        if (simulationManager != null)
        {
            simulationManager.onSimulationEnded.RemoveAllListeners();
            simulationManager.onSimulationEnded.AddListener(() => OnIterationCompleted(false));
            simulationManager.StartSimulation();
            Debug.Log($"Simulation started. Duration: {SimConfig.SimulationDuration}s");
        }
        else
        {
            Debug.LogError("SimulationManager not found in the loaded scene! Aborting iteration.");
            OnIterationCompleted(true); 
        }
    }

    private async void OnIterationCompleted(bool errorOccurred = false)
    {
        if (!isRunningIteration || isEvaluating) return;
        
        isRunningIteration = false;
        isEvaluating = true;

        if (errorOccurred) {
            Debug.LogError($"Iteration {currentIteration + 1} completed with an error.");
        } else {
            Debug.Log($"Iteration {currentIteration + 1} completed successfully.");
        }
        
        // Load a blank/loading scene to unload the simulation scene
        Debug.Log("Unloading simulation scene before evaluation...");
        await UnloadSimulationScene();
        
        // Use the BehaviorEvaluator to evaluate the simulation
        Debug.Log($"Starting evaluation for iteration {currentIteration + 1}...");
        float score = await EvaluateSimulationData(currentIterationLogPath);
        Debug.Log($"Iteration {currentIteration + 1} - KL Divergence Score: {score}");
        
        // Save the score to a summary file
        // SaveIterationScore(currentIteration, score);
        
        currentIteration++;
        isEvaluating = false;
        
        // Use a coroutine to start the next iteration after a short delay
        StartCoroutine(StartNextIterationAfterDelay(1.0f));
    }

    private async Task<float> EvaluateSimulationData(string iterationLogPath)
    {
        Debug.Log($"Evaluating simulation data from: {iterationLogPath}");
        
        try
        {
            // Use the BehaviorEvaluator to analyze agent behaviors and calculate KL divergence
            float klDivergence = await BehaviorEvaluator.EvaluateSimulation(iterationLogPath);
            return klDivergence;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error evaluating simulation data: {ex.Message}");
            return float.MaxValue; // Return worst possible score on error
        }
    }

    private IEnumerator StartNextIterationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextIteration();
    }

    // Add this new method to handle scene unloading
    private async Task UnloadSimulationScene()
    {
        // Load a minimal scene (scene index 0 is typically the initialization/menu scene)
        // You can change this to whatever lightweight scene you prefer
        var asyncOperation = SceneManager.LoadSceneAsync(EMPTY_SCENE_INDEX);
        
        // Wait for the scene to finish loading
        while (!asyncOperation.isDone)
        {
            await Task.Yield();
        }
        
        // Give Unity a frame to settle after scene change
        await Task.Yield();
        
        Debug.Log("Simulation scene unloaded successfully.");
    }
} 