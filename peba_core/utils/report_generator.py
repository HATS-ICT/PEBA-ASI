#!/usr/bin/env python
"""
Report generation utilities for PEBA-PEvo framework.

This module provides functions for generating various types of reports
including summary reports, optimization logs, and data exports.
"""

import os
import json
import pandas as pd
import datetime
from typing import Dict, List, Any, Optional

from ..config import BEHAVIOR_CATEGORIES, TARGET_DISTRIBUTION, METRIC_NAMES


def create_summary_report(optimization_data: Dict[str, Any], metrics_stats: Dict[int, Dict[str, Any]], 
                         behavior_stats: Dict[int, Dict[str, Any]], output_dir: str) -> str:
    """
    Create a comprehensive summary report of the optimization analysis.
    
    Args:
        optimization_data: Dictionary containing optimization run data
        metrics_stats: Dictionary containing metrics statistics by iteration
        behavior_stats: Dictionary containing behavior statistics by iteration
        output_dir: Directory to save the report
        
    Returns:
        Path to the saved report
    """
    os.makedirs(output_dir, exist_ok=True)
    
    # Create a summary report
    report = []
    report.append("# Optimization Analysis Summary Report")
    report.append("")
    report.append(f"Generated on: {datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    report.append("")
    
    # Add information about the optimization runs
    report.append("## Optimization Runs Analyzed")
    for run_name in optimization_data.keys():
        num_iterations = len(optimization_data[run_name]["iterations"])
        report.append(f"- {run_name}: {num_iterations} iterations")
    report.append("")
    
    # Add information about metrics
    report.append("## Metrics Analysis")
    for metric_name in ["kl_divergence", "js_divergence", "entropy_gap", "tvd"]:
        report.append(f"### {METRIC_NAMES.get(metric_name, metric_name)}")
        
        # Create a table of metrics by iteration
        report.append("| Iteration | Mean | Std | Min | Max |")
        report.append("| --- | --- | --- | --- | --- |")
        
        for iteration_num in sorted(metrics_stats.keys()):
            if metric_name in metrics_stats[iteration_num]:
                stats = metrics_stats[iteration_num][metric_name]
                report.append(f"| {iteration_num} | {stats['mean']:.4f} | {stats['std']:.4f} | {stats['min']:.4f} | {stats['max']:.4f} |")
        
        report.append("")
        
        # Calculate improvement from first to last iteration
        if metrics_stats:
            first_iteration = min(metrics_stats.keys())
            last_iteration = max(metrics_stats.keys())
            
            if (metric_name in metrics_stats[first_iteration] and 
                metric_name in metrics_stats[last_iteration]):
                first_mean = metrics_stats[first_iteration][metric_name]["mean"]
                last_mean = metrics_stats[last_iteration][metric_name]["mean"]
                
                if first_mean != 0:
                    improvement = (first_mean - last_mean) / first_mean * 100
                    report.append(f"Improvement from first to last iteration: {improvement:.2f}%")
                else:
                    report.append("Could not calculate improvement percentage (division by zero)")
        
        report.append("")
    
    # Add information about behavior distribution
    report.append("## Behavior Distribution Analysis")
    report.append("")
    
    # Create a table of final behavior distribution vs target
    if behavior_stats:
        last_iteration = max(behavior_stats.keys())
        
        report.append("### Final Behavior Distribution vs Target")
        report.append("| Category | Final Mean | Final Std | Target | Difference |")
        report.append("| --- | --- | --- | --- | --- |")
        
        for category in BEHAVIOR_CATEGORIES:
            if category in behavior_stats[last_iteration]:
                stats = behavior_stats[last_iteration][category]
                target = TARGET_DISTRIBUTION.get(category, 0)
                difference = stats["mean"] - target
                
                report.append(f"| {category} | {stats['mean']:.4f} | {stats['std']:.4f} | {target:.4f} | {difference:.4f} |")
    
    report.append("")
    
    # Save the report
    report_path = os.path.join(output_dir, 'summary_report.md')
    with open(report_path, 'w', encoding='utf-8') as f:
        f.write("\n".join(report))
    
    return report_path


def export_data_as_csv(metrics_stats: Dict[int, Dict[str, Any]], behavior_stats: Dict[int, Dict[str, Any]], 
                      output_dir: str) -> List[str]:
    """
    Export statistics data as CSV files for further analysis.
    
    Args:
        metrics_stats: Dictionary containing metrics statistics by iteration
        behavior_stats: Dictionary containing behavior statistics by iteration
        output_dir: Directory to save the CSV files
        
    Returns:
        List of paths to saved CSV files
    """
    os.makedirs(output_dir, exist_ok=True)
    
    saved_files = []
    
    # Export metrics data
    metrics_data = []
    for iteration_num in sorted(metrics_stats.keys()):
        row = {'Iteration': iteration_num}
        for metric_name in ["kl_divergence", "js_divergence", "entropy_gap", "tvd"]:
            if metric_name in metrics_stats[iteration_num]:
                stats = metrics_stats[iteration_num][metric_name]
                row[f"{metric_name}_mean"] = stats["mean"]
                row[f"{metric_name}_std"] = stats["std"]
                row[f"{metric_name}_min"] = stats["min"]
                row[f"{metric_name}_max"] = stats["max"]
        metrics_data.append(row)
    
    metrics_df = pd.DataFrame(metrics_data)
    metrics_path = os.path.join(output_dir, 'metrics_statistics.csv')
    metrics_df.to_csv(metrics_path, index=False)
    saved_files.append(metrics_path)
    
    # Export behavior distribution data
    behavior_data = []
    for iteration_num in sorted(behavior_stats.keys()):
        for category in BEHAVIOR_CATEGORIES:
            if category in behavior_stats[iteration_num]:
                stats = behavior_stats[iteration_num][category]
                row = {
                    'Iteration': iteration_num,
                    'Category': category,
                    'Mean': stats["mean"],
                    'Std': stats["std"],
                    'Min': stats["min"],
                    'Max': stats["max"],
                    'Target': TARGET_DISTRIBUTION.get(category, 0),
                    'Difference': stats["mean"] - TARGET_DISTRIBUTION.get(category, 0)
                }
                behavior_data.append(row)
    
    behavior_df = pd.DataFrame(behavior_data)
    behavior_path = os.path.join(output_dir, 'behavior_statistics.csv')
    behavior_df.to_csv(behavior_path, index=False)
    saved_files.append(behavior_path)
    
    return saved_files


def generate_optimization_log(optimization_run: str, iteration: str, observed_dist: Dict[str, float], 
                            distribution_gap: Dict[str, float], agents_adjusted: List[Dict[str, Any]], 
                            errors: List[str], api_usage: Dict[str, Any], output_dir: str) -> str:
    """
    Generate an optimization log file documenting the optimization process.
    
    Args:
        optimization_run: Name of the optimization run
        iteration: Name of the iteration
        observed_dist: Observed behavior distribution
        distribution_gap: Gap between observed and target distributions
        agents_adjusted: List of agent adjustment records
        errors: List of errors encountered
        api_usage: API usage statistics
        output_dir: Directory to save the log
        
    Returns:
        Path to the saved optimization log
    """
    os.makedirs(output_dir, exist_ok=True)
    
    # Initialize optimization log
    optimization_log = {
        "timestamp": datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
        "optimization_run": optimization_run,
        "iteration": iteration,
        "observed_distribution": observed_dist,
        "target_distribution": TARGET_DISTRIBUTION,
        "distribution_gap": distribution_gap,
        "agents_adjusted": agents_adjusted,
        "errors": errors,
        "api_usage": api_usage
    }
    
    # Save optimization log
    log_path = os.path.join(output_dir, "optimization_log.json")
    with open(log_path, 'w', encoding='utf-8') as f:
        json.dump(optimization_log, f, indent=4, ensure_ascii=False)
    
    return log_path


def generate_human_comparison_data(agent_results: Dict[str, Any], output_file: str, 
                                 agent_context_func: callable) -> str:
    """
    Generate a JSON document for human comparison with the LLM classifier.
    
    Args:
        agent_results: Dictionary of agent classification results
        output_file: Path to save the JSON output
        agent_context_func: Function to get agent context from agent data
        
    Returns:
        Path to the saved comparison data file
    """
    comparison_data = []
    
    for agent_name, agent_data in agent_results.items():
        behavior_data = agent_data.get("behavior", {})
        persona = agent_data.get("persona", {})
        
        # Skip agents with errors or missing data
        if "error" in behavior_data:
            continue
        
        # Format the context in a structured way
        formatted_context = f"AGENT: {agent_name}\n\n"
        formatted_context += f"PERSONA:\n"
        formatted_context += f"Name: {persona.get('name', 'Unknown')}\n"
        formatted_context += f"Occupation: {persona.get('occupation', 'Unknown')}\n"
        formatted_context += f"Age: {persona.get('age', 'Unknown')}\n"
        formatted_context += f"Gender: {persona.get('gender', 'Unknown')}\n\n"
        
        # Create the comparison entry
        entry = {
            "agent_id": agent_name,
            "context": formatted_context,
            "reasoning": behavior_data.get("reasoning", ""),
            "classification": behavior_data.get("classification", "UNKNOWN"),
            "ranking": behavior_data.get("ranking", [])
        }
        
        comparison_data.append(entry)
    
    # Save the comparison data to file
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(comparison_data, f, indent=2)
    
    return output_file


def generate_label_studio_data(agent_results: Dict[str, Any], output_file: str, 
                              agent_context_func: callable) -> str:
    """
    Generate a JSON file formatted for Label Studio with just the context text.
    
    Args:
        agent_results: Dictionary of agent classification results
        output_file: Path to save the JSON output
        agent_context_func: Function to get agent context from agent data
        
    Returns:
        Path to the saved Label Studio data file
    """
    label_studio_data = []
    
    for agent_name, agent_data in agent_results.items():
        behavior_data = agent_data.get("behavior", {})
        persona = agent_data.get("persona", {})
        
        # Skip agents with errors or missing data
        if "error" in behavior_data:
            continue
        
        # Format the context in a structured way
        formatted_context = f"AGENT: {agent_name}\n\n"
        formatted_context += f"PERSONA:\n"
        formatted_context += f"Name: {persona.get('name', 'Unknown')}\n"
        formatted_context += f"Occupation: {persona.get('occupation', 'Unknown')}\n"
        formatted_context += f"Age: {persona.get('age', 'Unknown')}\n"
        formatted_context += f"Gender: {persona.get('gender', 'Unknown')}\n\n"
        
        # Create the label studio entry
        entry = {
            "data": {
                "text": formatted_context
            }
        }
        
        label_studio_data.append(entry)
    
    # Save the label studio data to file
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(label_studio_data, f, indent=2)
    
    return output_file


def print_behavior_summary(behavior_counts: Dict[str, int], distribution_metrics: Optional[Dict[str, float]] = None, 
                          topk_metrics: Optional[Dict[int, Dict[str, float]]] = None, 
                          token_usage: Optional[Dict[str, Any]] = None, 
                          unity_token_usage: Optional[Dict[str, Any]] = None):
    """
    Print a comprehensive behavior classification summary to console.
    
    Args:
        behavior_counts: Dictionary mapping behaviors to their counts
        distribution_metrics: Optional distribution metrics
        topk_metrics: Optional top-k metrics
        token_usage: Optional API token usage statistics
        unity_token_usage: Optional Unity token usage statistics
    """
    print("\nBehavior Classification Summary:")
    print("-" * 40)
    
    total = sum(behavior_counts.values())
    for behavior, count in sorted(behavior_counts.items(), key=lambda x: x[1], reverse=True):
        percentage = (count / total) * 100 if total > 0 else 0
        print(f"{behavior}: {count} agents ({percentage:.1f}%)")
    
    print("-" * 40)
    print(f"Total: {total} agents")
    
    if distribution_metrics:
        print(f"Distribution Metrics:")
        print(f"  KL Divergence: {distribution_metrics['kl_divergence']:.4f}")
        print(f"  JS Divergence: {distribution_metrics['js_divergence']:.4f}")
        print(f"  Entropy Gap (DeltaH): {distribution_metrics['entropy_gap']:.4f}")
        print(f"  Total Variation Distance: {distribution_metrics['tvd']:.4f}")
    
    if token_usage:
        print("\nToken Usage Summary:")
        print("-" * 40)
        print(f"Total API Requests: {token_usage.get('total_requests', 'N/A')}")
        print(f"Prompt Tokens: {token_usage.get('prompt_tokens', 'N/A')}")
        print(f"Completion Tokens: {token_usage.get('completion_tokens', 'N/A')}")
        print(f"Total Tokens: {token_usage.get('total_tokens', 'N/A')}")
    
    if unity_token_usage:
        print("\nUnity Token Usage:")
        print("-" * 40)
        print(f"Model: {unity_token_usage.get('model', 'Unknown')}")
        print(f"Total Requests: {unity_token_usage.get('total_requests', 'Unknown')}")
        print(f"Prompt Tokens: {unity_token_usage.get('prompt_tokens', 'Unknown')}")
        print(f"Completion Tokens: {unity_token_usage.get('completion_tokens', 'Unknown')}")
        print(f"Total Cost: ${unity_token_usage.get('total_cost', 'Unknown')}")
    
    if topk_metrics:
        print("\nTop-k Credit Sharing Metrics:")
        print("-" * 40)
        for k in sorted(topk_metrics.keys()):
            print(f"Top-{k} metrics:")
            print(f"  KL Divergence: {topk_metrics[k]['kl_divergence']:.4f}")
            print(f"  JS Divergence: {topk_metrics[k]['js_divergence']:.4f}")
            print(f"  Total Variation Distance: {topk_metrics[k]['tvd']:.4f}")


def print_optimization_summary(observed_dist: Dict[str, float], distribution_gap: Dict[str, float], 
                              agents_to_adjust: Dict[str, Any], token_usage: Dict[str, Any], 
                              errors: List[str]):
    """
    Print a comprehensive optimization summary to console.
    
    Args:
        observed_dist: Current behavior distribution
        distribution_gap: Gap between observed and target distributions  
        agents_to_adjust: Dictionary of agents to be adjusted
        token_usage: API token usage statistics
        errors: List of errors encountered
    """
    print("\nCurrent Behavior Distribution:")
    print("-" * 40)
    for behavior, value in observed_dist.items():
        target_value = TARGET_DISTRIBUTION.get(behavior, 0)
        gap = distribution_gap.get(behavior, 0)
        print(f"{behavior}: {value:.2f} (Target: {target_value:.2f}, Gap: {gap:.2f})")
    
    print(f"\nIdentified {len(agents_to_adjust)} agents to adjust")
    
    print("\nToken Usage Summary:")
    print("-" * 40)
    print(f"Total API Requests: {token_usage.get('total_requests', 0)}")
    print(f"Prompt Tokens: {token_usage.get('prompt_tokens', 0)}")
    print(f"Completion Tokens: {token_usage.get('completion_tokens', 0)}")
    print(f"Total Tokens: {token_usage.get('total_tokens', 0)}")
    
    if errors:
        print("\nErrors encountered during optimization:")
        for error in errors:
            print(f"- {error}")
