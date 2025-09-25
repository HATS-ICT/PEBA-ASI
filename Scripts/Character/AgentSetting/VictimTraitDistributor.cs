using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var previousGUIState = GUI.enabled;
        
        GUI.enabled = false;
        
        EditorGUI.PropertyField(position, property, label, true);
        
        GUI.enabled = previousGUIState;
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif

public class VictimTraitDistributor : MonoBehaviour
{
    [SerializeField] private GameObject victimsContainer;
    
    [System.Serializable]
    public class VictimTraitProfile
    {
        [ReadOnly] public TrainingLevel trainingLevel;
        [ReadOnly] public FamiliarityLevel familiarityLevel;
        [ReadOnly] public ShooterPerceptionLevel shooterPerceptionLevel;
        [ReadOnly] public float weight; // Weight for this configuration (percentage) - now readonly in inspector
        [ReadOnly] public int calculatedCount; // Will be calculated based on weight
    }
    
    [System.Serializable]
    public class BehaviorDistribution
    {
        [ReadOnly] public string behaviorType;
        public string instruction; // Can be adjusted
        [ReadOnly] public float weight; // Weight for this behavior (percentage) - now readonly in inspector
        [ReadOnly] public int calculatedCount; // Will be calculated based on weight
    }
    
    // [SerializeField, ReadOnly]
    // private VictimTraitProfile[] settingsDistributions = new VictimTraitProfile[]
    // {
    //     // Based on Table 7 data
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Direct, weight = 0.252f },
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Vague, weight = 0.01f },
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Unaware, weight = 0.01f },
        
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Direct, weight = 0.01f },
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Vague, weight = 0.01f },
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Unaware, weight = 0.01f },
        
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Direct, weight = 0.01f },
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Vague, weight = 0.208f },
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Unaware, weight = 0.01f },
         
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Direct, weight = 0.01f },
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Vague, weight = 0.252f },
    //     new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Unaware, weight = 0.208f }
    // };

    private VictimTraitProfile[] settingsDistributions = new VictimTraitProfile[]
    {
        // Based on Table 7 data
        new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Direct, weight = 0.00f },
        new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Vague, weight = 0.00f },
        new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Unaware, weight = 0.00f },
        
        new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Direct, weight = 0.00f },
        new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Vague, weight = 0.00f },
        new VictimTraitProfile { trainingLevel = TrainingLevel.High, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Unaware, weight = 0.00f },
        
        new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Direct, weight = 1.0f },
        new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Vague, weight = 0.0f },
        new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.High, shooterPerceptionLevel = ShooterPerceptionLevel.Unaware, weight = 0.00f },
         
        new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Direct, weight = 0.00f },
        new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Vague, weight = 0.0f },
        new VictimTraitProfile { trainingLevel = TrainingLevel.Low, familiarityLevel = FamiliarityLevel.Low, shooterPerceptionLevel = ShooterPerceptionLevel.Unaware, weight = 0.0f }
    };
    
    // [SerializeField, ReadOnly]
    // private BehaviorDistribution[] behaviorDistributions = new BehaviorDistribution[]
    // {
    //     // Based on Table 5 data for Active Shooter Incidents
    //     new BehaviorDistribution { behaviorType = "Run following a crowd", instruction = "In case of emergency, follow the crowd to the exit as soon as possible.", weight = 0.28f },
    //     new BehaviorDistribution { behaviorType = "Hide in place", instruction = "In case of emergency, hide in place as soon as possible.", weight = 0.26f },
    //     new BehaviorDistribution { behaviorType = "Run then hide", instruction = "In case of emergency, run away and then hide in a safe place.", weight = 0.22f },
    //     new BehaviorDistribution { behaviorType = "Run independently", instruction = "In case of emergency, run independently to the exit as soon as possible without ever hiding.", weight = 0.12f },
    //     new BehaviorDistribution { behaviorType = "Freeze", instruction = "In case of emergency, freeze in place and unable to do anything.", weight = 0f },
    //     new BehaviorDistribution { behaviorType = "Fight", instruction = "In case of emergency, fight back or confront the threat.", weight = 0f }
    // };
    private BehaviorDistribution[] behaviorDistributions = new BehaviorDistribution[]
    {
        // Based on Table 5 data for Active Shooter Incidents
        new BehaviorDistribution { behaviorType = "Run following a crowd", instruction = "This behavior involves fleeing alongside a group, driven by the instinct to follow others without independently evaluating the safest route. The individual may be overwhelmed and default to the crowd's direction out of panic or uncertainty, relying on others’ actions instead of personal judgment.", weight = 0.28f },
        new BehaviorDistribution { behaviorType = "Hide in place", instruction = "The person immediately takes cover wherever they currently are, often due to the belief that movement would increase danger or because they are unable to assess a safer location. This behavior may stem from fear, confusion, or limited knowledge of the environment, leading to a decision to conceal rather than flee.", weight = 0.26f },
        new BehaviorDistribution { behaviorType = "Run then hide", instruction = "This response begins with the person running away from the immediate threat and then transitioning to hiding once they feel a degree of separation from danger. It reflects a blend of instinctive flight and strategic thinking, where the person adapts their response as the situation evolves, often choosing concealment when escape routes narrow or become unsafe.", weight = 0.12f },
        new BehaviorDistribution { behaviorType = "Run independently", instruction = "In this behavior, the person flees the scene in a direction they determine independently of the crowd, often based on quick environmental assessment or prior knowledge of the area. This reflects a high level of situational awareness, personal decisiveness, and the ability to think critically under pressure.", weight = 0.12f },
        new BehaviorDistribution { behaviorType = "Freeze", instruction = "The person becomes mentally and physically immobilized, unable to act due to overwhelming fear or shock. This involuntary reaction, often referred to as tonic immobility, results from cognitive overload and can prevent any attempt to run, hide, or fight, despite imminent danger.", weight = 0.12f },
        new BehaviorDistribution { behaviorType = "Fight", instruction = "This behavior involves actively attempting to confront, disarm, or incapacitate the shooter, typically as a last resort when no other options are viable. It may arise from a survival instinct, protective impulse (especially toward others), or previous training that enables the person to override fear and engage in direct action.", weight = 0.10f }
    };
    
    private List<(TrainingLevel, FamiliarityLevel, ShooterPerceptionLevel)> settingsPool = new List<(TrainingLevel, FamiliarityLevel, ShooterPerceptionLevel)>();
    private List<string> behaviorPool = new List<string>();
    
    private void Awake()
    {
        if (victimsContainer == null)
        {
            victimsContainer = GameObject.Find("Victims");
            if (victimsContainer == null)
            {
                Debug.LogError("Victims container not found! Please assign it in the inspector.");
                return;
            }
        }
        
        int totalAgents = victimsContainer.transform.childCount;
        
        if (totalAgents == 0)
        {
            Debug.LogError("No agents found in the Victims container!");
            return;
        }
        
        CalculateDistributionCounts(settingsDistributions, totalAgents);
        
        CalculateDistributionCounts(behaviorDistributions, totalAgents);
        
        foreach (var distribution in settingsDistributions)
        {
            for (int i = 0; i < distribution.calculatedCount; i++)
            {
                settingsPool.Add((distribution.trainingLevel, distribution.familiarityLevel, distribution.shooterPerceptionLevel));
            }
        }
        
        foreach (var distribution in behaviorDistributions)
        {
            for (int i = 0; i < distribution.calculatedCount; i++)
            {
                behaviorPool.Add(distribution.instruction);
            }
        }
        
        int settingsRemaining = totalAgents - settingsPool.Count;
        if (settingsRemaining > 0)
        {
            var firstSetting = settingsDistributions[0];
            for (int i = 0; i < settingsRemaining; i++)
            {
                settingsPool.Add((firstSetting.trainingLevel, firstSetting.familiarityLevel, firstSetting.shooterPerceptionLevel));
            }
        }
        
        int behaviorsRemaining = totalAgents - behaviorPool.Count;
        if (behaviorsRemaining > 0)
        {
            var firstBehavior = behaviorDistributions[0];
            for (int i = 0; i < behaviorsRemaining; i++)
            {
                behaviorPool.Add(firstBehavior.instruction);
            }
        }
        
        ShufflePool(settingsPool);
        ShufflePool(behaviorPool);
        
        if (false)
        {
            LogDistribution();
        }
    }
    
    private void Start()
    {
        // Get PersonDataManager components from the victims container
        var personDataManagers = victimsContainer.GetComponentsInChildren<PersonDataManager>();
        
        // Ensure we have enough settings and behaviors for all agents
        if (personDataManagers.Length > settingsPool.Count || personDataManagers.Length > behaviorPool.Count)
        {
            Debug.LogError($"Not enough settings or behaviors for all agents. Agents: {personDataManagers.Length}, Settings: {settingsPool.Count}, Behaviors: {behaviorPool.Count}");
            return;
        }
        
        // Assign settings and behaviors to each agent based on the enforcement mode
        for (int i = 0; i < personDataManagers.Length; i++)
        {
            var settings = settingsPool[i];
            
            if (SimConfig.BehaviorEnforcing == SimConfig.BehaviorEnforcementMode.NoEnforcing)
            {
                // For NoEnforcing mode, set all agents to Low training, Low familiarity, Direct perception
                personDataManagers[i].trainingLevel = TrainingLevel.Low;
                personDataManagers[i].familiarityLevel = FamiliarityLevel.Low;
                personDataManagers[i].shooterPerceptionLevel = ShooterPerceptionLevel.Direct;
                personDataManagers[i].agentSpecificInstruction = "";
            }
            else if (SimConfig.BehaviorEnforcing == SimConfig.BehaviorEnforcementMode.Explicit)
            {
                // For Explicit mode, set all agents to Low training, High familiarity, Direct perception
                personDataManagers[i].trainingLevel = TrainingLevel.Low;
                personDataManagers[i].familiarityLevel = FamiliarityLevel.High;
                personDataManagers[i].shooterPerceptionLevel = ShooterPerceptionLevel.Direct;
                personDataManagers[i].agentSpecificInstruction = behaviorPool[i];
            }
            else if (SimConfig.BehaviorEnforcing == SimConfig.BehaviorEnforcementMode.Implicit)
            {
                // In Implicit mode, use the distributed settings from the pool
                personDataManagers[i].trainingLevel = settings.Item1;
                personDataManagers[i].familiarityLevel = settings.Item2;
                personDataManagers[i].shooterPerceptionLevel = settings.Item3;
                personDataManagers[i].agentSpecificInstruction = "";
            }
        }
    }
    
    private void CalculateDistributionCounts<T>(T[] distributions, int totalAgents) where T : class
    {
        float totalWeight = 0f;
        foreach (var dist in distributions)
        {
            if (dist is VictimTraitProfile settingDist)
            {
                totalWeight += settingDist.weight;
            }
            else if (dist is BehaviorDistribution behaviorDist)
            {
                totalWeight += behaviorDist.weight;
            }
        }
        
        // Normalize weights if they don't sum to 1
        if (Mathf.Abs(totalWeight - 1f) > 0.001f)
        {
            Debug.LogWarning($"Weights don't sum to 1 (sum: {totalWeight}). Normalizing...");
            foreach (var dist in distributions)
            {
                if (dist is VictimTraitProfile settingDist)
                {
                    settingDist.weight /= totalWeight;
                }
                else if (dist is BehaviorDistribution behaviorDist)
                {
                    behaviorDist.weight /= totalWeight;
                }
            }
        }
        
        int assignedCount = 0;
        for (int i = 0; i < distributions.Length; i++)
        {
            float weight = 0f;
            if (distributions[i] is VictimTraitProfile settingDist)
            {
                weight = settingDist.weight;
            }
            else if (distributions[i] is BehaviorDistribution behaviorDist)
            {
                weight = behaviorDist.weight;
            }
            
            if (i < distributions.Length - 1)
            {
                int count = Mathf.RoundToInt(totalAgents * weight);
                assignedCount += count;
                
                if (distributions[i] is VictimTraitProfile settingDistToUpdate)
                {
                    settingDistToUpdate.calculatedCount = count;
                }
                else if (distributions[i] is BehaviorDistribution behaviorDistToUpdate)
                {
                    behaviorDistToUpdate.calculatedCount = count;
                }
            }
            else
            {
                int remainingCount = totalAgents - assignedCount;
                
                if (distributions[i] is VictimTraitProfile settingDistToUpdate)
                {
                    settingDistToUpdate.calculatedCount = remainingCount;
                }
                else if (distributions[i] is BehaviorDistribution behaviorDistToUpdate)
                {
                    behaviorDistToUpdate.calculatedCount = remainingCount;
                }
            }
        }
    }
    
    private void ShufflePool<T>(List<T> pool)
    {
        int n = pool.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = pool[k];
            pool[k] = pool[n];
            pool[n] = value;
        }
    }
    
    // Log the calculated distribution
    private void LogDistribution()
    {
        Debug.Log("Settings Distribution:");
        foreach (var dist in settingsDistributions)
        {
            Debug.Log($"Training: {dist.trainingLevel}, Familiarity: {dist.familiarityLevel}, Perception: {dist.shooterPerceptionLevel} - Count: {dist.calculatedCount} ({dist.weight:P1})");
        }
        
        Debug.Log("Behavior Distribution:");
        foreach (var dist in behaviorDistributions)
        {
            Debug.Log($"Behavior: {dist.behaviorType} - Count: {dist.calculatedCount} ({dist.weight:P1})");
        }
    }
    
    // Editor utility to validate in the inspector
    [ContextMenu("Validate Weights")]
    private void ValidateWeights()
    {
        float settingsTotal = settingsDistributions.Sum(d => d.weight);
        float behaviorsTotal = behaviorDistributions.Sum(d => d.weight);
        
        Debug.Log($"Settings weights sum: {settingsTotal:F3} (should be close to 1.0)");
        Debug.Log($"Behavior weights sum: {behaviorsTotal:F3} (should be close to 1.0)");
    }

    // Public methods to update weights programmatically
    public void UpdateTraitWeights(float[] newWeights)
    {
        if (newWeights.Length != settingsDistributions.Length)
        {
            Debug.LogError($"Weight array length mismatch. Expected {settingsDistributions.Length}, got {newWeights.Length}");
            return;
        }
        
        // Update all weights
        for (int i = 0; i < settingsDistributions.Length; i++)
        {
            settingsDistributions[i].weight = newWeights[i];
        }
        
        ValidateWeights();
    }

    public void UpdateBehaviorWeights(float[] newWeights)
    {
        if (newWeights.Length != behaviorDistributions.Length)
        {
            Debug.LogError($"Weight array length mismatch. Expected {behaviorDistributions.Length}, got {newWeights.Length}");
            return;
        }
        
        for (int i = 0; i < behaviorDistributions.Length; i++)
        {
            behaviorDistributions[i].weight = newWeights[i];
        }
        
        ValidateWeights();
    }

    public float[] GetTraitWeights()
    {
        float[] weights = new float[settingsDistributions.Length];
        for (int i = 0; i < settingsDistributions.Length; i++)
        {
            weights[i] = settingsDistributions[i].weight;
        }
        return weights;
    }

    public float[] GetBehaviorWeights()
    {
        float[] weights = new float[behaviorDistributions.Length];
        for (int i = 0; i < behaviorDistributions.Length; i++)
        {
            weights[i] = behaviorDistributions[i].weight;
        }
        return weights;
    }

    public void RecalculateDistributions()
    {
        if (victimsContainer == null)
        {
            Debug.LogError("Victims container not set!");
            return;
        }
        
        int totalAgents = victimsContainer.transform.childCount;
        
        // Recalculate counts based on new weights
        CalculateDistributionCounts(settingsDistributions, totalAgents);
        CalculateDistributionCounts(behaviorDistributions, totalAgents);
        
        // Clear and repopulate pools
        settingsPool.Clear();
        behaviorPool.Clear();
        
        // Repopulate the settings pool based on the calculated counts
        foreach (var distribution in settingsDistributions)
        {
            for (int i = 0; i < distribution.calculatedCount; i++)
            {
                settingsPool.Add((distribution.trainingLevel, distribution.familiarityLevel, distribution.shooterPerceptionLevel));
            }
        }
        
        // Repopulate the behavior pool based on the calculated counts
        foreach (var distribution in behaviorDistributions)
        {
            for (int i = 0; i < distribution.calculatedCount; i++)
            {
                behaviorPool.Add(distribution.instruction);
            }
        }
        
        // Handle any remaining agents due to rounding
        int settingsRemaining = totalAgents - settingsPool.Count;
        if (settingsRemaining > 0)
        {
            var firstSetting = settingsDistributions[0];
            for (int i = 0; i < settingsRemaining; i++)
            {
                settingsPool.Add((firstSetting.trainingLevel, firstSetting.familiarityLevel, firstSetting.shooterPerceptionLevel));
            }
        }
        
        int behaviorsRemaining = totalAgents - behaviorPool.Count;
        if (behaviorsRemaining > 0)
        {
            var firstBehavior = behaviorDistributions[0];
            for (int i = 0; i < behaviorsRemaining; i++)
            {
                behaviorPool.Add(firstBehavior.instruction);
            }
        }
        
        // Shuffle both pools to randomize assignment
        ShufflePool(settingsPool);
        ShufflePool(behaviorPool);
    }
} 