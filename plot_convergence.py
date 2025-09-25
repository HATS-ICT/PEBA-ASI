#!/usr/bin/env python
"""
Convergence Plot Generator

This script creates a 2x2 subplot comparing the convergence of different metrics
(KL divergence, JS divergence, entropy gap, and TVD) across optimization runs
for different models. Each plot includes error bands showing standard error of the mean.
"""

import os
import sys
import json
import argparse
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.ticker import MaxNLocator
from collections import defaultdict
import pandas as pd

# ======= SETTINGS =======
# Base path where optimization runs are stored
BASE_PATH = os.path.join(os.path.expanduser("~"), "AppData", "LocalLow", "...", "OptimizationRuns")

# Base path where evaluation results will be stored
EVAL_RESULTS_PATH = os.path.join(os.path.expanduser("~"), "AppData", "LocalLow", "...", "EvaluationResults")

# Model configurations
MODEL_CONFIGS = {
    "GPT-4.1 Mini": [
        "ComplexPersonaLong2_2025-05-13_22-57-55",
        "ComplexPersonaLong2_2025-05-13_23-32-10",
        "ComplexPersonaLong2_2025-05-14_00-04-42",
        "ComplexPersonaLong2_2025-05-14_01-06-35",
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

# Metrics to plot
METRICS = [
    "kl_divergence",
    "js_divergence",
    "entropy_gap",
    "tvd"
]

# Metric display names
METRIC_NAMES = {
    "kl_divergence": "KL Divergence",
    "js_divergence": "Jensen-Shannon Divergence",
    "entropy_gap": "Entropy Gap (ΔH)",
    "tvd": "Total Variation Distance"
}

# Colors for each model
MODEL_COLORS = {
    "GPT-4.1 Mini": "#1f77b4",    # blue
    "GPT-4o Mini": "#ff7f0e",     # orange
    "Gemini 2.5 Flash": "#2ca02c", # green
    "DeepSeek V3": "#d62728"      # red
}

def load_optimization_data(model_name, optimization_runs):
    """Load data from multiple optimization runs for a specific model."""
    all_data = {}
    
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
        
        run_data = {"iterations": {}}
        
        # Load data from each iteration
        for iteration_folder in iteration_folders:
            iteration_path = os.path.join(run_path, iteration_folder)
            analysis_path = os.path.join(iteration_path, "behavior_analysis.json")
            
            if not os.path.exists(analysis_path):
                print(f"Warning: Behavior analysis file not found: {analysis_path}")
                continue
            
            try:
                with open(analysis_path, 'r', encoding='utf-8') as f:
                    analysis_data = json.load(f)
                
                # Extract iteration number
                iteration_num = int(iteration_folder.split("_")[1])
                
                # Extract metrics
                metrics = analysis_data.get("statistics", {}).get("distribution_metrics", {})
                
                # Store data for this iteration
                run_data["iterations"][iteration_num] = {
                    "metrics": metrics
                }
                
            except Exception as e:
                print(f"Error loading data from {analysis_path}: {e}")
        
        # Store data for this run
        all_data[run_name] = run_data
    
    return all_data

def calculate_statistics(model_name, optimization_data):
    """Calculate statistics across multiple optimization runs for a specific model."""
    # Find the maximum number of iterations across all runs
    max_iterations = 0
    for run_name, run_data in optimization_data.items():
        if run_data["iterations"]:
            max_iterations = max(max_iterations, max(run_data["iterations"].keys()))
    
    # Initialize data structures for statistics
    metrics_by_iteration = defaultdict(lambda: defaultdict(list))
    
    # Collect data from all runs
    for run_name, run_data in optimization_data.items():
        for iteration_num, iteration_data in run_data["iterations"].items():
            # Collect metrics
            for metric_name, metric_value in iteration_data["metrics"].items():
                metrics_by_iteration[iteration_num][metric_name].append(metric_value)
    
    # Calculate statistics for metrics
    metrics_stats = {}
    for iteration_num in range(1, max_iterations + 1):
        metrics_stats[iteration_num] = {}
        for metric_name in METRICS:
            values = metrics_by_iteration[iteration_num][metric_name]
            if values:
                metrics_stats[iteration_num][metric_name] = {
                    "mean": np.mean(values),
                    "std": np.std(values),
                    "sem": np.std(values) / np.sqrt(len(values)),  # Standard error of the mean
                    "n": len(values),
                    "values": values
                }
    
    return metrics_stats

def plot_convergence_comparison(model_stats, output_dir):
    """Create a 2x2 subplot comparing convergence of metrics across models."""
    os.makedirs(output_dir, exist_ok=True)
    
    # Create a 2x2 subplot with a more elegant style
    plt.style.use('seaborn-v0_8-whitegrid')
    fig, axes = plt.subplots(2, 2, figsize=(12, 10), dpi=300)
    axes = axes.flatten()
    
    # Define subplot positions for each metric
    metric_positions = {
        "kl_divergence": 0,  # top left
        "js_divergence": 1,  # top right
        "entropy_gap": 2,    # bottom left
        "tvd": 3             # bottom right
    }
    
    # Plot each metric
    for metric_name, pos in metric_positions.items():
        ax = axes[pos]
        
        # Plot data for each model
        for model_name, stats in model_stats.items():
            # Extract data for this metric
            iterations = sorted(stats.keys())
            means = [stats[i][metric_name]["mean"] if metric_name in stats[i] else np.nan for i in iterations]
            sems = [stats[i][metric_name]["sem"] if metric_name in stats[i] else np.nan for i in iterations]
            
            # Remove NaN values
            valid_indices = ~np.isnan(means)
            valid_iterations = np.array(iterations)[valid_indices]
            valid_means = np.array(means)[valid_indices]
            valid_sems = np.array(sems)[valid_indices]
            
            if len(valid_iterations) > 0:
                # Plot the mean line with improved styling
                color = MODEL_COLORS.get(model_name, "gray")
                line, = ax.plot(valid_iterations, valid_means, 'o-', 
                        label=model_name, color=color, linewidth=2.5, 
                        markersize=6, markeredgecolor='white', markeredgewidth=1)
                
                # Fill between standard error bounds with more subtle transparency
                ax.fill_between(valid_iterations, valid_means - valid_sems, valid_means + valid_sems, 
                               alpha=0.15, color=color)
        
        # Set axis labels and title with improved typography
        ax.set_xlabel('Iteration', fontsize=13, fontweight='bold')
        ax.set_ylabel(METRIC_NAMES.get(metric_name, metric_name), fontsize=13, fontweight='bold')
        # ax.set_title(METRIC_NAMES.get(metric_name, metric_name), fontsize=15, fontweight='bold')
        
        # Set x-axis to show only integer values
        ax.xaxis.set_major_locator(MaxNLocator(integer=True))
        
        # Improve grid appearance
        ax.grid(True, linestyle='--', alpha=0.5, linewidth=0.5)
        
        # Improve tick label appearance
        ax.tick_params(axis='both', which='major', labelsize=11)
        
        # For KL divergence, also create a log-scale version with improved styling
        if metric_name == "kl_divergence":
            # Create a separate figure for log-scale KL divergence
            plt.figure(figsize=(10, 6), dpi=300)
            plt.style.use('seaborn-v0_8-whitegrid')
            
            for model_name, stats in model_stats.items():
                # Extract data for this metric
                iterations = sorted(stats.keys())
                means = [stats[i][metric_name]["mean"] if metric_name in stats[i] else np.nan for i in iterations]
                sems = [stats[i][metric_name]["sem"] if metric_name in stats[i] else np.nan for i in iterations]
                
                # Remove NaN values
                valid_indices = ~np.isnan(means)
                valid_iterations = np.array(iterations)[valid_indices]
                valid_means = np.array(means)[valid_indices]
                valid_sems = np.array(sems)[valid_indices]
                
                if len(valid_iterations) > 0:
                    # Plot the mean line with log scale and improved styling
                    color = MODEL_COLORS.get(model_name, "gray")
                    plt.semilogy(valid_iterations, valid_means, 'o-', 
                            label=model_name, color=color, linewidth=2.5,
                            markersize=6, markeredgecolor='white', markeredgewidth=1)
                    
                    # Ensure lower bound is positive for log scale
                    lower_bound = np.maximum(valid_means - valid_sems, 1e-10)
                    upper_bound = valid_means + valid_sems
                    
                    # Fill between standard error bounds with more subtle transparency
                    plt.fill_between(valid_iterations, lower_bound, upper_bound, alpha=0.15, color=color)
            
            # Set axis labels with improved typography
            plt.xlabel('Iteration', fontsize=13, fontweight='bold')
            plt.ylabel(METRIC_NAMES.get(metric_name, metric_name), fontsize=13, fontweight='bold')
            plt.title(f'{METRIC_NAMES.get(metric_name, metric_name)} (Log Scale)', fontsize=15, fontweight='bold')
            
            # Set x-axis to show only integer values
            plt.gca().xaxis.set_major_locator(MaxNLocator(integer=True))
            
            # Improve grid appearance
            plt.grid(True, linestyle='--', alpha=0.5, linewidth=0.5)
            
            # Improve tick label appearance
            plt.tick_params(axis='both', which='major', labelsize=11)
            
            # Add legend with improved styling
            plt.legend(loc='best', fontsize=11, framealpha=0.9, edgecolor='lightgray')
            
            # Save the log-scale figure
            plt.tight_layout()
            plt.savefig(os.path.join(output_dir, f'{metric_name}_log_scale_comparison.png'), dpi=300, bbox_inches='tight')
            plt.savefig(os.path.join(output_dir, f'{metric_name}_log_scale_comparison.pdf'), dpi=300, bbox_inches='tight')
            plt.close()
    
    # Add a common legend at the bottom of the figure with improved styling
    handles, labels = axes[0].get_legend_handles_labels()
    fig.legend(handles, labels, loc='lower center', ncol=len(model_stats), 
              fontsize=12, framealpha=0.9, edgecolor='lightgray',
              bbox_to_anchor=(0.5, 0.02))
    
    # Adjust layout to make room for the legend
    plt.tight_layout()
    plt.subplots_adjust(bottom=0.1)
    
    # Remove the overall title
    
    # Save the figure with improved quality
    plt.savefig(os.path.join(output_dir, 'metrics_convergence_comparison.png'), dpi=300, bbox_inches='tight')
    plt.savefig(os.path.join(output_dir, 'metrics_convergence_comparison.pdf'), dpi=300, bbox_inches='tight')
    plt.close()

def main():
    """Main function to run the script."""
    # Parse command-line arguments
    parser = argparse.ArgumentParser(description='Generate convergence comparison plots across models.')
    parser.add_argument('--output', type=str, default=os.path.join(EVAL_RESULTS_PATH, 'ModelComparison'),
                        help='Output directory for comparison plots')
    args = parser.parse_args()
    
    # Create output directory
    os.makedirs(args.output, exist_ok=True)
    
    print(f"Generating convergence comparison plots for {len(MODEL_CONFIGS)} models")
    print(f"Output directory: {args.output}")
    
    # Load data and calculate statistics for each model
    model_stats = {}
    for model_name, runs in MODEL_CONFIGS.items():
        print(f"Processing data for {model_name}...")
        optimization_data = load_optimization_data(model_name, runs)
        
        if not optimization_data:
            print(f"Warning: No valid optimization data found for {model_name}.")
            continue
        
        # Calculate statistics
        model_stats[model_name] = calculate_statistics(model_name, optimization_data)
    
    if not model_stats:
        print("Error: No valid model data found.")
        return 1
    
    # Create comparison plots
    print("Generating convergence comparison plots...")
    plot_convergence_comparison(model_stats, args.output)
    
    # Export data as CSV for further analysis
    print("Exporting data as CSV...")
    export_data = []
    
    for model_name, stats in model_stats.items():
        for iteration, metrics in stats.items():
            for metric_name in METRICS:
                if metric_name in metrics:
                    export_data.append({
                        'Model': model_name,
                        'Iteration': iteration,
                        'Metric': metric_name,
                        'Mean': metrics[metric_name]['mean'],
                        'SEM': metrics[metric_name]['sem'],
                        'N': metrics[metric_name]['n']
                    })
    
    export_df = pd.DataFrame(export_data)
    export_df.to_csv(os.path.join(args.output, 'model_comparison_data.csv'), index=False)
    
    print(f"Analysis complete. Results saved to: {args.output}")
    return 0

if __name__ == "__main__":
    sys.exit(main()) 