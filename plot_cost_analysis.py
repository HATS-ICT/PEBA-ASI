#!/usr/bin/env python
"""
Cost Analysis Plot Generator

This script creates a 2x2 subplot analyzing token usage, costs, and efficiency
of different models during optimization runs. It extracts data from simulation_metadata.json,
optimization_log.json, and behavior_analysis.json files.
"""

import os
import sys
import json
import argparse
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.ticker import MaxNLocator
import pandas as pd
import glob
from collections import defaultdict

# ======= SETTINGS =======
# Base path where optimization runs are stored
BASE_PATH = os.path.join(os.path.expanduser("~"), "AppData", "LocalLow", "...", "OptimizationRuns")

# Base path where evaluation results will be stored
EVAL_RESULTS_PATH = os.path.join(os.path.expanduser("~"), "AppData", "LocalLow", "...", "EvaluationResults")

# Model configurations
MODEL_CONFIGS = {
    "GPT-4.1 Mini": [
        "PersonaEvolveGPT41Mini_2025-05-18_23-04-22"
    ],
    "GPT-4o Mini": [
        "PersonaEvolveGPT4oMini_2025-05-18_17-55-19",
        "PersonaEvolveGPT4oMini_2025-05-18_18-20-50",
        "PersonaEvolveGPT4oMini_2025-05-18_18-48-35",
        "PersonaEvolveGPT4oMini_2025-05-18_19-16-10",
        "PersonaEvolveGPT4oMini_2025-05-18_19-42-05"
    ],
    "Gemini 2.5 Flash": [
        "PersonaEvolveGemini25flash_2025-05-19_00-09-55",
        "PersonaEvolveGemini25flash_2025-05-19_00-37-18",
        "PersonaEvolveGemini25flash_2025-05-19_01-08-07",
    ],
    "DeepSeek V3": [
        "PersonaEvolveDeepSeekV3_2025-05-19_02-43-40",
        "PersonaEvolveDeepSeekV3_2025-05-19_03-10-52",
        "PersonaEvolveDeepSeekV3_2025-05-19_03-40-28",
    ]
}

# Model to API name mapping
MODEL_TO_API = {
    "GPT-4.1 Mini": "gpt-4.1-mini",
    "GPT-4o Mini": "gpt-4o-mini",
    "Gemini 2.5 Flash": "google/gemini-2.5-flash-preview",
    "DeepSeek V3": "deepseek/deepseek-chat"
}

# Token costs per million tokens (in USD)
TOKEN_COSTS = {
    "gpt-4.1-mini": {
        "prompt_tokens": 0.4,
        "cached_tokens": 0.1,
        "completion_tokens": 1.6
    },
    "gpt-4o-mini": {
        "prompt_tokens": 0.15,
        "cached_tokens": 0.075,
        "completion_tokens": 0.6
    },
    "google/gemini-2.5-flash-preview": {
        "prompt_tokens": 0.15,
        "cached_tokens": 0.0,
        "completion_tokens": 0.6
    },
    "deepseek/deepseek-chat": {
        "prompt_tokens": 0.38,
        "cached_tokens": 0.0,
        "completion_tokens": 0.89
    }
}

# Colors for each model
MODEL_COLORS = {
    "GPT-4.1 Mini": "#1f77b4",    # blue
    "GPT-4o Mini": "#ff7f0e",     # orange
    "Gemini 2.5 Flash": "#2ca02c", # green
    "DeepSeek V3": "#d62728"      # red
}

def load_cost_data(model_name, optimization_runs):
    """Load cost and token usage data from optimization runs."""
    all_data = []
    
    for run_name in optimization_runs:
        run_path = os.path.join(BASE_PATH, run_name)
        if not os.path.exists(run_path):
            print(f"Warning: Optimization run path not found: {run_path}")
            continue
        
        # Find all iteration folders
        iteration_folders = [f for f in os.listdir(run_path) 
                            if os.path.isdir(os.path.join(run_path, f)) and f.startswith("Iteration_")]
        
        if not iteration_folders:
            print(f"Warning: No iteration folders found in {run_path}")
            continue
        
        # Sort iteration folders numerically
        iteration_folders.sort(key=lambda x: int(x.split("_")[1]))
        
        # Process each iteration
        for iteration_folder in iteration_folders:
            iteration_num = int(iteration_folder.split("_")[1])
            iteration_path = os.path.join(run_path, iteration_folder)
            
            # Initialize data for this iteration
            iteration_data = {
                "model": model_name,
                "run_name": run_name,
                "iteration": iteration_num,
                "simulation_prompt_tokens": 0,
                "simulation_completion_tokens": 0,
                "simulation_cached_tokens": 0,
                "optimization_prompt_tokens": 0,
                "optimization_completion_tokens": 0,
                "analysis_prompt_tokens": 0,
                "analysis_completion_tokens": 0,
                "total_cost_usd": 0,
                "kl_divergence": None
            }
            
            # Load simulation metadata
            sim_metadata_path = os.path.join(iteration_path, "simulation_metadata.json")
            if os.path.exists(sim_metadata_path):
                try:
                    with open(sim_metadata_path, 'r', encoding='utf-8') as f:
                        sim_data = json.load(f)
                    
                    if "llm_usage" in sim_data:
                        llm_usage = sim_data["llm_usage"]
                        iteration_data["simulation_prompt_tokens"] = llm_usage.get("prompt_tokens", 0)
                        iteration_data["simulation_completion_tokens"] = llm_usage.get("completion_tokens", 0)
                        iteration_data["simulation_cached_tokens"] = llm_usage.get("cached_tokens", 0)
                        iteration_data["total_cost_usd"] += llm_usage.get("total_cost_usd", 0)
                except Exception as e:
                    print(f"Error loading simulation metadata from {sim_metadata_path}: {e}")
            
            # Load optimization log
            opt_log_path = os.path.join(iteration_path, "optimization_log.json")
            if os.path.exists(opt_log_path):
                try:
                    with open(opt_log_path, 'r', encoding='utf-8') as f:
                        opt_data = json.load(f)
                    
                    if "api_usage" in opt_data and "token_usage" in opt_data["api_usage"]:
                        token_usage = opt_data["api_usage"]["token_usage"]
                        iteration_data["optimization_prompt_tokens"] = token_usage.get("prompt_tokens", 0)
                        iteration_data["optimization_completion_tokens"] = token_usage.get("completion_tokens", 0)
                        
                        # Calculate cost for optimization
                        api_name = MODEL_TO_API.get(model_name)
                        if api_name in TOKEN_COSTS:
                            prompt_cost = (token_usage.get("prompt_tokens", 0) / 1000000) * TOKEN_COSTS[api_name]["prompt_tokens"]
                            completion_cost = (token_usage.get("completion_tokens", 0) / 1000000) * TOKEN_COSTS[api_name]["completion_tokens"]
                            iteration_data["total_cost_usd"] += prompt_cost + completion_cost
                except Exception as e:
                    print(f"Error loading optimization log from {opt_log_path}: {e}")
            
            # Load behavior analysis
            analysis_path = os.path.join(iteration_path, "behavior_analysis.json")
            if os.path.exists(analysis_path):
                try:
                    with open(analysis_path, 'r', encoding='utf-8') as f:
                        analysis_data = json.load(f)
                    
                    # Get KL divergence
                    if "statistics" in analysis_data and "distribution_metrics" in analysis_data["statistics"]:
                        metrics = analysis_data["statistics"]["distribution_metrics"]
                        iteration_data["kl_divergence"] = metrics.get("kl_divergence")
                    
                    # Get token usage
                    if "api_usage" in analysis_data and "token_usage" in analysis_data["api_usage"]:
                        token_usage = analysis_data["api_usage"]["token_usage"]
                        iteration_data["analysis_prompt_tokens"] = token_usage.get("prompt_tokens", 0)
                        iteration_data["analysis_completion_tokens"] = token_usage.get("completion_tokens", 0)
                        
                        # Calculate cost for analysis
                        api_name = MODEL_TO_API.get(model_name)
                        if api_name in TOKEN_COSTS:
                            prompt_cost = (token_usage.get("prompt_tokens", 0) / 1000000) * TOKEN_COSTS[api_name]["prompt_tokens"]
                            completion_cost = (token_usage.get("completion_tokens", 0) / 1000000) * TOKEN_COSTS[api_name]["completion_tokens"]
                            iteration_data["total_cost_usd"] += prompt_cost + completion_cost
                except Exception as e:
                    print(f"Error loading behavior analysis from {analysis_path}: {e}")
            
            # Add to all data
            all_data.append(iteration_data)
    
    return all_data

def calculate_improvement(data):
    """Calculate KL divergence improvement for each run."""
    # Group by run
    runs = {}
    for item in data:
        run_name = item["run_name"]
        if run_name not in runs:
            runs[run_name] = []
        runs[run_name].append(item)
    
    # Calculate improvement for each run
    for run_name, run_data in runs.items():
        # Sort by iteration
        run_data.sort(key=lambda x: x["iteration"])
        
        # Find first and last valid KL divergence
        first_kl = None
        for item in run_data:
            if item["kl_divergence"] is not None:
                first_kl = item["kl_divergence"]
                break
        
        if first_kl is None:
            continue
        
        # Calculate improvement for each iteration
        for item in run_data:
            if item["kl_divergence"] is not None:
                item["kl_improvement"] = first_kl - item["kl_divergence"]
                item["kl_improvement_percent"] = (first_kl - item["kl_divergence"]) / first_kl * 100 if first_kl > 0 else 0
                
                # Calculate cumulative cost up to this iteration
                item["cumulative_cost"] = sum(i["total_cost_usd"] for i in run_data if i["iteration"] <= item["iteration"])
                
                # Calculate efficiency (improvement per dollar)
                if item["cumulative_cost"] > 0:
                    item["efficiency"] = item["kl_improvement"] / item["cumulative_cost"]
                else:
                    item["efficiency"] = 0
    
    # Flatten the data back
    result = []
    for run_data in runs.values():
        result.extend(run_data)
    
    return result

def plot_cost_analysis(data, output_dir):
    """Create a 2x2 subplot for cost analysis."""
    os.makedirs(output_dir, exist_ok=True)
    
    # Convert to DataFrame for easier manipulation
    df = pd.DataFrame(data)
    
    # Create a 2x2 subplot with a more elegant style
    plt.style.use('seaborn-v0_8-whitegrid')
    fig, axes = plt.subplots(2, 2, figsize=(12, 9), dpi=300)
    
    # 1. Top Left: Prompt vs Completion Token Usage (Bar Plot)
    ax1 = axes[0, 0]
    
    # Calculate average token usage by model
    token_usage = df.groupby('model').agg({
        'simulation_prompt_tokens': 'mean',
        'simulation_completion_tokens': 'mean'
    }).reset_index()
    
    # Rename columns for plotting
    token_usage.columns = ['model', 'prompt', 'completion']
    
    # Convert to millions for display
    token_usage['prompt'] = token_usage['prompt'] / 1000000
    token_usage['completion'] = token_usage['completion'] / 1000000
    
    # Create bar positions
    models = token_usage['model']
    x = np.arange(len(models))
    width = 0.35  # Wider bars since we only have two categories now
    
    # Plot bars
    ax1.bar(x - width/2, token_usage['prompt'], width, label='Prompt Tokens', color='#5975a4', alpha=0.8)
    ax1.bar(x + width/2, token_usage['completion'], width, label='Completion Tokens', color='#5f9e6e', alpha=0.8)
    
    # Add labels and title
    ax1.set_ylabel('Average Tokens (Millions) per Iteration', fontsize=12, fontweight='bold')
    ax1.set_title('Prompt vs Completion Token Usage', fontsize=14, fontweight='bold')
    ax1.set_xticks(x)
    ax1.set_xticklabels(models, rotation=45, ha='right', fontsize=10)
    ax1.legend(fontsize=10)
    
    # Format y-axis to show numbers with M suffix
    ax1.get_yaxis().set_major_formatter(plt.FuncFormatter(lambda x, loc: f"{x:.1f}M"))
    
    # 2. Top Right: Average Cost per Iteration by Model (Line Plot)
    ax2 = axes[0, 1]
    
    # Calculate average cost per iteration by model
    cost_by_iteration = df.groupby(['model', 'iteration']).agg({
        'total_cost_usd': 'mean'
    }).reset_index()
    
    # Plot for each model
    for model in df['model'].unique():
        model_data = cost_by_iteration[cost_by_iteration['model'] == model]
        ax2.plot(model_data['iteration'], model_data['total_cost_usd'], 
                 'o-', label=model, color=MODEL_COLORS.get(model, 'gray'), 
                 linewidth=2, markersize=6, markeredgecolor='white', markeredgewidth=1)
    
    # Add labels and title
    ax2.set_xlabel('Iteration', fontsize=12, fontweight='bold')
    ax2.set_ylabel('Average Cost (USD)', fontsize=12, fontweight='bold')
    ax2.set_title('Average Cost per Iteration by Model', fontsize=14, fontweight='bold')
    ax2.legend(fontsize=10)
    ax2.grid(True, linestyle='--', alpha=0.7)
    
    # Format y-axis with dollar sign
    ax2.get_yaxis().set_major_formatter(plt.FuncFormatter(lambda x, loc: "${:.2f}".format(x)))
    
    # Set x-axis to show only integer values
    ax2.xaxis.set_major_locator(MaxNLocator(integer=True))
    
    # 3. Bottom Left: Cost vs Improvement Comparison (Scatter Plot)
    ax3 = axes[1, 0]
    
    # Filter data to include only rows with improvement data
    improvement_data = df.dropna(subset=['kl_improvement'])
    
    # Calculate the final improvement and total cost for each model
    model_final_stats = []
    for model in improvement_data['model'].unique():
        model_data = improvement_data[improvement_data['model'] == model]
        # Get the maximum iteration data for each run
        max_iterations = model_data.groupby('run_name')['iteration'].max()
        final_data = []
        for run_name, max_iter in max_iterations.items():
            run_iter_data = model_data[(model_data['run_name'] == run_name) & 
                                       (model_data['iteration'] == max_iter)]
            if not run_iter_data.empty:
                final_data.append(run_iter_data.iloc[0])
        
        if final_data:
            # Calculate average final improvement and cost across runs
            avg_improvement = np.mean([d['kl_improvement'] for d in final_data])
            avg_cost = np.mean([d['cumulative_cost'] for d in final_data])
            model_final_stats.append({
                'model': model,
                'avg_improvement': avg_improvement,
                'avg_cost': avg_cost
            })
    
    # Plot one point per model
    for stat in model_final_stats:
        ax3.scatter(stat['avg_cost'], stat['avg_improvement'], 
                   label=stat['model'], color=MODEL_COLORS.get(stat['model'], 'gray'), 
                   s=120, alpha=0.8, edgecolors='white', linewidths=1.5)
        
        # Add model name as text label
        ax3.annotate(stat['model'], 
                    (stat['avg_cost'], stat['avg_improvement']),
                    xytext=(7, 0), 
                    textcoords='offset points',
                    fontsize=10,
                    fontweight='bold')
    
    # Add labels and title
    ax3.set_xlabel('Total Cost (USD)', fontsize=12, fontweight='bold')
    ax3.set_ylabel('KL Divergence Improvement', fontsize=12, fontweight='bold')
    ax3.set_title('Cost vs Improvement Comparison', fontsize=14, fontweight='bold')
    ax3.grid(True, linestyle='--', alpha=0.7)
    
    # Format x-axis with dollar sign
    ax3.get_xaxis().set_major_formatter(plt.FuncFormatter(lambda x, loc: "${:.2f}".format(x)))
    
    # Remove legend since we have text labels
    ax3.get_legend().remove() if ax3.get_legend() else None
    
    # 4. Bottom Right: Cost Efficiency (Line Plot)
    ax4 = axes[1, 1]
    
    # Calculate average efficiency per iteration by model
    efficiency_by_iteration = df.dropna(subset=['efficiency']).groupby(['model', 'iteration']).agg({
        'efficiency': 'mean'
    }).reset_index()
    
    # Plot for each model
    for model in df['model'].unique():
        model_data = efficiency_by_iteration[efficiency_by_iteration['model'] == model]
        if not model_data.empty:
            ax4.plot(model_data['iteration'], model_data['efficiency'], 
                     'o-', label=model, color=MODEL_COLORS.get(model, 'gray'), 
                     linewidth=2, markersize=6, markeredgecolor='white', markeredgewidth=1)
    
    # Add labels and title
    ax4.set_xlabel('Iteration', fontsize=12, fontweight='bold')
    ax4.set_ylabel('KL Improvement per Dollar', fontsize=12, fontweight='bold')
    ax4.set_title('Cost Efficiency by Model', fontsize=14, fontweight='bold')
    ax4.legend(fontsize=10)
    ax4.grid(True, linestyle='--', alpha=0.7)
    
    # Set x-axis to show only integer values
    ax4.xaxis.set_major_locator(MaxNLocator(integer=True))
    
    # Adjust layout
    plt.tight_layout()
    
    # Save the figure
    plt.savefig(os.path.join(output_dir, 'cost_analysis.png'), dpi=300, bbox_inches='tight')
    plt.savefig(os.path.join(output_dir, 'cost_analysis.pdf'), dpi=300, bbox_inches='tight')
    plt.close()
    
    # Export data as CSV for further analysis
    df.to_csv(os.path.join(output_dir, 'cost_analysis_data.csv'), index=False)
    
    print(f"Cost analysis plots saved to {output_dir}")

def main():
    """Main function to run the script."""
    # Parse command-line arguments
    parser = argparse.ArgumentParser(description='Generate cost analysis plots across models.')
    parser.add_argument('--output', type=str, default=os.path.join(EVAL_RESULTS_PATH, 'CostAnalysis'),
                        help='Output directory for cost analysis plots')
    args = parser.parse_args()
    
    # Create output directory
    os.makedirs(args.output, exist_ok=True)
    
    print(f"Generating cost analysis plots for {len(MODEL_CONFIGS)} models")
    print(f"Output directory: {args.output}")
    
    # Load data for each model
    all_data = []
    for model_name, runs in MODEL_CONFIGS.items():
        print(f"Processing data for {model_name}...")
        model_data = load_cost_data(model_name, runs)
        all_data.extend(model_data)
    
    if not all_data:
        print("Error: No valid cost data found.")
        return 1
    
    # Calculate improvement metrics
    all_data = calculate_improvement(all_data)
    
    # Create plots
    print("Generating cost analysis plots...")
    plot_cost_analysis(all_data, args.output)
    
    print(f"Analysis complete. Results saved to: {args.output}")
    return 0

if __name__ == "__main__":
    sys.exit(main()) 