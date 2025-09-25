using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public static class OpenAIUtils
{
    private static readonly string apiUrl = "https://openrouter.ai/api/v1/chat/completions";
    private static readonly string apiKey = "sk-or-v1-put-your-key-here-or-on-system-environment-variable";

    private static string logFileName = SimConfig.ChatLogsFileName;
    
    private static string logFilePath = null;
    
    private static Dictionary<string, string> agentLogFilePaths = new Dictionary<string, string>();
    
    public static string model = SimConfig.DefaultLLMModel;

    public static int totalRequests = 0;
    
    public static Dictionary<string, float> totalTokens = new Dictionary<string, float>
    {
        { "prompt_tokens", 0 },
        { "cached_tokens", 0 },
        { "completion_tokens", 0 }
    };

    public static void InitializeLogFile()
    {
        if (!SimConfig.LoggingEnabled)
        {
            return;
        }
        
        logFilePath = null;
        
        agentLogFilePaths.Clear();
        
        if (SimulationManager.Instance != null)
        {
            string simulationFolder = SimulationManager.Instance.GetCurrentSimulationFolder();
            if (!string.IsNullOrEmpty(simulationFolder))
            {
                logFilePath = System.IO.Path.Combine(simulationFolder, logFileName);
                return;
            }
        }
    }

    public static float ComputeTokenCost()
    {
        float cost = 0;

        cost = costMap[model]["prompt_tokens"] * totalTokens["prompt_tokens"] / 1000000 +
                costMap[model]["cached_tokens"] * totalTokens["cached_tokens"] / 1000000 +
                costMap[model]["completion_tokens"] * totalTokens["completion_tokens"] / 1000000;

        return cost;
    }

    private static readonly Dictionary<string, Dictionary<string, float>> costMap = new Dictionary<string, Dictionary<string, float>>
    {
        {
            "gpt-4o-mini", new Dictionary<string, float>
            {
                { "prompt_tokens", 0.15f },
                { "cached_tokens", 0.075f },
                { "completion_tokens", 0.6f }
            }
        },
        {
            "gpt-4o", new Dictionary<string, float>
            {
                { "prompt_tokens", 2.5f },
                { "cached_tokens", 1.25f },
                { "completion_tokens", 10.0f }
            }
        },
        {
            "gpt-4.5", new Dictionary<string, float>
            {
                { "prompt_tokens", 75.0f },
                { "cached_tokens", 37.5f },
                { "completion_tokens", 150.0f }
            }
        },
        {
            "gpt-4.1-mini", new Dictionary<string, float>
            {
                { "prompt_tokens", 0.4f },
                { "cached_tokens", 0.1f },
                { "completion_tokens", 1.6f }
            }
        },
        {
            "gpt-4.1-nano", new Dictionary<string, float>
            {
                { "prompt_tokens", 0.1f },
                { "cached_tokens", 0.03f },
                { "completion_tokens", 0.4f }
            }
        },
        {
            "google/gemini-2.5-flash-preview", new Dictionary<string, float>
            {
                { "prompt_tokens", 0.15f },
                { "cached_tokens", 0.0f },
                { "completion_tokens", 0.6f }
            }
        },
        {
            "deepseek/deepseek-chat", new Dictionary<string, float>
            {
                { "prompt_tokens", 0.38f },
                { "cached_tokens", 0.0f },
                { "completion_tokens", 0.89f }
            }
        }
    };

    public static async Task<string> GetOneTimeResponse(string systemMessage, string prompt, List<object> conversationHistory = null, string agentName = null, string responseFormat = "json_object")
    {
        InitializeLogFile();
        
        List<object> messages = new List<object>();
        messages.Add(new { role = "system", content = systemMessage });
        
        if (conversationHistory != null && conversationHistory.Count > 0)
        {
            messages.AddRange(conversationHistory);
        }
        
        messages.Add(new { role = "user", content = prompt });
        
        var requestBody = new
        {
            model = model,
            messages = messages.ToArray(),
            response_format = new { type = responseFormat },
            temperature = SimConfig.LLMTemperature,
            seed = SimConfig.RandomSeed
        };

        string jsonRequest = JsonConvert.SerializeObject(requestBody);
        
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        
        var operation = request.SendWebRequest();
        
        while (!operation.isDone)
        {
            await Task.Yield();
        }
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.error}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
            throw new Exception($"API request failed: {request.error}");
        }
        
        string jsonResponse = request.downloadHandler.text;
        
        var responseObject = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);
        
        totalTokens["prompt_tokens"] += responseObject.Usage.PromptTokens;
        totalTokens["cached_tokens"] += responseObject.Usage.CachedTokens;
        totalTokens["completion_tokens"] += responseObject.Usage.CompletionTokens;
        
        string responseContent = responseObject.Choices[0].Message.Content;
        
        StringBuilder logEntry = new StringBuilder();
        logEntry.AppendLine($"[{DateTime.Now}]");
        logEntry.AppendLine($"SYSTEM:\n{systemMessage}");
        logEntry.AppendLine($"USER:\n{prompt}");
        logEntry.AppendLine($"ASSISTANT:\n{responseContent}");
        logEntry.AppendLine("----------------------------------------\n");
        
        LogToFile(logEntry.ToString());
        
        if (!string.IsNullOrEmpty(agentName))
        {
            LogToAgentFile(agentName, logEntry.ToString());
        }
        
        totalRequests++;
        
        return responseContent;
    }
    
    private static void LogToFile(string message)
    {
        if (!SimConfig.LoggingEnabled)
        {
            return;
        }
        
        try
        {
            if (logFilePath == null)
            {
                InitializeLogFile();
            }
            
            string directory = System.IO.Path.GetDirectoryName(logFilePath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            System.IO.File.AppendAllText(logFilePath, message);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write to log file: {ex.Message}");
        }
    }
    
    private static void LogToAgentFile(string agentName, string message)
    {
        if (!SimConfig.LoggingEnabled || string.IsNullOrEmpty(agentName))
        {
            return;
        }
        
        try
        {
            string agentLogPath;
            if (!agentLogFilePaths.TryGetValue(agentName, out agentLogPath))
            {
                if (SimulationManager.Instance != null)
                {
                    string simulationFolder = SimulationManager.Instance.GetCurrentSimulationFolder();
                    if (!string.IsNullOrEmpty(simulationFolder))
                    {
                        string agentLogsFolder = System.IO.Path.Combine(simulationFolder, "AgentChatLogs");
                        if (!System.IO.Directory.Exists(agentLogsFolder))
                        {
                            System.IO.Directory.CreateDirectory(agentLogsFolder);
                        }
                        
                        string sanitizedName = new string(agentName.Where(c => !System.IO.Path.GetInvalidFileNameChars().Contains(c)).ToArray());
                        
                        agentLogPath = System.IO.Path.Combine(agentLogsFolder, $"{sanitizedName}_ChatLogs.txt");
                        agentLogFilePaths[agentName] = agentLogPath;
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(agentLogPath))
            {
                // Ensure directory exists
                string directory = System.IO.Path.GetDirectoryName(agentLogPath);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                System.IO.File.AppendAllText(agentLogPath, message);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write to agent log file for {agentName}: {ex.Message}");
        }
    }

    // Updated method to get token usage statistics as a formatted string
    public static string GetTokenUsageInfo()
    {
        StringBuilder info = new StringBuilder();
        info.AppendLine($"<b>AI Token Usage:</b>");
        info.AppendLine($"Model: {model}");
        info.AppendLine($"Prompt tokens: {totalTokens["prompt_tokens"]:N0}");
        info.AppendLine($"Completion tokens: {totalTokens["completion_tokens"]:N0}");
        info.AppendLine($"Total requests: {totalRequests}");
        info.AppendLine($"Total cost: ${ComputeTokenCost():F3}");
        
        return info.ToString();
    }

    // Add a method to reset token usage statistics
    public static void ResetTokenUsage()
    {
        totalTokens["prompt_tokens"] = 0;
        totalTokens["cached_tokens"] = 0;
        totalTokens["completion_tokens"] = 0;
        totalRequests = 0;
    }
}

// Add these classes to handle the JSON response
public class OpenAIResponse
{
    [JsonProperty("choices")]
    public List<Choice> Choices { get; set; }
    [JsonProperty("usage")]
    public Usage Usage { get; set; }
}

public class Usage
{
    [JsonProperty("prompt_tokens")]
    public int PromptTokens { get; set; }
    [JsonProperty("cached_tokens")]
    public int CachedTokens { get; set; }
    [JsonProperty("completion_tokens")]
    public int CompletionTokens { get; set; }
}
public class Choice
{
    [JsonProperty("message")]
    public Message Message { get; set; }
}

public class Message
{
    [JsonProperty("content")]
    public string Content { get; set; }
}
