#!/usr/bin/env python
"""
Visualization utilities for PEBA-PEvo framework.

This module provides functions for creating various plots and visualizations
for behavior analysis, optimization tracking, and result presentation.
"""

import os
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
import pandas as pd
import plotly.graph_objects as go
from plotly.subplots import make_subplots
from matplotlib.ticker import MaxNLocator
from typing import Dict, List, Any, Optional
from collections import defaultdict

from ..config import (
    BEHAVIOR_CATEGORIES,
    TARGET_DISTRIBUTION,
    BEHAVIOR_COLORS,
    PLOTLY_BEHAVIOR_COLORS,
    DEFAULT_FIGURE_SIZE,
    DEFAULT_DPI,
    METRIC_NAMES
)


def setup_matplotlib_style():
    """Set up consistent matplotlib styling."""
    plt.style.use('default')
    plt.rcParams['figure.facecolor'] = 'white'
    plt.rcParams['axes.facecolor'] = 'white'


def create_behavior_comparison_plot(observed_counts: Dict[str, int], output_dir: str, 
                                  ground_truth: Dict[str, float] = TARGET_DISTRIBUTION) -> str:
    """
    Create and save visualizations comparing observed vs ground truth distributions.
    
    Args:
        observed_counts: Dictionary mapping behaviors to observed counts
        output_dir: Directory to save the plots
        ground_truth: Ground truth distribution to compare against
        
    Returns:
        Path to the saved comparison plot
    """
    os.makedirs(output_dir, exist_ok=True)
    setup_matplotlib_style()
    
    # Convert observed counts to distribution
    total_observed = sum(observed_counts.values())
    observed_dist = {k: v/total_observed for k, v in observed_counts.items()}
    
    # Fill in missing categories with zeros
    for category in BEHAVIOR_CATEGORIES:
        if category not in observed_dist:
            observed_dist[category] = 0.0
    
    # Create figure with subplots
    fig, ((ax1, ax2), (ax3, ax4)) = plt.subplots(2, 2, figsize=(15, 12))
    
    # Prepare data for bar charts
    categories = [cat for cat in BEHAVIOR_CATEGORIES if cat != "UNKNOWN"]
    observed_values = [observed_dist.get(cat, 0.0) for cat in categories]
    ground_truth_values = [ground_truth.get(cat, 0.0) for cat in categories]
    bar_colors = [BEHAVIOR_COLORS.get(cat, "#95a5a6") for cat in categories]
    
    # Create the observed bar chart
    bars1 = ax1.bar(categories, observed_values, color=bar_colors)
    ax1.set_xlabel('Behavior Category')
    ax1.set_ylabel('Proportion of Agents')
    ax1.set_title('Observed Behavior Distribution')
    plt.setp(ax1.get_xticklabels(), rotation=45, ha='right')
    
    # Add value labels on top of bars
    for i, v in enumerate(observed_values):
        ax1.text(i, v + 0.01, f"{v:.2f}", ha='center')
    
    # Create the ground truth bar chart
    bars2 = ax2.bar(categories, ground_truth_values, color=bar_colors)
    ax2.set_xlabel('Behavior Category')
    ax2.set_ylabel('Proportion of Agents')
    ax2.set_title('Ground Truth Behavior Distribution')
    plt.setp(ax2.get_xticklabels(), rotation=45, ha='right')
    
    # Add value labels on top of bars
    for i, v in enumerate(ground_truth_values):
        ax2.text(i, v + 0.01, f"{v:.2f}", ha='center')
    
    # Set the same y-axis limits for both bar charts
    max_y = max(max(observed_values), max(ground_truth_values)) + 0.05
    ax1.set_ylim(0, max_y)
    ax2.set_ylim(0, max_y)
    
    # Create the observed pie chart
    ax3.pie(observed_values, labels=categories, autopct='%1.1f%%', startangle=90, colors=bar_colors)
    ax3.set_title('Observed Behavior Distribution')
    ax3.axis('equal')
    
    # Create the ground truth pie chart
    ax4.pie(ground_truth_values, labels=categories, autopct='%1.1f%%', startangle=90, colors=bar_colors)
    ax4.set_title('Ground Truth Behavior Distribution')
    ax4.axis('equal')
    
    plt.tight_layout()
    
    # Save the plot
    plot_path = os.path.join(output_dir, 'behavior_comparison.png')
    plt.savefig(plot_path, dpi=DEFAULT_DPI, bbox_inches='tight')
    plt.close(fig)
    
    return plot_path


def create_metrics_over_iterations_plot(metrics_stats: Dict[int, Dict[str, Any]], output_dir: str) -> List[str]:
    """
    Plot metrics over iterations with confidence intervals.
    
    Args:
        metrics_stats: Dictionary containing metrics statistics by iteration
        output_dir: Directory to save the plots
        
    Returns:
        List of paths to saved plots
    """
    os.makedirs(output_dir, exist_ok=True)
    setup_matplotlib_style()
    
    saved_plots = []
    
    # Create a figure for each metric
    for metric_name in ["kl_divergence", "js_divergence", "entropy_gap", "tvd"]:
        plt.figure(figsize=DEFAULT_FIGURE_SIZE)
        
        # Extract data for this metric
        iterations = sorted(metrics_stats.keys())
        means = [metrics_stats[i][metric_name]["mean"] if metric_name in metrics_stats[i] else np.nan for i in iterations]
        stds = [metrics_stats[i][metric_name]["std"] if metric_name in metrics_stats[i] else np.nan for i in iterations]
        
        # Remove NaN values
        valid_indices = ~np.isnan(means)
        valid_iterations = np.array(iterations)[valid_indices]
        valid_means = np.array(means)[valid_indices]
        valid_stds = np.array(stds)[valid_indices]
        
        if len(valid_iterations) > 0:
            # Plot the mean line
            plt.plot(valid_iterations, valid_means, 'o-', 
                    label=f'Mean {METRIC_NAMES.get(metric_name, metric_name)}')
            
            # Calculate standard error of the mean (SEM = std / sqrt(n))
            n_runs = [len(metrics_stats[i][metric_name]["values"]) if metric_name in metrics_stats[i] else 0 
                     for i in iterations]
            valid_n_runs = np.array(n_runs)[valid_indices]
            
            std_errors = valid_stds / np.sqrt(valid_n_runs)
            
            # Fill between standard error bounds
            plt.fill_between(valid_iterations, valid_means - std_errors, valid_means + std_errors, alpha=0.2,
                           label='Standard Error of Mean')
            
            # Add a trend line
            if len(valid_iterations) > 1:
                z = np.polyfit(valid_iterations, valid_means, 1)
                p = np.poly1d(z)
                plt.plot(valid_iterations, p(valid_iterations), "r--", 
                        label=f'Trend: {z[0]:.4f}x + {z[1]:.4f}')
            
            # Set axis labels and title
            plt.xlabel('Iteration')
            plt.ylabel(METRIC_NAMES.get(metric_name, metric_name))
            plt.title(f'{METRIC_NAMES.get(metric_name, metric_name)} Over Iterations')
            
            # Set x-axis to show only integer values
            plt.gca().xaxis.set_major_locator(MaxNLocator(integer=True))
            
            # Add grid and legend
            plt.grid(True, linestyle='--', alpha=0.7)
            plt.legend(loc='best', fontsize=8)
            
            # Save the figure
            plt.tight_layout()
            plot_path = os.path.join(output_dir, f'{metric_name}_over_iterations.png')
            plt.savefig(plot_path, dpi=DEFAULT_DPI)
            plt.close()
            saved_plots.append(plot_path)
    
    return saved_plots


def create_behavior_distribution_over_iterations_plot(behavior_stats: Dict[int, Dict[str, Any]], output_dir: str) -> List[str]:
    """
    Plot behavior distribution over iterations with confidence intervals.
    
    Args:
        behavior_stats: Dictionary containing behavior statistics by iteration
        output_dir: Directory to save the plots
        
    Returns:
        List of paths to saved plots
    """
    os.makedirs(output_dir, exist_ok=True)
    setup_matplotlib_style()
    
    # Create a dataframe for easier plotting
    data = []
    for iteration_num, categories in behavior_stats.items():
        for category, stats in categories.items():
            data.append({
                'Iteration': iteration_num,
                'Category': category,
                'Mean': stats['mean'],
                'Std': stats['std'],
                'Target': TARGET_DISTRIBUTION.get(category, 0),
                'Count': len(stats['values']) if 'values' in stats else 0
            })
    
    df = pd.DataFrame(data)
    
    # Plot behavior distribution over iterations
    plt.figure(figsize=(12, 8))
    
    # Create a line plot for each category
    for category in BEHAVIOR_CATEGORIES:
        if category == "UNKNOWN":
            continue
            
        category_data = df[df['Category'] == category]
        if not category_data.empty:
            iterations = category_data['Iteration'].values
            means = category_data['Mean'].values
            stds = category_data['Std'].values
            counts = category_data['Count'].values
            
            # Calculate standard error of the mean (SEM = std / sqrt(n))
            std_errors = stds / np.sqrt(counts)
            
            # Plot mean line
            color = BEHAVIOR_COLORS[category]
            plt.plot(iterations, means, 'o-', color=color, label=category)
            
            # Fill between standard error bounds
            plt.fill_between(iterations, means - std_errors, means + std_errors, color=color, alpha=0.2)
            
            # Add a horizontal line for the target value
            target = TARGET_DISTRIBUTION.get(category, 0)
            plt.axhline(y=target, color=color, linestyle='--', alpha=0.5)
    
    # Set axis labels and title
    plt.xlabel('Iteration')
    plt.ylabel('Proportion')
    plt.title('Behavior Distribution Over Iterations')
    
    # Set x-axis to show only integer values
    plt.gca().xaxis.set_major_locator(MaxNLocator(integer=True))
    
    # Add grid and legend
    plt.grid(True, linestyle='--', alpha=0.7)
    plt.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    
    # Save the figure
    plt.tight_layout()
    plot_path = os.path.join(output_dir, 'behavior_distribution_over_iterations.png')
    plt.savefig(plot_path, dpi=DEFAULT_DPI)
    plt.close()
    
    # Also create a heatmap showing the difference from target
    plt.figure(figsize=(12, 8))
    
    # Calculate difference from target
    df['Difference'] = df['Mean'] - df['Target']
    
    # Pivot the dataframe for the heatmap
    pivot_df = df.pivot(index='Category', columns='Iteration', values='Difference')
    
    # Create the heatmap
    sns.heatmap(pivot_df, cmap='RdBu_r', center=0, annot=True, fmt='.2f', 
                cbar_kws={'label': 'Difference from Target'})
    
    # Set title
    plt.title('Difference from Target Distribution Over Iterations')
    
    # Save the figure
    plt.tight_layout()
    heatmap_path = os.path.join(output_dir, 'behavior_difference_heatmap.png')
    plt.savefig(heatmap_path, dpi=DEFAULT_DPI)
    plt.close()
    
    return [plot_path, heatmap_path]


def create_behavior_radar_chart(behavior_stats: Dict[int, Dict[str, Any]], output_dir: str) -> str:
    """
    Create radar charts to visualize behavior distribution by category.
    
    Args:
        behavior_stats: Dictionary containing behavior statistics by iteration
        output_dir: Directory to save the plots
        
    Returns:
        Path to the saved radar chart
    """
    os.makedirs(output_dir, exist_ok=True)
    setup_matplotlib_style()
    
    # Get the last iteration for final distribution
    last_iteration = max(behavior_stats.keys())
    
    # Set up the radar chart
    categories = [cat for cat in BEHAVIOR_CATEGORIES if cat != "UNKNOWN"]
    N = len(categories)
    
    # Create angles for each category
    angles = np.linspace(0, 2*np.pi, N, endpoint=False).tolist()
    # Make the plot circular by appending the first angle again
    angles += angles[:1]
    
    # Set up the figure
    fig, ax = plt.subplots(figsize=(10, 10), subplot_kw=dict(projection='polar'))
    
    # Get the mean values for the last iteration
    means = []
    for category in categories:
        if category in behavior_stats[last_iteration]:
            means.append(behavior_stats[last_iteration][category]["mean"])
        else:
            means.append(0)
    
    # Make values circular for plotting
    means = means + means[:1]
    
    # Get target values
    targets = [TARGET_DISTRIBUTION.get(category, 0) for category in categories]
    # Make targets circular for plotting
    targets = targets + targets[:1]
    
    # Plot the actual distribution
    ax.plot(angles, means, 'o-', linewidth=2, label='Actual Distribution')
    ax.fill(angles, means, alpha=0.25)
    
    # Plot the target distribution
    ax.plot(angles, targets, 'o-', linewidth=2, label='Target Distribution')
    ax.fill(angles, targets, alpha=0.1)
    
    # Set category labels
    ax.set_xticks(angles[:-1])
    ax.set_xticklabels(categories)
    
    # Add legend and title
    ax.legend(loc='upper right', bbox_to_anchor=(0.1, 0.1))
    plt.title('Behavior Distribution Radar Chart (Final Iteration)', size=15)
    
    # Save the figure
    plt.tight_layout()
    plot_path = os.path.join(output_dir, 'behavior_radar_chart.png')
    plt.savefig(plot_path, dpi=DEFAULT_DPI)
    plt.close()
    
    return plot_path


def create_sankey_diagram(optimization_data: Dict[str, Any], output_dir: str) -> List[str]:
    """
    Create Sankey diagrams to visualize behavior transitions across iterations.
    
    Args:
        optimization_data: Dictionary containing optimization run data
        output_dir: Directory to save the diagrams
        
    Returns:
        List of paths to saved Sankey diagrams
    """
    os.makedirs(output_dir, exist_ok=True)
    
    saved_files = []
    
    # Process each run separately
    for run_name, run_data in optimization_data.items():
        iterations = sorted(run_data["iterations"].keys())
        if len(iterations) < 2:
            print(f"Warning: Run {run_name} has fewer than 2 iterations, skipping Sankey diagram")
            continue
        
        # Initialize data structures for the Sankey diagram
        nodes = []
        node_colors = []
        links_source = []
        links_target = []
        links_value = []
        links_color = []
        
        # Create nodes for each behavior category in each iteration
        node_indices = {}
        node_idx = 0
        
        for iteration in iterations:
            for category in BEHAVIOR_CATEGORIES:
                if category == "UNKNOWN":
                    continue
                node_name = f"{category} (Iter {iteration})"
                nodes.append(node_name)
                node_colors.append(PLOTLY_BEHAVIOR_COLORS[category])
                node_indices[(iteration, category)] = node_idx
                node_idx += 1
        
        # Track behavior transitions between consecutive iterations
        for i in range(len(iterations) - 1):
            current_iter = iterations[i]
            next_iter = iterations[i + 1]
            
            # Get agent behaviors for both iterations
            current_behaviors = run_data["iterations"][current_iter].get("agent_behaviors", {})
            next_behaviors = run_data["iterations"][next_iter].get("agent_behaviors", {})
            
            # Count transitions
            transitions = defaultdict(int)
            
            # Find agents present in both iterations
            common_agents = set(current_behaviors.keys()) & set(next_behaviors.keys())
            
            for agent in common_agents:
                from_behavior = current_behaviors[agent]
                to_behavior = next_behaviors[agent]
                if from_behavior != "UNKNOWN" and to_behavior != "UNKNOWN":
                    transitions[(from_behavior, to_behavior)] += 1
            
            # Add links for each transition
            for (from_behavior, to_behavior), count in transitions.items():
                if count > 0:
                    source_idx = node_indices.get((current_iter, from_behavior))
                    target_idx = node_indices.get((next_iter, to_behavior))
                    
                    if source_idx is not None and target_idx is not None:
                        links_source.append(source_idx)
                        links_target.append(target_idx)
                        links_value.append(count)
                        links_color.append(PLOTLY_BEHAVIOR_COLORS[from_behavior])
        
        # Create the Sankey diagram
        fig = go.Figure(data=[go.Sankey(
            node=dict(
                pad=15,
                thickness=20,
                line=dict(color="black", width=0.5),
                label=nodes,
                color=node_colors
            ),
            link=dict(
                source=links_source,
                target=links_target,
                value=links_value,
                color=links_color
            )
        )])
        
        # Update layout
        fig.update_layout(
            title_text=f"Behavior Transitions Across Iterations - {run_name}",
            font_size=10,
            width=1200,
            height=800
        )
        
        # Save as HTML for interactive viewing
        html_path = os.path.join(output_dir, f'behavior_sankey_{run_name}.html')
        fig.write_html(html_path)
        saved_files.append(html_path)
    
    return saved_files


def create_topk_distributions_plot(agent_results: Dict[str, Any], output_dir: str, 
                                 k_values: List[int] = [1, 2, 3, 4, 5, 6]) -> str:
    """
    Create visualizations for different top-k credit sharing distributions.
    
    Args:
        agent_results: Dictionary of agent classification results
        output_dir: Directory to save the plots
        k_values: List of k values to analyze
        
    Returns:
        Path to the saved plot
    """
    from .metrics import calculate_topk_distribution, calculate_distribution_metrics
    
    os.makedirs(output_dir, exist_ok=True)
    setup_matplotlib_style()
    
    # Calculate distributions for different k values
    distributions = {}
    metrics = {}
    
    for k in k_values:
        distributions[k] = calculate_topk_distribution(agent_results, k=k)
        metrics[k] = calculate_distribution_metrics(distributions[k], TARGET_DISTRIBUTION)
    
    # Create a figure with subplots for each k value
    fig, axes = plt.subplots(len(k_values), 2, figsize=(15, 5 * len(k_values)))
    
    # Prepare data for ground truth
    categories = [cat for cat in BEHAVIOR_CATEGORIES if cat != "UNKNOWN"]
    ground_truth_values = [TARGET_DISTRIBUTION.get(cat, 0.0) for cat in categories]
    bar_colors = [BEHAVIOR_COLORS.get(cat, "#95a5a6") for cat in categories]
    
    # Create plots for each k value
    for i, k in enumerate(k_values):
        # Get the distribution for this k
        dist = distributions[k]
        observed_values = [dist.get(cat, 0.0) for cat in categories]
        
        # Bar chart
        ax_bar = axes[i][0]
        ax_bar.bar(categories, observed_values, color=bar_colors)
        ax_bar.set_xlabel('Behavior Category')
        ax_bar.set_ylabel('Credit-Shared Proportion')
        ax_bar.set_title(f'Top-{k} Credit Sharing Distribution')
        plt.setp(ax_bar.get_xticklabels(), rotation=45, ha='right')
        
        # Add value labels on top of bars
        for j, v in enumerate(observed_values):
            ax_bar.text(j, v + 0.01, f"{v:.2f}", ha='center')
        
        # Add ground truth as a line
        ax_bar.plot(range(len(categories)), ground_truth_values, 'k--', label='Ground Truth')
        ax_bar.legend()
        
        # Set y-axis limit
        ax_bar.set_ylim(0, max(max(observed_values), max(ground_truth_values)) + 0.05)
        
        # Comparison chart (observed vs ground truth)
        ax_comp = axes[i][1]
        width = 0.35
        x = np.arange(len(categories))
        
        # Plot observed and ground truth side by side
        ax_comp.bar(x - width/2, observed_values, width, label='Observed', color=bar_colors)
        ax_comp.bar(x + width/2, ground_truth_values, width, label='Ground Truth', 
                   color=[c + '80' for c in bar_colors])
        
        ax_comp.set_xlabel('Behavior Category')
        ax_comp.set_ylabel('Proportion')
        ax_comp.set_title(f'Top-{k} vs Ground Truth')
        ax_comp.set_xticks(x)
        ax_comp.set_xticklabels(categories)
        plt.setp(ax_comp.get_xticklabels(), rotation=45, ha='right')
        ax_comp.legend()
        
        # Set y-axis limit
        ax_comp.set_ylim(0, max(max(observed_values), max(ground_truth_values)) + 0.05)
        
        # Add metrics as text
        metric_text = (
            f"KL Divergence: {metrics[k]['kl_divergence']:.4f}\n"
            f"JS Divergence: {metrics[k]['js_divergence']:.4f}\n"
            f"TVD: {metrics[k]['tvd']:.4f}"
        )
        ax_comp.text(0.5, 0.95, metric_text, transform=ax_comp.transAxes, 
                    ha='center', va='top', bbox=dict(boxstyle='round', facecolor='wheat', alpha=0.5))
    
    plt.tight_layout()
    
    # Save the plot
    plot_path = os.path.join(output_dir, 'topk_distributions.png')
    plt.savefig(plot_path, dpi=DEFAULT_DPI, bbox_inches='tight')
    plt.close(fig)
    
    return plot_path
