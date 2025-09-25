#!/usr/bin/env python
"""
Agent Personality Optimizer (Refactored)

This script analyzes behavior distribution results, identifies mismatches between observed and target behaviors,
and uses an LLM to suggest personality adjustments to nudge agents toward the target distribution.
"""

import os
import sys
import json
import argparse
import datetime
from typing import Dict, Any, List, Tuple
from concurrent.futures import ThreadPoolExecutor
import concurrent.futures
from tqdm import tqdm

# Add the peba_core package to the path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from peba_core.config import (
    BASE_OPTIMIZATION_PATH,
    TARGET_DISTRIBUTION,
    BEHAVIOR_CATEGORIES,
    DEFAULT_MAX_WORKERS
)
from peba_core.utils.data_loader import (
    load_json_file,
    load_agent_data
)
from peba_core.utils.metrics import analyze_distribution_gap
from peba_core.utils.optimization import (
    identify_agents_to_adjust,
    validate_persona_update
)
from peba_core.utils.llm_client import LLMClient
from peba_core.utils.report_generator import (
    generate_optimization_log,
    print_optimization_summary
)


class PersonaOptimizer:
    """Main class for persona optimization workflow."""
    
    def __init__(self, api_key=None):
        """Initialize the persona optimizer."""
        self.llm_client = LLMClient(api_key)
    
    def load_optimization_data(self, optimization_run: str, iteration: str) -> Tuple[Dict[str, Any], Dict[str, Any]]:
        """Load personas and behavior analysis data from an optimization run."""
        base_dir = os.path.join(BASE_OPTIMIZATION_PATH, optimization_run, iteration)
        
        # Load personas
        personas_path = os.path.join(base_dir, "personas.json")
        personas_data = load_json_file(personas_path)
        if not personas_data:
            raise FileNotFoundError(f"Personas file not found or could not be loaded: {personas_path}")
        
        # Load behavior analysis
        analysis_path = os.path.join(base_dir, "behavior_analysis.json")
        analysis_data = load_json_file(analysis_path)
        if not analysis_data:
            raise FileNotFoundError(f"Behavior analysis file not found or could not be loaded: {analysis_path}")
        
        return personas_data, analysis_data
    
    def process_agent_optimization(self, agent_task) -> Tuple[str, Tuple[Dict[str, Any], str, Dict[str, Any]]]:
        """Process a single agent for persona optimization."""
        agent_name, adjustment, persona, agent_full_data = agent_task
        
        result = self.llm_client.optimize_agent_personality(
            agent_name,
            adjustment["current_behavior"],
            adjustment["target_behavior"],
            persona,
            agent_full_data
        )
        
        return agent_name, result
    
    def optimize_personas_parallel(self, personas_data: Dict[str, Any], analysis_data: Dict[str, Any], 
                                 optimization_run: str, iteration: str, 
                                 max_workers: int = DEFAULT_MAX_WORKERS) -> str:
        """Optimize personas using parallel processing."""
        
        # Analyze distribution gap
        distribution_gap, observed_dist = analyze_distribution_gap(analysis_data)
        
        print("\nCurrent Behavior Distribution:")
        for behavior, value in observed_dist.items():
            target_value = TARGET_DISTRIBUTION.get(behavior, 0)
            gap = distribution_gap.get(behavior, 0)
            print(f"{behavior}: {value:.2f} (Target: {target_value:.2f}, Gap: {gap:.2f})")
        
        # Calculate UNKNOWN percentage
        total_agents = analysis_data["statistics"]["total_agents"]
        unknown_count = analysis_data["statistics"]["behavior"].get("UNKNOWN", 0)
        unknown_percentage = (unknown_count / total_agents) if total_agents > 0 else 0
        print(f"UNKNOWN: {unknown_percentage:.2f} ({unknown_count} agents)")
        
        # Identify agents to adjust
        agents_to_adjust = identify_agents_to_adjust(analysis_data, distribution_gap)
        
        print(f"\nIdentified {len(agents_to_adjust)} agents to adjust")
        
        # Create a copy of the personas data to modify
        updated_personas = personas_data.copy()
        
        # Track errors and token usage
        errors = []
        total_token_usage = {"prompt_tokens": 0, "completion_tokens": 0, "total_tokens": 0}
        total_requests = 0
        
        # Load agent data from the original simulation for context
        agent_data_cache = self._load_agent_context_data(optimization_run, iteration, agents_to_adjust.keys())
        
        # Prepare agent tasks for parallel processing
        agent_tasks = []
        original_personas = {}
        
        for agent_name, adjustment in agents_to_adjust.items():
            # Find the agent in the personas data
            agent_persona = self._find_agent_persona(updated_personas, agent_name)
            if agent_persona:
                # Store original persona for logging
                original_personas[agent_name] = agent_persona.copy()
                
                agent_tasks.append((
                    agent_name,
                    adjustment,
                    agent_persona,
                    agent_data_cache.get(agent_name, None)
                ))
            else:
                error_msg = f"Agent {agent_name} not found in personas data"
                errors.append(error_msg)
        
        # Process agents in parallel
        print(f"\nOptimizing {len(agent_tasks)} agent personalities in parallel...")
        
        optimization_results = []
        results = {}
        
        # Determine optimal number of workers
        workers = min(max_workers, len(agent_tasks)) if agent_tasks else 1
        
        if not agent_tasks:
            print("No agents need optimization based on current distribution.")
            
            # Still generate an optimization log for completeness
            api_usage = {"total_requests": 0, "token_usage": total_token_usage}
            
            output_dir = os.path.join(BASE_OPTIMIZATION_PATH, optimization_run, iteration)
            log_path = generate_optimization_log(
                optimization_run=optimization_run,
                iteration=iteration,
                observed_dist=observed_dist,
                distribution_gap=distribution_gap,
                agents_adjusted=[],
                errors=errors,
                api_usage=api_usage,
                output_dir=output_dir
            )
            
            # Return the original personas file path since no changes were made
            return os.path.join(output_dir, "personas.json")
        
        with ThreadPoolExecutor(max_workers=workers) as executor:
            # Create futures for all tasks
            future_to_agent = {
                executor.submit(self.process_agent_optimization, task): task[0] for task in agent_tasks
            }
            
            # Process results as they complete
            for future in tqdm(concurrent.futures.as_completed(future_to_agent), total=len(agent_tasks)):
                agent_name = future_to_agent[future]
                try:
                    agent_name, (updated_persona, error, token_usage) = future.result()
                    
                    # Track token usage if available
                    if token_usage:
                        total_token_usage["prompt_tokens"] += token_usage.get("prompt_tokens", 0)
                        total_token_usage["completion_tokens"] += token_usage.get("completion_tokens", 0)
                        total_token_usage["total_tokens"] += token_usage.get("total_tokens", 0)
                        total_requests += 1
                    
                    if error:
                        error_msg = f"Error optimizing {agent_name}: {error}"
                        errors.append(error_msg)
                    else:
                        # Validate the persona update
                        original = original_personas.get(agent_name, {})
                        is_valid, validation_errors = validate_persona_update(original, updated_persona)
                        
                        if is_valid:
                            results[agent_name] = updated_persona
                            
                            # Add to optimization results for logging
                            adjustment_info = agents_to_adjust.get(agent_name, {})
                            optimization_results.append({
                                "agent_name": agent_name,
                                "behavior_change": {
                                    "from": adjustment_info.get("current_behavior", "UNKNOWN"),
                                    "to": adjustment_info.get("target_behavior", "UNKNOWN")
                                },
                                "original_persona": original,
                                "updated_persona": updated_persona,
                                "token_usage": token_usage
                            })
                        else:
                            error_msg = f"Persona validation failed for {agent_name}: {validation_errors}"
                            errors.append(error_msg)
                        
                except Exception as exc:
                    error_msg = f"Agent {agent_name} generated an exception: {exc}"
                    errors.append(error_msg)
        
        # Update personas with the results
        self._update_personas_data(updated_personas, results)
        
        # Save the updated personas to a new file
        output_dir = os.path.join(BASE_OPTIMIZATION_PATH, optimization_run, iteration)
        output_path = os.path.join(output_dir, "personas_updated.json")
        
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(updated_personas, f, indent=4, ensure_ascii=False)
        
        # Generate and save optimization log
        api_usage = {
            "total_requests": total_requests,
            "token_usage": total_token_usage
        }
        
        log_path = generate_optimization_log(
            optimization_run=optimization_run,
            iteration=iteration,
            observed_dist=observed_dist,
            distribution_gap=distribution_gap,
            agents_adjusted=optimization_results,
            errors=errors,
            api_usage=api_usage,
            output_dir=output_dir
        )
        
        print(f"\nOptimization log saved to: {log_path}")
        
        # Print summary
        print_optimization_summary(
            observed_dist=observed_dist,
            distribution_gap=distribution_gap,
            agents_to_adjust=agents_to_adjust,
            token_usage=total_token_usage,
            errors=errors
        )
        
        return output_path
    
    def _load_agent_context_data(self, optimization_run: str, iteration: str, agent_names: List[str]) -> Dict[str, Any]:
        """Load agent data for context during optimization."""
        agent_logs_folder = os.path.join(BASE_OPTIMIZATION_PATH, optimization_run, iteration, "AgentLogs")
        
        if not os.path.exists(agent_logs_folder):
            print(f"Warning: Agent logs folder not found: {agent_logs_folder}")
            return {}
        
        print(f"\nLoading agent data from: {agent_logs_folder}")
        
        agent_data_cache = {}
        for agent_name in agent_names:
            agent_file_path = os.path.join(agent_logs_folder, f"{agent_name}.json")
            agent_data = load_json_file(agent_file_path)
            
            if agent_data:
                agent_data_cache[agent_name] = agent_data
            else:
                print(f"Warning: Could not load agent data for {agent_name}")
        
        return agent_data_cache
    
    def _find_agent_persona(self, personas_data: Dict[str, Any], agent_name: str) -> Dict[str, Any]:
        """Find an agent's persona in the personas data."""
        for persona in personas_data.get("personas", []):
            if persona.get("name") == agent_name:
                return persona
        return None
    
    def _update_personas_data(self, personas_data: Dict[str, Any], results: Dict[str, Dict[str, Any]]):
        """Update personas data with optimization results."""
        for i, persona in enumerate(personas_data.get("personas", [])):
            agent_name = persona.get("name")
            if agent_name in results:
                personas_data["personas"][i] = results[agent_name]


def main():
    """Main function to run the persona optimization script."""
    parser = argparse.ArgumentParser(description='Optimize agent personalities based on behavior analysis.')
    parser.add_argument('--run', type=str, default="BehaviorOptimizer_2025-05-08_02-23-23",
                        help='Optimization run folder name')
    parser.add_argument('--iteration', type=str, default="Iteration_1",
                        help='Iteration folder name within the optimization run')
    parser.add_argument('--max-workers', type=int, default=DEFAULT_MAX_WORKERS,
                        help='Maximum number of parallel workers for optimization')
    args = parser.parse_args()
    
    try:
        # Initialize the persona optimizer
        optimizer = PersonaOptimizer()
        
        # Load data
        print(f"Loading data from {args.run}/{args.iteration}...")
        personas_data, analysis_data = optimizer.load_optimization_data(args.run, args.iteration)
        
        # Optimize personas
        output_path = optimizer.optimize_personas_parallel(
            personas_data=personas_data,
            analysis_data=analysis_data,
            optimization_run=args.run,
            iteration=args.iteration,
            max_workers=args.max_workers
        )
        
        print(f"\nOptimization complete. Updated personas saved to: {output_path}")
        return 0
        
    except Exception as e:
        print(f"Error: {e}")
        return 1


if __name__ == "__main__":
    sys.exit(main())
