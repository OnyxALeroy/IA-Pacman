import os
import json
import matplotlib.pyplot as plt
import numpy as np

# Directory containing the population files
population_dir = "./"  # Adjust this to the directory where your JSON files are located

# List to store fitness values
fitness_values = []
avg_best_fitness = []

# Sort files numerically based on the generation number
files = sorted(
    [f for f in os.listdir(population_dir) if f.startswith("Population_") and f.endswith(".json")],
    key=lambda x: int(x.split("_")[1].split(".")[0])
)

# Iterate through all population files
for filename in files:
    filepath = os.path.join(population_dir, filename)
    
    # Read the JSON file
    with open(filepath, "r") as file:
        data = json.load(file)
    
    # Extract the first non-zero fitness value
    non_zero_fitness = []
    for genome in data.get("genomes", []):
        fitness = genome.get("fitness", 0)
        if fitness != 0:
            non_zero_fitness.append(fitness)
    
    if non_zero_fitness:
        fitness_values.append(max(non_zero_fitness))
        avg_best_fitness.append(sum(non_zero_fitness) / len(non_zero_fitness))
    else:
        fitness_values.append(0)  # Append 0 if no non-zero fitness values are found
        avg_best_fitness.append(0)  # Append 0 for average fitness as well

# Debug log for fitness values
print("Fitness values:", fitness_values)

# Calculate the moving average (window size = 10)
window_size = 10
moving_average = np.convolve(fitness_values, np.ones(window_size)/window_size, mode='valid')

# Calculate the trend line (linear regression)
generations = np.arange(len(fitness_values))
trend = np.polyfit(generations, fitness_values, 1)  # Linear regression (degree 1)
trend_line = np.polyval(trend, generations)  # Evaluate the trend line

# Plot the fitness evolution
plt.figure(figsize=(12, 6))

# Plot the raw fitness values
plt.plot(generations, fitness_values, marker="o", linestyle="-", color="b", label="Fitness")

# Plot the moving average
plt.plot(range(window_size - 1, len(fitness_values)), moving_average, marker="o", linestyle="--", color="r", label=f"Moving Average (window={window_size})")

# Plot the trend line
plt.plot(generations, trend_line, linestyle="--", color="g", label="Trend Line")

# Add titles and labels
plt.title("Best Fitness Evolution, Moving Average, and Trend Line")
plt.xlabel("Generation")
plt.ylabel("Fitness")
plt.legend()
plt.grid(True)

# Save the first plot
plt.savefig("c:/Users/lowel/OneDrive/Documents/GitHub/IA-Pacman/Assets/Populations/fitness_evolution.png", dpi=300, bbox_inches="tight")
plt.show()

# Calculate the moving average for avg_best_fitness (window size = 10)
avg_window_size = 10
avg_moving_average = np.convolve(avg_best_fitness, np.ones(avg_window_size)/avg_window_size, mode='valid')

# Calculate the trend line (linear regression) for avg_best_fitness
avg_generations = np.arange(len(avg_best_fitness))
avg_trend = np.polyfit(avg_generations, avg_best_fitness, 1)  # Linear regression (degree 1)
avg_trend_line = np.polyval(avg_trend, avg_generations)  # Evaluate the trend line

# Plot the average best fitness evolution with trend and moving average
plt.figure(figsize=(12, 6))

# Plot the average best fitness values
plt.plot(avg_generations, avg_best_fitness, marker="o", linestyle="-", color="purple", label="Average Best Fitness")

# Plot the moving average for avg_best_fitness
plt.plot(range(avg_window_size - 1, len(avg_best_fitness)), avg_moving_average, marker="o", linestyle="--", color="orange", label=f"Moving Average (window={avg_window_size})")

# Plot the trend line for avg_best_fitness
plt.plot(avg_generations, avg_trend_line, linestyle="--", color="green", label="Trend Line")

# Add titles and labels
plt.title("Average Best Fitness Evolution with Moving Average and Trend Line")
plt.xlabel("Generation")
plt.ylabel("Average Best Fitness")
plt.legend()
plt.grid(True)

# Save the updated plot
plt.savefig("c:/Users/lowel/OneDrive/Documents/GitHub/IA-Pacman/Assets/Populations/avg_best_fitness_analysis.png", dpi=300, bbox_inches="tight")
plt.show()
