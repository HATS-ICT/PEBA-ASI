#!/usr/bin/env python
"""
Optimization Analysis Tool (Refactored)

This script analyzes the results of multiple optimization runs, tracking metrics like KL divergence
over iterations and calculating statistics (mean, standard deviation) across different runs.
It generates visualizations to show the optimization progress and effectiveness.
"""

import os
import sys
import json
import argparse
import shutil

# Add the peba_core package to the path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from peba_core.config import (
    BASE_OPTIMIZATION_PATH,
    BASE_EVALUATION_PATH,
    METRICS,
    METRIC_NAMES
)
from peba_core.utils.data_loader import load_optimization_run_data
from peba_core.utils.metrics import calculate_statistics
from peba_core.utils.visualization import (
    create_metrics_over_iterations_plot,
    create_behavior_distribution_over_iterations_plot,
    create_behavior_radar_chart,
    create_sankey_diagram
)
from peba_core.utils.optimization import calculate_optimization_effectiveness
from peba_core.utils.report_generator import (
    create_summary_report,
    export_data_as_csv
)


class OptimizationAnalyzer:
    """Main class for optimization analysis workflow."""
    
    def __init__(self, optimization_runs: list, output_dir: str = None):
        """Initialize the optimization analyzer."""
        self.optimization_runs = optimization_runs
        self.output_dir = output_dir or self._generate_default_output_dir()
        
    def _generate_default_output_dir(self):
        """Generate default output directory name."""
        if len(self.optimization_runs) > 0:
            folder_name = f"{self.optimization_runs[0]}_{self.optimization_runs[-1]}"
            return os.path.join(BASE_EVALUATION_PATH, folder_name)
        return os.path.join(BASE_EVALUATION_PATH, "optimization_analysis")
    
    def _setup_output_directory(self):
        """Set up the output directory."""
        if os.path.exists(self.output_dir):
            print(f"Notice: Output directory already exists and will be overwritten: {self.output_dir}")
            shutil.rmtree(self.output_dir, ignore_errors=True)
        
        # Create the output directory
        os.makedirs(self.output_dir, exist_ok=True)
        
        # Create EvaluationResults directory if it doesn't exist
        os.makedirs(BASE_EVALUATION_PATH, exist_ok=True)
    
    def load_data(self):
        """Load optimization data from all specified runs."""
        print(f"Loading optimization data from {len(self.optimization_runs)} runs...")
        
        optimization_data = load_optimization_run_data(self.optimization_runs, BASE_OPTIMIZATION_PATH)
        
        if not optimization_data:
            raise ValueError("No valid optimization data found.")
        
        return optimization_data
    
    def analyze_runs(self, optimization_data):
        """Analyze optimization runs and calculate statistics."""
        print("Calculating statistics across optimization runs...")
        
        metrics_stats, behavior_stats = calculate_statistics(optimization_data)
        
        return metrics_stats, behavior_stats
    
    def generate_visualizations(self, optimization_data, metrics_stats, behavior_stats):
        """Generate all visualization plots."""
        print("Generating visualizations...")
        
        plot_paths = []
        
        # Generate metric plots
        print("  - Generating metric plots...")
        metric_plots = create_metrics_over_iterations_plot(metrics_stats, self.output_dir)
        plot_paths.extend(metric_plots)
        
        # Generate behavior distribution plots
        print("  - Generating behavior distribution plots...")
        behavior_plots = create_behavior_distribution_over_iterations_plot(behavior_stats, self.output_dir)
        plot_paths.extend(behavior_plots)
        
        # Generate radar charts
        print("  - Generating radar charts...")
        radar_path = create_behavior_radar_chart(behavior_stats, self.output_dir)
        plot_paths.append(radar_path)
        
        # Generate Sankey diagrams
        print("  - Generating behavior Sankey diagrams...")
        sankey_paths = create_sankey_diagram(optimization_data, self.output_dir)
        plot_paths.extend(sankey_paths)
        
        return plot_paths
    
    def analyze_effectiveness(self, optimization_data):
        """Analyze optimization effectiveness."""
        print("Analyzing optimization effectiveness...")
        
        effectiveness_results = calculate_optimization_effectiveness(optimization_data, BASE_OPTIMIZATION_PATH)
        
        # Save individual effectiveness results
        for run_name, result in effectiveness_results.items():
            output_file = os.path.join(self.output_dir, f'optimization_effectiveness_{run_name}.json')
            with open(output_file, 'w', encoding='utf-8') as f:
                json.dump(result, f, indent=2)
        
        # Create combined effectiveness report if multiple runs
        if len(effectiveness_results) > 1:
            self._create_combined_effectiveness_report(effectiveness_results)
        
        return effectiveness_results
    
    def _create_combined_effectiveness_report(self, effectiveness_results):
        """Create a combined effectiveness report across all runs."""
        from peba_core.config import BEHAVIOR_CATEGORIES
        import matplotlib.pyplot as plt
        
        # Calculate combined metrics
        combined_total = sum(result["summary"]["total_adjustments"] for result in effectiveness_results.values())
        combined_success = sum(result["summary"]["successful_adjustments"] for result in effectiveness_results.values())
        combined_rate = combined_success / combined_total if combined_total > 0 else 0
        
        # Combine success rates by target
        combined_by_target = {category: {"total": 0, "success": 0} for category in BEHAVIOR_CATEGORIES}
        
        for result in effectiveness_results.values():
            for category, rate in result["summary"]["success_rates_by_target"].items():
                if category in combined_by_target:
                    # Extract the actual counts and add them
                    target_stats = result["summary"]["success_rates_by_target"][category]
                    # Approximate total from rate - this is a simplification
                    total_for_category = result["summary"]["total_adjustments"] * 0.1  # rough estimate
                    success_for_category = rate * total_for_category
                    
                    combined_by_target[category]["total"] += total_for_category
                    combined_by_target[category]["success"] += success_for_category
        
        # Calculate combined rates
        combined_rates_by_target = {}
        for category, counts in combined_by_target.items():
            combined_rates_by_target[category] = counts["success"] / counts["total"] if counts["total"] > 0 else 0
        
        # Create combined summary
        combined_summary = {
            "total_adjustments": combined_total,
            "successful_adjustments": combined_success,
            "overall_success_rate": combined_rate,
            "success_rates_by_target": combined_rates_by_target
        }
        
        # Save combined data
        combined_data = {
            "summary": combined_summary,
            "individual_runs": list(effectiveness_results.keys())
        }
        
        with open(os.path.join(self.output_dir, 'optimization_effectiveness_combined.json'), 'w', encoding='utf-8') as f:
            json.dump(combined_data, f, indent=2)
        
        # Create a visualization of combined success rates
        plt.figure(figsize=(10, 6))
        categories = []
        rates = []
        
        for category, rate in combined_rates_by_target.items():
            if combined_by_target[category]["total"] > 0:
                categories.append(category)
                rates.append(rate)
        
        if categories and rates:
            plt.bar(categories, rates, color='skyblue')
            plt.axhline(y=combined_rate, color='red', linestyle='--', 
                       label=f'Overall: {combined_rate:.2f}')
            plt.xlabel('Target Behavior')
            plt.ylabel('Success Rate')
            plt.title('Combined Optimization Success Rate by Target Behavior')
            plt.ylim(0, 1.1)
            plt.legend()
            plt.tight_layout()
            
            # Save the figure
            plt.savefig(os.path.join(self.output_dir, 'success_rate_by_target_combined.png'), dpi=300)
            plt.close()
        
        print("Created combined optimization effectiveness report")
    
    def generate_reports(self, optimization_data, metrics_stats, behavior_stats):
        """Generate summary reports and export data."""
        print("Generating reports...")
        
        # Create summary report
        print("  - Creating summary report...")
        summary_path = create_summary_report(optimization_data, metrics_stats, behavior_stats, self.output_dir)
        
        # Export data as CSV
        print("  - Exporting data as CSV...")
        csv_paths = export_data_as_csv(metrics_stats, behavior_stats, self.output_dir)
        
        # Save raw data for future reference
        raw_data_path = os.path.join(self.output_dir, 'raw_optimization_data.json')
        with open(raw_data_path, 'w', encoding='utf-8') as f:
            json.dump(optimization_data, f, indent=2)
        
        return [summary_path] + csv_paths + [raw_data_path]
    
    def run_complete_analysis(self):
        """Run the complete optimization analysis workflow."""
        print(f"Starting optimization analysis for: {self.optimization_runs}")
        print(f"Output directory: {self.output_dir}")
        
        # Setup output directory
        self._setup_output_directory()
        
        # Load optimization data
        optimization_data = self.load_data()
        
        # Analyze runs
        metrics_stats, behavior_stats = self.analyze_runs(optimization_data)
        
        # Generate visualizations
        plot_paths = self.generate_visualizations(optimization_data, metrics_stats, behavior_stats)
        
        # Analyze effectiveness
        effectiveness_results = self.analyze_effectiveness(optimization_data)
        
        # Generate reports
        report_paths = self.generate_reports(optimization_data, metrics_stats, behavior_stats)
        
        print(f"\nAnalysis complete. Results saved to: {self.output_dir}")
        print(f"Generated {len(plot_paths)} visualization files")
        print(f"Generated {len(report_paths)} report files")
        print(f"Analyzed {len(effectiveness_results)} optimization runs for effectiveness")
        
        return {
            "output_directory": self.output_dir,
            "plot_paths": plot_paths,
            "report_paths": report_paths,
            "effectiveness_results": effectiveness_results
        }


def main():
    """Main function to run the optimization analysis script."""
    # Default optimization runs to analyze
    default_runs = [
        "TransferOfficegemini_2025-05-19_22-59-58",
        "TransferOfficegemini_2025-05-19_23-31-10",
    ]
    
    # Parse command-line arguments
    parser = argparse.ArgumentParser(description='Analyze optimization runs and generate visualizations.')
    parser.add_argument('--runs', nargs='+', default=default_runs,
                        help='List of optimization run folder names to analyze')
    parser.add_argument('--output', type=str, default=None,
                        help='Custom output directory for analysis results')
    args = parser.parse_args()
    
    try:
        # Initialize the analyzer
        analyzer = OptimizationAnalyzer(
            optimization_runs=args.runs,
            output_dir=args.output
        )
        
        # Run the complete analysis
        results = analyzer.run_complete_analysis()
        
        return 0
        
    except Exception as e:
        print(f"Error during analysis: {e}")
        return 1


if __name__ == "__main__":
    sys.exit(main())
