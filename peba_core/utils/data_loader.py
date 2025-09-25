#!/usr/bin/env python
"""
Data loading utilities for PEBA-PEvo framework.

This module provides functions to load and process simulation data, optimization runs,
and agent behavior data from various file formats.
"""

import os
import json
import re
from typing import Dict, List, Optional, Tuple, Any
from collections import defaultdict

from ..config import (
    BASE_OPTIMIZATION_PATH, 
    BASE_SIMULATION_PATH, 
    DEFAULT_SIMULATION_FOLDER,
    BEHAVIOR_CATEGORIES
)


def load_json_file(file_path: str) -> Optional[Dict[str, Any]]:
    """
    Load a JSON file and return its contents.
    
    Args:
        file_path: Path to the JSON file
        
    Returns:
        Dictionary containing the JSON data, or None if loading fails
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            return json.load(f)
    except Exception as e:
        print(f"Error loading JSON file {file_path}: {e}")
        return None


def load_optimization_run_data(optimization_runs: List[str], base_path: str = BASE_OPTIMIZATION_PATH) -> Dict[str, Any]:
    """
    Load data from multiple optimization runs.
    
    Args:
        optimization_runs: List of optimization run folder names
        base_path: Base path where optimization runs are stored
        
    Returns:
        Dictionary containing all optimization run data
    """
    all_data = {}
    
    for run_name in optimization_runs:
        run_path = os.path.join(base_path, run_name)
        if not os.path.exists(run_path):
            print(f"Warning: Optimization run path not found: {run_path}")
            continue
        
        # Find all iteration folders
        iteration_folders = [
            f for f in os.listdir(run_path) 
            if os.path.isdir(os.path.join(run_path, f)) and f.startswith("Iteration_")
        ]
        
        if not iteration_folders:
            print(f"Warning: No iteration folders found in {run_path}")
            continue
        
        # Sort iteration folders numerically
        iteration_folders.sort(key=lambda x: int(x.split("_")[1]))
        
        run_data = {"iterations": {}}
        
        # Load data from each iteration
        for iteration_folder in iteration_folders:
            iteration_path = os.path.join(run_path, iteration_folder)
            analysis_path = os.path.join(iteration_path, "behavior_analysis.json")
            
            if not os.path.exists(analysis_path):
                print(f"Warning: Behavior analysis file not found: {analysis_path}")
                continue
            
            analysis_data = load_json_file(analysis_path)
            if not analysis_data:
                continue
            
            # Extract iteration number
            iteration_num = int(iteration_folder.split("_")[1])
            
            # Extract metrics
            metrics = analysis_data.get("statistics", {}).get("distribution_metrics", {})
            
            # Extract behavior distribution
            behavior_counts = analysis_data.get("statistics", {}).get("behavior", {})
            total_agents = analysis_data.get("statistics", {}).get("total_agents", 0)
            
            # Calculate behavior distribution
            behavior_dist = {}
            for category in BEHAVIOR_CATEGORIES:
                behavior_dist[category] = behavior_counts.get(category, 0) / total_agents if total_agents > 0 else 0
            
            # Store individual agent behaviors
            agent_behaviors = {}
            for agent_name, agent_data in analysis_data.get("agents", {}).items():
                if "behavior" in agent_data and "classification" in agent_data["behavior"]:
                    agent_behaviors[agent_name] = agent_data["behavior"]["classification"]
            
            # Store data for this iteration
            run_data["iterations"][iteration_num] = {
                "metrics": metrics,
                "behavior_distribution": behavior_dist,
                "total_agents": total_agents,
                "agent_behaviors": agent_behaviors
            }
        
        # Store data for this run
        all_data[run_name] = run_data
    
    return all_data


def load_agent_data(agent_logs_folder: str) -> Dict[str, Any]:
    """
    Load agent data from JSON files in the agent logs folder.
    
    Args:
        agent_logs_folder: Path to the folder containing agent JSON files
        
    Returns:
        Dictionary mapping agent names to their data
    """
    agent_data = {}
    
    if not os.path.exists(agent_logs_folder):
        print(f"Agent logs folder not found: {agent_logs_folder}")
        return agent_data
    
    agent_files = [f for f in os.listdir(agent_logs_folder) if f.endswith('.json')]
    
    for agent_file in agent_files:
        agent_name = agent_file.replace('.json', '')
        file_path = os.path.join(agent_logs_folder, agent_file)
        
        data = load_json_file(file_path)
        if data:
            agent_data[agent_name] = data
    
    return agent_data


def load_personas_data(personas_path: str) -> Optional[Dict[str, Any]]:
    """
    Load personas data from a JSON file.
    
    Args:
        personas_path: Path to the personas JSON file
        
    Returns:
        Dictionary containing personas data, or None if loading fails
    """
    return load_json_file(personas_path)


def load_simulation_data(simulation_folder: str, base_path: str = BASE_SIMULATION_PATH, 
                        folder_name: str = DEFAULT_SIMULATION_FOLDER, direct_path: bool = False) -> Tuple[Optional[str], Optional[Dict[str, Any]]]:
    """
    Load simulation data including agent logs.
    
    Args:
        simulation_folder: Name of the simulation folder
        base_path: Base path where simulations are stored
        folder_name: Simulation folder name within base path
        direct_path: If True, treat simulation_folder as direct path
        
    Returns:
        Tuple of (agent_logs_folder_path, agent_data_dict)
    """
    if direct_path:
        agent_logs_folder = os.path.join(simulation_folder, "AgentLogs")
    else:
        agent_logs_folder = os.path.join(base_path, folder_name, simulation_folder, "AgentLogs")
    
    if not os.path.exists(agent_logs_folder):
        return None, None
    
    agent_data = load_agent_data(agent_logs_folder)
    return agent_logs_folder, agent_data


def parse_token_usage_log(log_path: str) -> Optional[Dict[str, Any]]:
    """
    Parse Unity token usage log file.
    
    Args:
        log_path: Path to the token usage log file
        
    Returns:
        Dictionary containing token usage information, or None if parsing fails
    """
    if not os.path.exists(log_path):
        return None
    
    try:
        with open(log_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        usage_info = {}
        
        # Extract model information
        model_match = re.search(r"Model: ([\w\.-]+)", content)
        if model_match:
            usage_info["model"] = model_match.group(1)
        
        # Extract prompt tokens
        prompt_match = re.search(r"Prompt tokens: ([\d,]+)", content)
        if prompt_match:
            usage_info["prompt_tokens"] = int(prompt_match.group(1).replace(",", ""))
        
        # Extract completion tokens
        completion_match = re.search(r"Completion tokens: ([\d,]+)", content)
        if completion_match:
            usage_info["completion_tokens"] = int(completion_match.group(1).replace(",", ""))
        
        # Extract total requests
        requests_match = re.search(r"Total requests: (\d+)", content)
        if requests_match:
            usage_info["total_requests"] = int(requests_match.group(1))
        
        # Extract total cost
        cost_match = re.search(r"Total cost: \$([\d.]+)", content)
        if cost_match:
            usage_info["total_cost"] = float(cost_match.group(1))
        
        return usage_info if usage_info else None
        
    except Exception as e:
        print(f"Error parsing token usage log {log_path}: {e}")
        return None


def get_agent_context(agent_data: Dict[str, Any]) -> Dict[str, str]:
    """
    Extract comprehensive context from agent data including memories, actions, moods, plans, and dialog.
    
    Args:
        agent_data: Dictionary containing agent data
        
    Returns:
        Dictionary containing formatted memories and timeline
    """
    # Extract memories
    memories = agent_data.get('memories', [])
    memory_texts = []
    
    for memory in sorted(memories, key=lambda x: x.get('time', 0)):
        time = memory.get('time', 0)
        description = memory.get('description', '')
        memory_texts.append(f"Time {time:.1f}s: {description}")
    
    # Extract actions, moods, plans, and dialog
    actions = agent_data.get('actions', [])
    observations = agent_data.get('observations', [])
    
    # Process observations to get moods
    moods = {}
    for obs in observations:
        time = obs.get('time', 0)
        observation_data = obs.get('observation', {})
        mood = observation_data.get('mood', 'unknown')
        moods[time] = mood
    
    # Process actions to get plans and dialog
    timeline = []
    for action in sorted(actions, key=lambda x: x.get('time', 0)):
        time = action.get('time', 0)
        action_type = action.get('action_type', '')
        plan = action.get('plan', '')
        dialog = action.get('dialog_text', '')
        mood = moods.get(time, moods.get(max(t for t in moods if t <= time), 'unknown'))
        
        timeline.append({
            'time': time,
            'action_type': action_type,
            'mood': mood,
            'plan': plan,
            'dialog': dialog
        })
    
    # Format timeline as text
    timeline_texts = []
    for entry in timeline:
        time = entry['time']
        timeline_texts.append(f"Time {time:.1f}s:")
        timeline_texts.append(f"  Mood: {entry['mood']}")
        timeline_texts.append(f"  Action: {entry['action_type']}")
        if entry['plan']:
            timeline_texts.append(f"  Plan: {entry['plan']}")
        if entry['dialog']:
            timeline_texts.append(f"  Dialog: \"{entry['dialog']}\"")
        timeline_texts.append("")
    
    return {
        "memories": "\n".join(memory_texts),
        "timeline": "\n".join(timeline_texts)
    }


def find_simulation_folders(batch_folder_path: str) -> List[str]:
    """
    Find all simulation folders in a batch directory.
    
    Args:
        batch_folder_path: Path to the batch folder containing multiple simulations
        
    Returns:
        List of simulation folder names
    """
    if not os.path.exists(batch_folder_path):
        return []
    
    return [
        f for f in os.listdir(batch_folder_path) 
        if os.path.isdir(os.path.join(batch_folder_path, f))
    ]
