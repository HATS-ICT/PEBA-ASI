using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class BehaviorEvaluator
{
    private static readonly string pythonScriptName = "classify_behavior.py";
    
    /// <summary>
    /// Evaluates a simulation by running the Python behavior analysis script and returns the KL divergence score
    /// </summary>
    /// <param name="simulationFolderPath">Path to the simulation folder to evaluate</param>
    /// <returns>KL divergence score (lower is better)</returns>
    public static async Task<float> EvaluateSimulation(string simulationFolderPath)
    {
        UnityEngine.Debug.Log($"Evaluating simulation at path: {simulationFolderPath}");
        
        // Ensure the simulation folder exists
        if (!Directory.Exists(simulationFolderPath))
        {
            throw new DirectoryNotFoundException($"Simulation folder not found: {simulationFolderPath}");
        }
        
        // Get the path to the Python script
        string pythonScriptPath = GetPythonScriptPath();
        if (string.IsNullOrEmpty(pythonScriptPath))
        {
            throw new FileNotFoundException("Python script not found: " + pythonScriptName);
        }
        
        UnityEngine.Debug.Log($"Using Python script at: {pythonScriptPath}");
        
        // Run the Python script as a process
        float klDivergence = await RunPythonScript(pythonScriptPath, simulationFolderPath);
        
        return klDivergence;
    }
    
    private static string GetPythonScriptPath()
    {
        // Try to find the Python script in the project directory
        string[] possibleLocations = new string[]
        {
            // In the root of the project
            Path.Combine(Application.dataPath, "..", pythonScriptName),
            // In the Scripts folder
            Path.Combine(Application.dataPath, "Scripts", pythonScriptName),
            // In the Python folder
            Path.Combine(Application.dataPath, "Python", pythonScriptName),
            // In the StreamingAssets folder
            Path.Combine(Application.streamingAssetsPath, pythonScriptName)
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
    
    private static async Task<float> RunPythonScript(string scriptPath, string simulationFolderPath)
    {
        // Create a process to run the Python script
        ProcessStartInfo startInfo = new ProcessStartInfo();
        
        // Use python or python3 depending on the system
        startInfo.FileName = GetPythonExecutablePath();
        startInfo.Arguments = $"\"{scriptPath}\" --folder \"{simulationFolderPath}\" --direct-path";
        
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.CreateNoWindow = true;
        
        UnityEngine.Debug.Log($"Running command: {startInfo.FileName} {startInfo.Arguments}");
        
        Process process = new Process();
        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;
        
        TaskCompletionSource<float> tcs = new TaskCompletionSource<float>();
        
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
                
                // Check if this is a progress bar output (contains percentage or progress indicators)
                if (e.Data.Contains("%") || e.Data.Contains("it/s"))
                {
                    // This is likely a progress update, log as info instead of error
                    // UnityEngine.Debug.Log($"Python Progress: {e.Data}");
                }
                else
                {
                    // This is likely an actual error
                    UnityEngine.Debug.LogError($"Python Error: {e.Data}");
                }
            }
        };
        
        // Handle process exit
        process.Exited += (sender, e) => {
            if (process.ExitCode != 0)
            {
                tcs.SetException(new Exception($"Python script exited with code {process.ExitCode}. Error: {error}"));
            }
            else
            {
                try
                {
                    string resultFilePath = Path.Combine(simulationFolderPath, "behavior_analysis.json");
                    if (File.Exists(resultFilePath))
                    {
                        string jsonContent = File.ReadAllText(resultFilePath);
                        
                        // Use Newtonsoft.Json to parse the JSON
                        JObject resultObj = JObject.Parse(jsonContent);
                        
                        // Extract all metrics
                        if (resultObj["statistics"]["distribution_metrics"] != null)
                        {
                            // New format with multiple metrics
                            var metrics = resultObj["statistics"]["distribution_metrics"];
                            float klDivergence = metrics["kl_divergence"].Value<float>();
                            
                            // Log all metrics for reference
                            UnityEngine.Debug.Log($"Behavior Metrics - KL: {metrics["kl_divergence"].Value<float>():F4}, " +
                                                 $"JS: {metrics["js_divergence"].Value<float>():F4}, " +
                                                 $"Entropy Gap: {metrics["entropy_gap"].Value<float>():F4}, " +
                                                 $"TVD: {metrics["tvd"].Value<float>():F4}");
                            
                            tcs.SetResult(klDivergence);
                        }
                        else
                        {
                            // Fallback to old format with just KL divergence
                            float klDivergence = resultObj["statistics"]["kl_divergence"].Value<float>();
                            tcs.SetResult(klDivergence);
                        }
                    }
                    else
                    {
                        tcs.SetException(new FileNotFoundException($"Evaluation result file not found: {resultFilePath}"));
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(new Exception($"Failed to read evaluation result: {ex.Message}"));
                }
            }
            
            process.Dispose();
        };
        
        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            // If the process doesn't exit within a reasonable time, kill it
            if (await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromMinutes(5))) != tcs.Task)
            {
                process.Kill();
                throw new TimeoutException("Python script execution timed out after 5 minutes");
            }
            
            return await tcs.Task;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error running Python script: {ex.Message}");
            throw;
        }
    }
    
    private static string GetPythonExecutablePath()
    {
        // Use the specific virtual environment python executable
        if (Application.platform == RuntimePlatform.WindowsEditor || 
            Application.platform == RuntimePlatform.WindowsPlayer)
        {
            return @"C:\Users\wangy\projects\ActiveShooterLLMAgent\venv\Scripts\python.exe";
        }
        else
        {
            // For macOS and Linux, you might need to adjust this path accordingly
            return "python3"; // Default fallback for non-Windows platforms
        }
    }
} 