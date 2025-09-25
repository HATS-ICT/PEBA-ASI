#!/usr/bin/env python
"""
Configuration module for PEBA-PEvo framework.

Contains shared constants, default values, and configuration settings used across all modules.
"""

import os

# ======= PATH CONFIGURATIONS =======
# Base path where Unity simulation logs are stored
BASE_SIMULATION_PATH = os.path.join(
    os.path.expanduser("~"), "AppData", "LocalLow", "ICT", "HATS-ASI-LLM"
)

# Base path where optimization runs are stored
BASE_OPTIMIZATION_PATH = os.path.join(BASE_SIMULATION_PATH, "OptimizationRuns")

# Base path where evaluation results will be stored
BASE_EVALUATION_PATH = os.path.join(BASE_SIMULATION_PATH, "EvaluationResults")

# Default simulation logs folder
DEFAULT_SIMULATION_FOLDER = "SimulationLogs"

# ======= BEHAVIOR CONFIGURATIONS =======
# Behavior categories for classification
BEHAVIOR_CATEGORIES = [
    "RUN_FOLLOWING_CROWD",
    "HIDE_IN_PLACE", 
    "HIDE_AFTER_RUNNING",
    "RUN_INDEPENDENTLY",
    "FREEZE",
    "FIGHT",
    "UNKNOWN"
]

# Target behavior distribution for optimization
TARGET_DISTRIBUTION = {
    "RUN_FOLLOWING_CROWD": 0.28,
    "HIDE_IN_PLACE": 0.26,
    "HIDE_AFTER_RUNNING": 0.12,
    "RUN_INDEPENDENTLY": 0.12,
    "FREEZE": 0.12,
    "FIGHT": 0.10,
    "UNKNOWN": 0.00
}

# Ground truth behavior distribution for validation
GROUND_TRUTH_DISTRIBUTION = {
    "RUN_FOLLOWING_CROWD": 0.28,
    "HIDE_IN_PLACE": 0.26,
    "HIDE_AFTER_RUNNING": 0.12,
    "RUN_INDEPENDENTLY": 0.12,
    "FREEZE": 0.12,
    "FIGHT": 0.10
}

# Behavior category descriptions for LLM prompts
BEHAVIOR_DESCRIPTIONS = {
    "RUN_FOLLOWING_CROWD": "Fleeing alongside a group, driven by the instinct to follow others without independently evaluating the safest route.",
    "HIDE_IN_PLACE": "Taking cover wherever they currently are, often due to the belief that movement would increase danger or because they are unable to assess a safer location.",
    "HIDE_AFTER_RUNNING": "Running away from the immediate threat and then transitioning to hiding once they feel a degree of separation from danger.",
    "RUN_INDEPENDENTLY": "Fleeing the scene in a direction they determine independently of the crowd, often based on quick environmental assessment or prior knowledge of the area.",
    "FREEZE": "Becoming mentally and physically immobilized, unable to act due to overwhelming fear or shock.",
    "FIGHT": "Actively attempting to confront, disarm, or incapacitate the shooter, typically as a last resort when no other options are viable."
}

# ======= METRICS CONFIGURATIONS =======
# Metrics to track for optimization analysis
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

# ======= API CONFIGURATIONS =======
# Default OpenAI model configurations
DEFAULT_OPENAI_CONFIG = {
    "model": "gpt-4o-mini",
    "temperature": 0.0,
    "max_tokens": 2000
}

# Classification specific OpenAI configuration
CLASSIFICATION_OPENAI_CONFIG = {
    "model": "gpt-4.1",
    "temperature": 0.0,
    "max_tokens": 1000,
    "response_format": {"type": "json_object"}
}

# Persona optimization specific OpenAI configuration
PERSONA_OPTIMIZATION_CONFIG = {
    "model": "gpt-4.1",
    "temperature": 0.5,
    "max_tokens": 2000,
    "response_format": {"type": "json_object"}
}

# ======= VISUALIZATION CONFIGURATIONS =======
# Color map for behavior categories
BEHAVIOR_COLORS = {
    "RUN_FOLLOWING_CROWD": "#3498db",  # Blue
    "HIDE_IN_PLACE": "#2ecc71",        # Green
    "HIDE_AFTER_RUNNING": "#1abc9c",   # Teal
    "RUN_INDEPENDENTLY": "#9b59b6",    # Purple
    "FREEZE": "#f1c40f",               # Yellow
    "FIGHT": "#e74c3c",                # Red
    "UNKNOWN": "#95a5a6",              # Gray
    "UNCLASSIFIED": "#7f8c8d",         # Dark Gray
    "ERROR": "#34495e"                 # Very Dark Gray
}

# Plotly color map for Sankey diagrams
PLOTLY_BEHAVIOR_COLORS = {
    "RUN_FOLLOWING_CROWD": "rgba(31, 119, 180, 0.8)",  # Blue
    "HIDE_IN_PLACE": "rgba(255, 127, 14, 0.8)",        # Orange
    "HIDE_AFTER_RUNNING": "rgba(44, 160, 44, 0.8)",    # Green
    "RUN_INDEPENDENTLY": "rgba(214, 39, 40, 0.8)",     # Red
    "FREEZE": "rgba(148, 103, 189, 0.8)",              # Purple
    "FIGHT": "rgba(140, 86, 75, 0.8)",                 # Brown
    "UNKNOWN": "rgba(188, 189, 34, 0.8)"               # Olive
}

# Default figure settings
DEFAULT_FIGURE_SIZE = (12, 8)
DEFAULT_DPI = 300

# ======= PROCESSING CONFIGURATIONS =======
# Default multiprocessing settings
DEFAULT_MAX_WORKERS = 32
DEFAULT_BATCH_SIZE = 50

# ======= DEBUG SETTINGS =======
DEBUG = True
VERBOSE = False
