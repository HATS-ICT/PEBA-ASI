#!/usr/bin/env python
"""
Agent Behavior Classifier (Refactored)

This script classifies the behavior of all agents in a simulation into behavior categories
based on their memories and creates visualization plots. It also compares the observed distribution
with ground truth distribution using various metrics.
"""

import os
import sys
import json
import time
import argparse
import multiprocessing
from functools import partial
from tqdm import tqdm

# Add the peba_core package to the path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from peba_core.config import (
    BASE_SIMULATION_PATH,
    DEFAULT_SIMULATION_FOLDER,
    BEHAVIOR_CATEGORIES,
    GROUND_TRUTH_DISTRIBUTION,
    DEBUG
)
from peba_core.utils.data_loader import (
    load_simulation_data,
    find_simulation_folders,
    get_agent_context,
    parse_token_usage_log
)
from peba_core.utils.metrics import (
    calculate_distribution_metrics,
    calculate_topk_distribution,
    calculate_behavior_counts
)
from peba_core.utils.visualization import (
    create_behavior_comparison_plot,
    create_topk_distributions_plot
)
from peba_core.utils.llm_client import LLMClient
from peba_core.utils.report_generator import (
    generate_human_comparison_data,
    generate_label_studio_data,
    print_behavior_summary
)


class BehaviorClassifier:
    """Main class for behavior classification workflow."""
    
    def __init__(self, api_key=None):
        """Initialize the behavior classifier."""
        self.llm_client = LLMClient(api_key)
        
    def process_agent(self, agent_data_tuple):
        """Process a single agent for behavior classification."""
        agent_name, agent_data = agent_data_tuple
        
        try:
            # Get agent context
            context = get_agent_context(agent_data)
            
            # Classify behavior using LLM
            behavior_result = self.llm_client.classify_agent_behavior(agent_data, context)
            
            return {
                "agent_name": agent_name,
                "persona": behavior_result.get("persona", {}),
                "behavior": behavior_result
            }
        except Exception as e:
            if DEBUG:
                print(f"Error processing agent {agent_name}: {e}")
            return None
    
    def classify_simulation(self, agent_data_dict):
        """Classify behaviors for all agents in a simulation."""
        # Prepare agent tasks for parallel processing
        agent_tasks = list(agent_data_dict.items())
        
        if not agent_tasks:
            print("No agent data found for classification.")
            return {}
        
        print(f"Classifying behaviors for {len(agent_tasks)} agents...")
        
        # Try parallel processing, fall back to sequential on Windows pickle issues
        try:
            # Process agents in parallel
            with multiprocessing.Pool(processes=multiprocessing.cpu_count()) as pool:
                results = list(tqdm(pool.imap(self.process_agent, agent_tasks), total=len(agent_tasks)))
        except Exception as e:
            print(f"Parallel processing failed ({e}), falling back to sequential processing...")
            # Fall back to sequential processing
            results = []
            for task in tqdm(agent_tasks, desc="Processing agents"):
                result = self.process_agent(task)
                results.append(result)
        
        # Filter out None results (failed processing)
        valid_results = [r for r in results if r is not None]
        
        if not valid_results:
            print("No valid classification results obtained.")
            return {}
        
        # Organize results by agent
        classified_agents = {}
        for result in valid_results:
            agent_name = result["agent_name"]
            classified_agents[agent_name] = {
                "persona": result["persona"],
                "behavior": result["behavior"]
            }
        
        return classified_agents
    
    def analyze_and_visualize(self, classified_agents, output_dir, simulation_id):
        """Analyze classified behaviors and create visualizations."""
        
        # Calculate behavior counts
        behavior_counts = calculate_behavior_counts(classified_agents)
        
        # Create analysis data structure
        analysis_data = {
            "simulation_id": simulation_id,
            "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
            "agents": classified_agents,
            "statistics": {
                "total_agents": len(classified_agents),
                "behavior": behavior_counts
            }
        }
        
        # Track token usage
        total_token_usage = {"prompt_tokens": 0, "completion_tokens": 0, "total_tokens": 0}
        total_requests = 0
        
        for agent_data in classified_agents.values():
            if "token_usage" in agent_data.get("behavior", {}):
                token_usage = agent_data["behavior"]["token_usage"]
                total_token_usage["prompt_tokens"] += token_usage.get("prompt_tokens", 0)
                total_token_usage["completion_tokens"] += token_usage.get("completion_tokens", 0)
                total_token_usage["total_tokens"] += token_usage.get("total_tokens", 0)
                total_requests += 1
        
        analysis_data["statistics"]["api_usage"] = {
            "total_requests": total_requests,
            "token_usage": total_token_usage
        }
        
        # Create plots directory
        plots_dir = os.path.join(output_dir, "Plots")
        
        # Create comparison visualization with ground truth
        create_behavior_comparison_plot(behavior_counts, plots_dir, GROUND_TRUTH_DISTRIBUTION)
        
        # Calculate and visualize top-k distributions
        k_values = [1, 2, 3, 4, 5, 6]
        topk_metrics = {}
        for k in k_values:
            topk_dist = calculate_topk_distribution(classified_agents, k=k)
            topk_metrics[k] = calculate_distribution_metrics(topk_dist, GROUND_TRUTH_DISTRIBUTION)
        
        create_topk_distributions_plot(classified_agents, plots_dir, k_values)
        
        # Calculate distribution metrics
        total_observed = sum(behavior_counts.values())
        observed_dist = {k: v/total_observed for k, v in behavior_counts.items()}
        distribution_metrics = calculate_distribution_metrics(observed_dist, GROUND_TRUTH_DISTRIBUTION)
        
        # Add metrics to analysis data
        analysis_data["statistics"]["distribution_metrics"] = distribution_metrics
        analysis_data["statistics"]["topk_metrics"] = topk_metrics
        
        return analysis_data
    
    def process_simulation(self, simulation_path, output_path=None, direct_path=False):
        """Process a single simulation folder."""
        print(f"Processing simulation: {simulation_path}")
        
        # Load simulation data
        agent_logs_folder, agent_data = load_simulation_data(
            simulation_path, 
            BASE_SIMULATION_PATH, 
            DEFAULT_SIMULATION_FOLDER, 
            direct_path
        )
        
        if not agent_data:
            print(f"Error: Could not load agent data from {simulation_path}")
            return False
        
        print(f"Found {len(agent_data)} agent files.")
        
        # Set output paths
        if direct_path:
            base_output_dir = simulation_path
        else:
            base_output_dir = os.path.join(BASE_SIMULATION_PATH, DEFAULT_SIMULATION_FOLDER, simulation_path)
        
        if output_path:
            base_output_dir = output_path
        
        # Classify agent behaviors
        classified_agents = self.classify_simulation(agent_data)
        
        if not classified_agents:
            print("No agents were successfully classified.")
            return False
        
        # Analyze and create visualizations
        simulation_id = os.path.basename(simulation_path)
        analysis_data = self.analyze_and_visualize(classified_agents, base_output_dir, simulation_id)
        
        # Save analysis results
        output_file = os.path.join(base_output_dir, "behavior_analysis.json")
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(analysis_data, f, indent=2)
        
        # Generate additional data files
        human_comparison_file = os.path.join(base_output_dir, "human_comparison_data.json")
        label_studio_file = os.path.join(base_output_dir, "label_studio_data.json")
        
        def get_context_from_logs(agent_name):
            """Helper function to get agent context from logs."""
            if agent_name in agent_data:
                return get_agent_context(agent_data[agent_name])
            return {"memories": "", "timeline": ""}
        
        generate_human_comparison_data(classified_agents, human_comparison_file, get_context_from_logs)
        generate_label_studio_data(classified_agents, label_studio_file, get_context_from_logs)
        
        # Try to load Unity token usage if available
        unity_token_usage = None
        if direct_path:
            token_log_path = os.path.join(simulation_path, "TokenUsage.txt")
        else:
            token_log_path = os.path.join(BASE_SIMULATION_PATH, DEFAULT_SIMULATION_FOLDER, simulation_path, "TokenUsage.txt")
        
        unity_token_usage = parse_token_usage_log(token_log_path)
        if unity_token_usage:
            analysis_data["statistics"]["unity_api_usage"] = unity_token_usage
        
        # Print summary
        behavior_counts = calculate_behavior_counts(classified_agents)
        distribution_metrics = analysis_data["statistics"]["distribution_metrics"]
        topk_metrics = analysis_data["statistics"]["topk_metrics"]
        api_usage = analysis_data["statistics"]["api_usage"]
        
        print_behavior_summary(
            behavior_counts=behavior_counts,
            distribution_metrics=distribution_metrics,
            topk_metrics=topk_metrics,
            token_usage=api_usage["token_usage"],
            unity_token_usage=unity_token_usage
        )
        
        print(f"\nResults saved to: {output_file}")
        return True


def main():
    """Main function to run the behavior classification script."""
    parser = argparse.ArgumentParser(description='Classify agent behaviors from simulation data.')
    parser.add_argument('--folder', type=str, default="Simulation_2025-05-20_01-24-17",
                        help='Simulation folder name')
    parser.add_argument('--batch', action='store_true', default=False,
                        help='Process multiple simulation folders in a batch directory')
    parser.add_argument('--direct-path', action='store_true', default=False,
                        help='Treat folder argument as a direct path to the simulation folder')
    parser.add_argument('--output', type=str, default=None,
                        help='Custom output directory for results')
    args = parser.parse_args()
    
    # Initialize the behavior classifier
    classifier = BehaviorClassifier()
    
    if args.direct_path:
        # Direct path mode - use the provided path directly
        if not os.path.exists(args.folder):
            print(f"Error: Simulation folder not found: {args.folder}")
            return 1
        
        print(f"Processing simulation at direct path: {args.folder}")
        success = classifier.process_simulation(args.folder, args.output, direct_path=True)
        return 0 if success else 1
        
    elif args.batch:
        # Batch processing mode
        batch_folder = os.path.join(BASE_SIMULATION_PATH, "AblationStudies", args.folder)
        if not os.path.exists(batch_folder):
            print(f"Error: Batch folder not found: {batch_folder}")
            return 1
            
        print(f"Batch processing simulations in: {batch_folder}")
        simulation_folders = find_simulation_folders(batch_folder)
        
        if not simulation_folders:
            print("No simulation folders found in the batch directory.")
            return 1
            
        print(f"Found {len(simulation_folders)} simulation folders to process.")
        
        success_count = 0
        for sim_folder in simulation_folders:
            print(f"\n{'='*50}")
            print(f"Processing simulation: {sim_folder}")
            print(f"{'='*50}")
            
            sim_path = os.path.join(batch_folder, sim_folder)
            output_path = os.path.join(args.output, sim_folder) if args.output else None
            
            if classifier.process_simulation(sim_path, output_path, direct_path=True):
                success_count += 1
        
        print(f"\nBatch processing complete: {success_count}/{len(simulation_folders)} simulations processed successfully.")
        return 0
        
    else:
        # Single simulation mode
        success = classifier.process_simulation(args.folder, args.output)
        return 0 if success else 1


if __name__ == "__main__":
    sys.exit(main())
