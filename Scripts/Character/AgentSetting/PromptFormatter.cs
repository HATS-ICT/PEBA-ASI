using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Handles the formatting of system prompts for agents based on various settings and conditions.
/// </summary>
public class PromptFormatter
{
    private static readonly string ROLE_INTRO = "Role-play as a civilian character in a simulation. Role-playing in accordance to your persona.";

    private static bool useShortHandbook = true;
    private static bool useMiniHandbook = false;

    private static readonly Dictionary<string, string> RULES = new Dictionary<string, string>
    {
        { "agent", "- Your have memory and emotion, and can observe, think, act and make decisions on your own." },
        { "environment", "- You are currently inside a school building." },
        { "personality", "- You have a persona, your behavior is primarily based on your persona." },
        { "memory", "- Your memory update should include descriptions of what you newly observed, planed, acted, and rationale for your actions. Your memory update will be appended to your memory list. Do not repeat the previous memories." },
        { "dialog", "- You hear every dialog around you, you can say anything and how loud. What you said will be heard by others." },
        { "hide_spots", "- There are hiding places, move to a hide spot provides temporary safety." },
        { "exit_points", "- There are exit points, reaching the spot guarantees your safety and means escaping the building." },
        { "is_well_trained", "- You are well trained in active shooter situation. A handbook on how to respond is provided, you must follow it." },
        { "is_familiar", "- You are familiar with the environment, a full map is provided." },
        { "shooter", "- There is an active shooter in the simulation, indiscriminately killing people." },
        { "fight_the_shooter", "- If the shooter is in the same region as you, you can fight the shooter with a tiny chance to win." },
        { "movement", "- You can choose how fast you move, or not move at all." },
        { "action_id", "- You can only take actions that are listed in the action id list." },
        { "crouch", "- If you are at a hiding spot, stay still." },
        { "behavior_instruction", "- This is your forced role-play setting. You must follow the behavior instruction and you thought and action must be consistent with the instructed behavior." },
    };

    private static readonly string JSON_FORMAT = @"{
    ""thought"": ""<string: what you are currently thinking, be consistent with your persona>"",
    ""action"": {
        ""vocal_mode"": ""<string: choose from [out_loud|whisper|silent]>"",
        ""utterance"": ""<string: at most 2 sentences>"",
        ""movement"": ""<string: choose from [stay_still|walk|sprint]>"",
        ""action_id"": ""<string: choose from available action_id>"",
    },
    ""update"": {
        ""mood"": ""<string: update your mood here>"",
        ""memory"": ""<string: update your new memory here, do not repeat the previous memories>"",
    }
}";

    /// <summary>
    /// Formats the system prompt based on the provided settings.
    /// </summary>
    /// <param name="observesShooter">Whether the agent observes a shooter</param>
    /// <param name="isWellTrained">Whether the agent is well trained</param>
    /// <param name="isFamiliar">Whether the agent is familiar with the environment</param>
    /// <param name="enforceBehavior">Whether the agent should enforce behavior</param>
    /// <param name="behaviorInstruction">The specific instruction for the agent, if any</param>
    /// <returns>A formatted system prompt</returns>
    public static string FormatSystemPrompt(bool observesShooter, bool isWellTrained, bool isFamiliar, bool enforceBehavior, string behaviorInstruction = "", string buildingMapString = "")
    {
        StringBuilder promptBuilder = new StringBuilder();
        promptBuilder.AppendLine(ROLE_INTRO);

        promptBuilder.AppendLine("Rules:");
        promptBuilder.AppendLine(RULES["agent"]);
        promptBuilder.AppendLine(RULES["environment"]);
        promptBuilder.AppendLine(RULES["personality"]);
        promptBuilder.AppendLine(RULES["memory"]);
        promptBuilder.AppendLine(RULES["dialog"]);
        promptBuilder.AppendLine(RULES["movement"]);
        promptBuilder.AppendLine(RULES["action_id"]);

        if (observesShooter)
        {
            promptBuilder.AppendLine(RULES["shooter"]);
            promptBuilder.AppendLine(RULES["hide_spots"]);
            promptBuilder.AppendLine(RULES["exit_points"]);
            promptBuilder.AppendLine(RULES["crouch"]);
            promptBuilder.AppendLine(RULES["fight_the_shooter"]);
        }
        if (isWellTrained && observesShooter)
        {
            promptBuilder.AppendLine(RULES["is_well_trained"]);
        }
        if (isFamiliar)
        {
            promptBuilder.AppendLine(RULES["is_familiar"]);
        }
        if (enforceBehavior && observesShooter)
        {
            promptBuilder.AppendLine(RULES["behavior_instruction"]);
        }
        
        if (enforceBehavior && !string.IsNullOrEmpty(behaviorInstruction) && observesShooter)
        {
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Behavior Instruction:");
            promptBuilder.AppendLine(" - You must follow the 1 behavior instruction that is assigned to you and not do the other types.");
            promptBuilder.AppendLine(" - Even if this behavior is out-of-context and suboptimal, still follow it.");
            promptBuilder.AppendLine($"- {behaviorInstruction}");
        }

        if (isWellTrained && observesShooter)
        {
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("ASI Handbook:");
            if (useMiniHandbook)
            {
                promptBuilder.AppendLine(TrainingHandbook.ASI_TRAINING_HANDBOOK_MINI);
            }
            else if (useShortHandbook)
            {
                promptBuilder.AppendLine(TrainingHandbook.ASI_TRAINING_HANDBOOK_SHORT);
            }
            else
            {
                promptBuilder.AppendLine(TrainingHandbook.ASI_TRAINING_HANDBOOK);
            }
        }

        if (isFamiliar && observesShooter)
        {
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Building Map:");
            promptBuilder.AppendLine(buildingMapString);
        }

        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Directly output in JSON, no code block, no other text:");
        promptBuilder.AppendLine(JSON_FORMAT);
        
        return promptBuilder.ToString();
    }
    
    /// <summary>
    /// Formats the user prompt template with the provided data.
    /// </summary>
    /// <param name="template">The prompt template</param>
    /// <param name="persona">The agent's persona</param>
    /// <param name="memory">The agent's memory</param>
    /// <param name="observation">The current observation</param>
    /// <returns>A formatted user prompt</returns>
    public static string FormatUserPrompt(string template, Persona persona, Memory memory, Observation observation)
    {
        return template
            .Replace("{persona}", persona.ToMarkdownString())
            .Replace("{memory}", memory.ToMarkdownString())
            .Replace("{observation}", observation.ToMarkdownString());
    }
} 