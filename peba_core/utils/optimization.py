#!/usr/bin/env python
"""
Optimization utilities for PEBA-PEvo framework.

This module provides functions for persona optimization, agent selection,
and behavior adjustment strategies.
"""

import random
from typing import Dict, List, Any, Tuple
from collections import defaultdict

from ..config import BEHAVIOR_CATEGORIES, TARGET_DISTRIBUTION


def identify_agents_to_adjust(analysis_data: Dict[str, Any], 
                            distribution_gap: Dict[str, float]) -> Dict[str, Dict[str, str]]:
    """
    Identify which agents should be adjusted based on the distribution gap.
    
    Args:
        analysis_data: Dictionary containing behavior analysis data
        distribution_gap: Gap between observed and target distributions
        
    Returns:
        Dictionary mapping agent names to their adjustment information
    """
    agents_by_behavior = defaultdict(list)
    
    # Group agents by their current behavior
    for agent_name, agent_data in analysis_data["agents"].items():
        behavior = agent_data.get("behavior", {}).get("classification", "UNKNOWN")
        agents_by_behavior[behavior].append(agent_name)
    
    # Determine how many agents to move from each category
    agents_to_adjust = {}
    total_agents = analysis_data["statistics"]["total_agents"]
    
    # Handle behaviors with negative gaps (too many agents)
    for behavior, gap in distribution_gap.items():
        if gap < 0 and behavior != "UNKNOWN":  # We have too many agents with this behavior
            # Calculate how many agents to move away from this behavior
            num_to_move = min(
                int(abs(gap) * total_agents),
                len(agents_by_behavior.get(behavior, []))
            )
            
            # If we have agents with this behavior, select some to move
            if behavior in agents_by_behavior and agents_by_behavior[behavior]:
                # Select random agents to move
                selected_agents = random.sample(agents_by_behavior[behavior], num_to_move)
                
                # For each selected agent, determine a target behavior to move to
                for agent in selected_agents:
                    # Find behaviors with positive gaps (need more agents)
                    target_behaviors = [b for b, g in distribution_gap.items() if g > 0]
                    if target_behaviors:
                        # Weight the selection by the size of the gap
                        weights = [max(0.001, distribution_gap[b]) for b in target_behaviors]
                        target_behavior = random.choices(target_behaviors, weights=weights, k=1)[0]
                        
                        agents_to_adjust[agent] = {
                            "current_behavior": behavior,
                            "target_behavior": target_behavior
                        }
    
    return agents_to_adjust


def calculate_optimization_effectiveness(optimization_data: Dict[str, Any], 
                                      base_optimization_path: str) -> Dict[str, Any]:
    """
    Analyze the effectiveness of optimization by tracking agent behavior changes.
    
    Args:
        optimization_data: Dictionary containing optimization run data
        base_optimization_path: Base path where optimization runs are stored
        
    Returns:
        Dictionary containing effectiveness analysis results
    """
    import os
    import json
    
    effectiveness_results = {}
    
    # Process each run separately
    for run_name, run_data in optimization_data.items():
        iterations = sorted(run_data["iterations"].keys())
        if len(iterations) < 2:
            print(f"Warning: Run {run_name} has fewer than 2 iterations, skipping effectiveness analysis")
            continue
        
        # Dictionary to store effectiveness data
        effectiveness_data = {}
        
        # For each iteration (except the last one), find agents that were adjusted
        for i in range(len(iterations) - 1):
            current_iter = iterations[i]
            next_iter = iterations[i + 1]
            
            # Path to the optimization log for the current iteration
            optimization_log_path = os.path.join(
                base_optimization_path, 
                run_name, 
                f"Iteration_{current_iter}", 
                "optimization_log.json"
            )
            
            # Skip if optimization log doesn't exist
            if not os.path.exists(optimization_log_path):
                print(f"Warning: Optimization log not found for {run_name}, Iteration_{current_iter}")
                continue
            
            try:
                # Load the optimization log
                with open(optimization_log_path, 'r', encoding='utf-8') as f:
                    optimization_log = json.load(f)
                
                # Get the list of adjusted agents
                adjusted_agents = optimization_log.get("agents_adjusted", [])
                
                # Get agent behaviors for both iterations
                current_behaviors = run_data["iterations"][current_iter].get("agent_behaviors", {})
                next_behaviors = run_data["iterations"][next_iter].get("agent_behaviors", {})
                
                # Track each agent's behavior change
                for agent_data in adjusted_agents:
                    agent_name = agent_data.get("agent_name")
                    behavior_change = agent_data.get("behavior_change", {})
                    
                    # Skip if missing data
                    if not agent_name or not behavior_change:
                        continue
                    
                    original_behavior = behavior_change.get("from")
                    target_behavior = behavior_change.get("to")
                    
                    # Get the actual behavior in the next iteration
                    actual_behavior = next_behaviors.get(agent_name)
                    
                    # Initialize agent data if not already present
                    if agent_name not in effectiveness_data:
                        effectiveness_data[agent_name] = {}
                    
                    # Store the behavior change data for this iteration
                    effectiveness_data[agent_name][current_iter] = {
                        "original_behavior": original_behavior,
                        "target_behavior": target_behavior,
                        "actual_behavior": actual_behavior,
                        "success": actual_behavior == target_behavior
                    }
            
            except Exception as e:
                print(f"Error processing optimization log for {run_name}, Iteration_{current_iter}: {e}")
        
        # Calculate success metrics
        total_adjustments = 0
        successful_adjustments = 0
        
        # Track success by behavior category
        success_by_target = {category: {"total": 0, "success": 0} for category in BEHAVIOR_CATEGORIES}
        
        for agent_name, iterations_data in effectiveness_data.items():
            for iteration, change_data in iterations_data.items():
                total_adjustments += 1
                if change_data["success"]:
                    successful_adjustments += 1
                
                target = change_data["target_behavior"]
                if target in success_by_target:
                    success_by_target[target]["total"] += 1
                    if change_data["success"]:
                        success_by_target[target]["success"] += 1
        
        # Calculate success rates
        overall_success_rate = successful_adjustments / total_adjustments if total_adjustments > 0 else 0
        
        success_rates_by_target = {}
        for category, counts in success_by_target.items():
            success_rates_by_target[category] = counts["success"] / counts["total"] if counts["total"] > 0 else 0
        
        # Add summary metrics to the data
        summary = {
            "total_adjustments": total_adjustments,
            "successful_adjustments": successful_adjustments,
            "overall_success_rate": overall_success_rate,
            "success_rates_by_target": success_rates_by_target
        }
        
        # Create the final effectiveness data structure
        effectiveness_results[run_name] = {
            "run_name": run_name,
            "summary": summary,
            "agent_data": effectiveness_data
        }
    
    return effectiveness_results


def select_agents_for_adjustment(agents_by_behavior: Dict[str, List[str]], 
                                behavior: str, num_to_select: int, 
                                selection_strategy: str = "random") -> List[str]:
    """
    Select agents for behavior adjustment using various strategies.
    
    Args:
        agents_by_behavior: Dictionary mapping behaviors to lists of agent names
        behavior: Behavior category to select agents from
        num_to_select: Number of agents to select
        selection_strategy: Strategy for selection ("random", "first", "last")
        
    Returns:
        List of selected agent names
    """
    available_agents = agents_by_behavior.get(behavior, [])
    
    if not available_agents or num_to_select <= 0:
        return []
    
    num_to_select = min(num_to_select, len(available_agents))
    
    if selection_strategy == "random":
        return random.sample(available_agents, num_to_select)
    elif selection_strategy == "first":
        return available_agents[:num_to_select]
    elif selection_strategy == "last":
        return available_agents[-num_to_select:]
    else:
        # Default to random
        return random.sample(available_agents, num_to_select)


def calculate_target_behavior_weights(distribution_gap: Dict[str, float]) -> Dict[str, float]:
    """
    Calculate weights for target behavior selection based on distribution gaps.
    
    Args:
        distribution_gap: Gap between observed and target distributions
        
    Returns:
        Dictionary mapping behaviors to their selection weights
    """
    weights = {}
    
    # Find behaviors with positive gaps (need more agents)
    positive_gaps = {b: g for b, g in distribution_gap.items() if g > 0}
    
    if not positive_gaps:
        # If no positive gaps, distribute equally among all behaviors
        return {b: 1.0 for b in BEHAVIOR_CATEGORIES}
    
    # Normalize gaps to create weights
    total_positive_gap = sum(positive_gaps.values())
    
    for behavior in BEHAVIOR_CATEGORIES:
        if behavior in positive_gaps:
            weights[behavior] = positive_gaps[behavior] / total_positive_gap
        else:
            weights[behavior] = 0.001  # Small weight for behaviors that don't need more agents
    
    return weights


def validate_persona_update(original_persona: Dict[str, Any], 
                           updated_persona: Dict[str, Any]) -> Tuple[bool, List[str]]:
    """
    Validate that a persona update is valid and contains required fields.
    
    Args:
        original_persona: Original persona data
        updated_persona: Updated persona data
        
    Returns:
        Tuple of (is_valid, list_of_errors)
    """
    errors = []
    
    # Required fields that should be present
    required_fields = ["name", "role", "age", "gender", "pronouns"]
    
    # Optional fields that can be modified
    modifiable_fields = [
        "personality_traits", "emotional_disposition", "motivations_goals",
        "communication_style", "knowledge_scope", "backstory"
    ]
    
    # Check that required fields are preserved
    for field in required_fields:
        if field not in updated_persona:
            errors.append(f"Required field '{field}' missing in updated persona")
        elif updated_persona.get(field) != original_persona.get(field):
            errors.append(f"Required field '{field}' should not be modified")
    
    # Check that modifiable fields are strings and within reasonable length
    for field in modifiable_fields:
        if field in updated_persona:
            value = updated_persona[field]
            if not isinstance(value, str):
                errors.append(f"Field '{field}' must be a string")
            elif len(value.split()) > 30:  # Reasonable word limit
                errors.append(f"Field '{field}' exceeds maximum length (30 words)")
    
    return len(errors) == 0, errors


def batch_process_agents(agent_tasks: List[Any], process_func: callable, 
                        max_workers: int = 32, batch_size: int = 50) -> List[Any]:
    """
    Process agent tasks in batches using parallel processing.
    
    Args:
        agent_tasks: List of agent processing tasks
        process_func: Function to process individual agents
        max_workers: Maximum number of worker threads
        batch_size: Size of each processing batch
        
    Returns:
        List of processing results
    """
    import concurrent.futures
    from concurrent.futures import ThreadPoolExecutor
    from tqdm import tqdm
    
    results = []
    
    # Process in batches to manage memory and API rate limits
    for i in range(0, len(agent_tasks), batch_size):
        batch = agent_tasks[i:i + batch_size]
        
        # Determine optimal number of workers for this batch
        workers = min(max_workers, len(batch))
        
        with ThreadPoolExecutor(max_workers=workers) as executor:
            # Create futures for this batch
            futures = [executor.submit(process_func, task) for task in batch]
            
            # Process results as they complete
            batch_results = []
            for future in tqdm(concurrent.futures.as_completed(futures), 
                             total=len(batch), desc=f"Processing batch {i//batch_size + 1}"):
                try:
                    result = future.result()
                    batch_results.append(result)
                except Exception as e:
                    print(f"Error in batch processing: {e}")
                    batch_results.append(None)
            
            results.extend(batch_results)
    
    return results
