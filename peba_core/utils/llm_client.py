#!/usr/bin/env python
"""
LLM client utilities for PEBA-PEvo framework.

This module provides a unified interface for interacting with language models,
specifically OpenAI's API for behavior classification and persona optimization.
"""

import os
import json
from typing import Dict, Any, Optional, Tuple
from openai import OpenAI

from ..config import (
    DEFAULT_OPENAI_CONFIG,
    CLASSIFICATION_OPENAI_CONFIG, 
    PERSONA_OPTIMIZATION_CONFIG,
    BEHAVIOR_DESCRIPTIONS,
    BEHAVIOR_CATEGORIES
)


class LLMClient:
    """Unified client for LLM interactions."""
    
    def __init__(self, api_key: Optional[str] = None):
        """
        Initialize the LLM client.
        
        Args:
            api_key: OpenAI API key. If None, will try to get from environment
        """
        if api_key is None:
            api_key = os.environ.get("OPENAI_API_KEY", "")
        
        self.api_key = api_key
        self.client = OpenAI(api_key=api_key) if api_key else None
    
    def _make_api_call(self, messages: list, config: Dict[str, Any]) -> Tuple[Optional[str], Optional[Dict[str, int]]]:
        """
        Make an API call to OpenAI.
        
        Args:
            messages: List of message dictionaries
            config: Configuration for the API call
            
        Returns:
            Tuple of (response_content, token_usage)
        """
        if not self.client:
            return None, None
        
        try:
            response = self.client.chat.completions.create(
                messages=messages,
                **config
            )
            
            content = response.choices[0].message.content
            token_usage = {
                "prompt_tokens": response.usage.prompt_tokens,
                "completion_tokens": response.usage.completion_tokens,
                "total_tokens": response.usage.prompt_tokens + response.usage.completion_tokens
            }
            
            return content, token_usage
            
        except Exception as e:
            print(f"Error in OpenAI API call: {e}")
            return None, None
    
    def classify_agent_behavior(self, agent_data: Dict[str, Any], context: Dict[str, str]) -> Dict[str, Any]:
        """
        Classify an agent's behavior based on their data using LLM.
        
        Args:
            agent_data: Dictionary containing agent data
            context: Dictionary with 'memories' and 'timeline' strings
            
        Returns:
            Dictionary containing classification results
        """
        if not self.client:
            return {
                "classification": "UNKNOWN",
                "error": "OpenAI API key not configured. Please set the OPENAI_API_KEY environment variable."
            }
        
        # Get agent persona information
        persona = agent_data.get('persona', {})
        name = persona.get('name', 'Unknown')
        occupation = persona.get('occupation', 'Unknown')
        age = persona.get('age', 'Unknown')
        gender = persona.get('gender', 'Unknown')
        
        memory_text = context["memories"]
        timeline_text = context["timeline"]
        
        if not memory_text or not timeline_text:
            return {
                "classification": "UNKNOWN",
                "error": "Insufficient data found for agent"
            }
        
        # Prepare the classification prompt
        behavior_descriptions_text = "\n".join([
            f"- {category}: {description}"
            for category, description in BEHAVIOR_DESCRIPTIONS.items()
        ])
        
        prompt = f"""
You are a behavior analyst categorizing how individuals responded during an active shooter incident.
Based on the agent's memories, actions, moods, plans, and dialog, classify its behavior into exactly ONE of these categories that best describes its behavior:

{behavior_descriptions_text}

Here are the agent's memories:
{memory_text}

Here is the agent's timeline showing actions, moods, plans, and dialog:
{timeline_text}

Given the context, reason about the agent's behavior and classify it into one of the 6 categories.
Then, give a rank of the 6 categories by ordering them from most likely to least likely.
Output in JSON format.

{{
    "reasoning": "<your reasoning here: 3 sentences max>",
    "classification": "<choose 1 from the 6 categories>",
    "ranking": [
        "<most likely category>",
        "<second most likely category>",
        "<third most likely category>",
        "<fourth most likely category>",
        "<fifth most likely category>",
        "<least likely category>"
    ]
}}
"""
        
        messages = [{"role": "system", "content": prompt}]
        response_content, token_usage = self._make_api_call(messages, CLASSIFICATION_OPENAI_CONFIG)
        
        if not response_content:
            return {
                "classification": "ERROR",
                "error": "Failed to get response from LLM"
            }
        
        try:
            # Extract the JSON response
            json_response = json.loads(response_content)
            
            # Extract the classification
            classification = json_response["classification"]
            reasoning = json_response.get("reasoning", "")
            ranking = json_response.get("ranking", [])
            
            # Validate it's one of our categories
            if classification not in BEHAVIOR_CATEGORIES:
                # If not an exact match, try to find the closest category
                for category in BEHAVIOR_CATEGORIES:
                    if category in classification:
                        classification = category
                        break
                else:
                    classification = "UNCLASSIFIED"
            
            return {
                "classification": classification,
                "reasoning": reasoning,
                "ranking": ranking,
                "persona": {
                    "name": name,
                    "occupation": occupation,
                    "age": age,
                    "gender": gender
                },
                "final_status": agent_data.get("final_status", "Unknown"),
                "token_usage": token_usage
            }
            
        except json.JSONDecodeError:
            return {
                "classification": "ERROR",
                "error": f"Failed to parse LLM response as JSON: {response_content}",
                "token_usage": token_usage
            }
    
    def optimize_agent_personality(self, agent_name: str, current_behavior: str, target_behavior: str, 
                                 persona: Dict[str, Any], agent_data: Optional[Dict[str, Any]] = None) -> Tuple[Dict[str, Any], Optional[str], Optional[Dict[str, int]]]:
        """
        Use LLM to suggest personality adjustments to nudge an agent toward a target behavior.
        
        Args:
            agent_name: Name of the agent
            current_behavior: Current observed behavior
            target_behavior: Desired target behavior
            persona: Current persona dictionary
            agent_data: Optional full agent data for context
            
        Returns:
            Tuple of (updated_persona, error_message, token_usage)
        """
        if not self.client:
            return persona, "OpenAI API key not configured. Please set the OPENAI_API_KEY environment variable.", None
        
        # Extract persona details
        name = persona.get("name", "Unknown")
        role = persona.get("role", "Unknown")
        age = persona.get("age", "Unknown")
        gender = persona.get("gender", "Unknown")
        pronouns = persona.get("pronouns", "Unknown")
        personality_traits = persona.get("personality_traits", "")
        emotional_disposition = persona.get("emotional_disposition", "")
        motivations_goals = persona.get("motivations_goals", "")
        communication_style = persona.get("communication_style", "")
        knowledge_scope = persona.get("knowledge_scope", "")
        backstory = persona.get("backstory", "")
        
        # Extract plan information if agent_data is available
        plan_info = ""
        if agent_data:
            actions = agent_data.get('actions', [])
            plans = []
            for action in sorted(actions, key=lambda x: x.get('time', 0)):
                time = action.get('time', 0)
                plan = action.get('plan', '')
                if plan:
                    plans.append(f"Time {time:.1f}s: {plan}")
            
            if plans:
                plan_info = "Agent's plans during the incident:\n" + "\n".join(plans)
        
        # Prepare behavior descriptions
        behavior_descriptions_text = "\n".join([
            f"- {category}: {description}"
            for category, description in BEHAVIOR_DESCRIPTIONS.items()
        ])
        
        # Prepare the optimization prompt
        system_prompt = f"""
You are an expert in human behavior during crisis situations. Your task is to adjust a person's personality traits to make them more likely to exhibit a specific behavior during an active shooter incident.

Behavior descriptions:
{behavior_descriptions_text}

Please suggest adjustments to the persona's traits that would make this person more likely to exhibit the target behavior during a crisis. Consider their age, role, and other factors that might influence their response.

You may only modify the following fields:
- personality_traits
- emotional_disposition
- motivations_goals
- communication_style
- knowledge_scope
- backstory

Return ONLY a JSON object with these fields, exactly in this format:
{{
    "personality_traits": "string | 25 words max",
    "emotional_disposition": "string | 25 words max",
    "motivations_goals": "string | 25 words max",
    "communication_style": "string | 25 words max",
    "knowledge_scope": "string | 25 words max",
    "backstory": "string | 25 words max"
}}
"""

        user_prompt = f"""
Current persona:
- Name: {name}
- Role: {role}
- Age: {age}
- Gender: {gender}
- Pronouns: {pronouns}
- Personality traits: {personality_traits}
- Emotional disposition: {emotional_disposition}
- Motivations and goals: {motivations_goals}
- Communication style: {communication_style}
- Knowledge scope: {knowledge_scope}
- Backstory: {backstory}

{plan_info}

Current observed behavior: {current_behavior}
Target behavior: {target_behavior}
"""
        
        messages = [
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt}
        ]
        
        response_content, token_usage = self._make_api_call(messages, PERSONA_OPTIMIZATION_CONFIG)
        
        if not response_content:
            return persona, "Failed to get response from LLM", token_usage
        
        # Parse the JSON object
        try:
            result = json.loads(response_content)
            
            # Ensure it has the correct structure with all required fields
            required_fields = ["personality_traits", "emotional_disposition", "motivations_goals", 
                              "communication_style", "knowledge_scope", "backstory"]
            
            if not isinstance(result, dict) or not all(field in result for field in required_fields):
                raise ValueError("Response does not have all the expected fields")
            
            # Ensure all values are strings
            if not all(isinstance(result[field], str) for field in required_fields):
                raise ValueError("Not all fields are strings")
            
            # Create updated persona
            updated_persona = persona.copy()
            for field in required_fields:
                updated_persona[field] = result[field]
            
            return updated_persona, None, token_usage
            
        except (json.JSONDecodeError, ValueError) as e:
            return persona, f"Failed to parse LLM response: {e}", token_usage
