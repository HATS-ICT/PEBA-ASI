#!/usr/bin/env python
"""
Metrics calculation utilities for PEBA-PEvo framework.

This module provides functions to calculate various distribution metrics used
for evaluating the alignment between observed and target behavior distributions.
"""

import numpy as np
from scipy.special import kl_div
from typing import Dict, List, Any
from collections import defaultdict

from ..config import BEHAVIOR_CATEGORIES, TARGET_DISTRIBUTION


def calculate_distribution_metrics(observed_dist: Dict[str, float], 
                                 ground_truth_dist: Dict[str, float]) -> Dict[str, float]:
    """
    Calculate various metrics between observed and ground truth distributions.
    
    Args:
        observed_dist: Dictionary mapping behavior categories to observed proportions
        ground_truth_dist: Dictionary mapping behavior categories to target proportions
        
    Returns:
        Dictionary containing calculated metrics
    """
    # Ensure both distributions have the same categories
    categories = BEHAVIOR_CATEGORIES
    
    # Create normalized probability distributions
    p = np.array([ground_truth_dist.get(cat, 0.0) for cat in categories])
    q = np.array([observed_dist.get(cat, 0.0) for cat in categories])
    
    # Normalize if not already normalized
    if abs(sum(p) - 1.0) > 1e-10:
        p = p / sum(p) if sum(p) > 0 else np.ones(len(p)) / len(p)
    if abs(sum(q) - 1.0) > 1e-10:
        q = q / sum(q) if sum(q) > 0 else np.ones(len(q)) / len(q)
    
    # Add small epsilon to avoid division by zero
    epsilon = 1e-10
    p = p + epsilon
    q = q + epsilon
    
    # Renormalize
    p = p / sum(p)
    q = q / sum(q)
    
    # Calculate KL divergence using scipy
    kl_pq = np.sum(kl_div(p, q))
    kl_qp = np.sum(kl_div(q, p))
    
    # Calculate Jensen-Shannon Divergence
    # JS = 0.5 * (KL(P||M) + KL(Q||M)) where M = 0.5 * (P + Q)
    m = 0.5 * (p + q)
    js_divergence = 0.5 * (np.sum(kl_div(p, m)) + np.sum(kl_div(q, m)))
    
    # Calculate entropy of each distribution
    entropy_p = -np.sum(p * np.log(p))
    entropy_q = -np.sum(q * np.log(q))
    
    # Entropy Gap (ΔH) - difference in entropy
    entropy_gap = entropy_p - entropy_q
    
    # Total Variation Distance - 0.5 * L1 norm
    tvd = 0.5 * np.sum(np.abs(p - q))
    
    return {
        "kl_divergence": kl_pq,
        "reverse_kl": kl_qp,
        "js_divergence": js_divergence,
        "entropy_gap": entropy_gap,
        "tvd": tvd,
        "ground_truth_entropy": entropy_p,
        "observed_entropy": entropy_q
    }


def calculate_statistics(optimization_data: Dict[str, Any]) -> tuple:
    """
    Calculate statistics across multiple optimization runs.
    
    Args:
        optimization_data: Dictionary containing optimization run data
        
    Returns:
        Tuple of (metrics_stats, behavior_stats)
    """
    # Find the maximum number of iterations across all runs
    max_iterations = 0
    for run_name, run_data in optimization_data.items():
        if run_data["iterations"]:
            max_iterations = max(max_iterations, max(run_data["iterations"].keys()))
    
    # Initialize data structures for statistics
    metrics_by_iteration = defaultdict(lambda: defaultdict(list))
    behavior_dist_by_iteration = defaultdict(lambda: defaultdict(list))
    
    # Collect data from all runs
    for run_name, run_data in optimization_data.items():
        for iteration_num, iteration_data in run_data["iterations"].items():
            # Collect metrics
            for metric_name, metric_value in iteration_data["metrics"].items():
                metrics_by_iteration[iteration_num][metric_name].append(metric_value)
            
            # Collect behavior distributions
            for category, value in iteration_data["behavior_distribution"].items():
                behavior_dist_by_iteration[iteration_num][category].append(value)
    
    # Calculate statistics for metrics
    metrics_stats = {}
    for iteration_num in range(1, max_iterations + 1):
        metrics_stats[iteration_num] = {}
        for metric_name in ["kl_divergence", "js_divergence", "entropy_gap", "tvd"]:
            values = metrics_by_iteration[iteration_num][metric_name]
            if values:
                metrics_stats[iteration_num][metric_name] = {
                    "mean": np.mean(values),
                    "std": np.std(values),
                    "min": np.min(values),
                    "max": np.max(values),
                    "values": values
                }
    
    # Calculate statistics for behavior distributions
    behavior_stats = {}
    for iteration_num in range(1, max_iterations + 1):
        behavior_stats[iteration_num] = {}
        for category in BEHAVIOR_CATEGORIES:
            values = behavior_dist_by_iteration[iteration_num][category]
            if values:
                behavior_stats[iteration_num][category] = {
                    "mean": np.mean(values),
                    "std": np.std(values),
                    "min": np.min(values),
                    "max": np.max(values),
                    "values": values
                }
    
    return metrics_stats, behavior_stats


def calculate_topk_distribution(agent_results: Dict[str, Any], k: int = 1) -> Dict[str, float]:
    """
    Calculate the distribution using top-k credit sharing approach.
    
    For each agent, the top k categories in the ranking get 1/k credit each.
    This creates a "softer" distribution that accounts for uncertainty in classification.
    
    Args:
        agent_results: Dictionary of agent classification results
        k: Number of top ranks to consider (default: 1, equivalent to hard voting)
        
    Returns:
        Dictionary mapping categories to their credit-shared proportions
    """
    # Initialize counts for each category
    category_credits = {category: 0.0 for category in BEHAVIOR_CATEGORIES}
    
    # Count valid agents (those with rankings)
    valid_agents = 0
    
    for agent_name, agent_data in agent_results.items():
        behavior_data = agent_data.get("behavior", {})
        
        # Get the ranking if available
        ranking = behavior_data.get("ranking", [])
        
        # Skip agents without ranking data
        if not ranking:
            continue
            
        valid_agents += 1
        
        # Assign credit to top-k categories
        for i in range(min(k, len(ranking))):
            category = ranking[i]
            # Verify the category is valid
            if category in BEHAVIOR_CATEGORIES:
                category_credits[category] += 1.0 / k
    
    # Normalize by number of agents to get proportions
    if valid_agents > 0:
        category_distribution = {cat: credit / valid_agents for cat, credit in category_credits.items()}
    else:
        category_distribution = {cat: 0.0 for cat in BEHAVIOR_CATEGORIES}
    
    return category_distribution


def analyze_distribution_gap(analysis_data: Dict[str, Any], target_distribution: Dict[str, float] = TARGET_DISTRIBUTION) -> tuple:
    """
    Analyze the gap between observed and target distribution.
    
    Args:
        analysis_data: Dictionary containing behavior analysis data
        target_distribution: Target behavior distribution
        
    Returns:
        Tuple of (distribution_gap, observed_dist)
    """
    observed_counts = analysis_data["statistics"]["behavior"]
    total_agents = analysis_data["statistics"]["total_agents"]
    
    # Convert observed counts to distribution
    observed_dist = {}
    for category in BEHAVIOR_CATEGORIES:
        observed_dist[category] = observed_counts.get(category, 0) / total_agents if total_agents > 0 else 0
    
    # Calculate the gap for each category
    distribution_gap = {}
    for category in BEHAVIOR_CATEGORIES:
        target = target_distribution.get(category, 0)
        observed = observed_dist.get(category, 0)
        gap = target - observed
        distribution_gap[category] = gap
    
    return distribution_gap, observed_dist


def calculate_behavior_counts(agent_results: Dict[str, Any]) -> Dict[str, int]:
    """
    Calculate behavior counts from agent classification results.
    
    Args:
        agent_results: Dictionary of agent classification results
        
    Returns:
        Dictionary mapping behaviors to their counts
    """
    behavior_counts = {}
    
    for agent_name, agent_data in agent_results.items():
        behavior = agent_data.get("behavior", {}).get("classification", "UNKNOWN")
        behavior_counts[behavior] = behavior_counts.get(behavior, 0) + 1
    
    return behavior_counts
