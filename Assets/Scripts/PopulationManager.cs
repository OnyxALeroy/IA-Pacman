using System.IO; // For file operations
using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    [SerializeField] private PacmanBrain[] pacmans;
    [SerializeField] private GameManager[] gameManagers;
    public int populationSize;
    public float mutationRate = 0.05f;

    private float BestFitness;
    public int gen = 0;

    [SerializeField] public int nbElite;
    [SerializeField] public int nbDiversity;

    private List<Genome> population = new();
    private bool isGenerationComplete = false;

    private void Start()
    {
        BestFitness = 0;
        populationSize = pacmans.Length;
        LoadPopulation();
        if (population.Count == 0)
        {
            SetPopulation();
        }        
        Debug.Log("Population size: " + population.Count);
        Debug.Log("Population initialized with " + populationSize + " genomes.");
        InitializePopulation();
    }

    private void Update()
    {
        if (NoPacmanLeft() && !isGenerationComplete)
        {
            isGenerationComplete = true; // Prevent further execution until reset
            EvolvePopulation();
            InitializePopulation();
            ResetAllGames();
        }
    }

    public float GetBestFitness()
    {
        return BestFitness;
    }


    public void SavePopulation()
    {
        string folderPath = Application.dataPath + "/Populations";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        Debug.Log($"Saving population. Current population size: {population.Count}");

        string filePath = $"{folderPath}/Population_{gen}.json";
        List<GenomeData> genomeDataList = new();

        foreach (Genome genome in population)
        {
            genomeDataList.Add(new GenomeData(genome));
        }

        string json = JsonUtility.ToJson(new PopulationData(genomeDataList), true);
        File.WriteAllText(filePath, json);

        Debug.Log($"Population saved to {filePath}");
    }

    public void LoadPopulation()
    {
        string folderPath = Application.dataPath + "/Populations";
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("No saved populations found!");
            return;
        }

        string[] files = Directory.GetFiles(folderPath, "Population_*.json");
        if (files.Length == 0)
        {
            Debug.LogError("No saved populations found!");
            return;
        }

        // Load the last generation file
        Debug.Log($"Loading population from {files.Length-1} files.");
        string lastFile = files[files.Length - 1];
        string json = File.ReadAllText(lastFile);
        gen = files.Length - 1; // Set the generation number based on the number of files
        PopulationData populationData = JsonUtility.FromJson<PopulationData>(json);
        population.Clear();

        foreach (GenomeData genomeData in populationData.genomes)
        {
            Genome genome = new Genome(genomeData);
            population.Add(genome);
        }

        Debug.Log($"Population loaded from {lastFile}");
    }

    private void SetPopulation()
    {
        for (int i = 0; i < populationSize; i++)
        {
            Genome g = pacmans[i].genome;
            population.Add(g);
        }
    }

    private void InitializePopulation()
    {
        for (int i = 0; i < populationSize; i++)
        {
            if (gameManagers[i].pacman is PacmanBrain pacmanBrain)
            {
                pacmanBrain.ChangeGenome(population[i]);
            }
        }   
    }

    public void EvolvePopulation()
    {   
        for (int i = 0; i < population.Count; i++)
        {
            population[i].SetFitness(pacmans[i].EvaluateFitness());
        }
        population.Sort((a, b) => b.GetFitness().CompareTo(a.GetFitness()));
        List<Genome> newPopulation = new();

        SavePopulation();

        Debug.Log("Population sorted by fitness:");
        foreach (var genome in population)
        {
            Debug.Log(genome.GetFitness());
        }

        // Add elite individuals to the new population
        for (int i = 0; i < nbElite; i++)
        {
            if (i == 0)
            {
                BestFitness = population[i].GetFitness();
                Debug.Log("Best fitness: " + BestFitness);
            }
            Genome best = population[i];
            newPopulation.Add(best);
        }

        // Allow elites to breed among themselves
        for (int i = 0; i < nbElite; i++)
        {
            for (int j = i + 1; j < nbElite; j++)
            {
                Genome child = population[i].Breed(population[j]);
                newPopulation.Add(child);
            }
        }

        // Breed remaining individuals with elites
        for (int i = nbElite; i < populationSize - nbDiversity; i++)
        {
            Genome strong = population[i % nbElite]; // Alternate between elites
            Genome weak = population[i];
            Genome child = strong.Breed(weak);
            child.Mutate(mutationRate);
            newPopulation.Add(child);
        }

        // Ensure the new population size matches the original population size
        while (newPopulation.Count > populationSize - nbDiversity)
        {
            newPopulation.RemoveAt(newPopulation.Count - 1); // Remove excess individuals
        }

        // Add randomized individuals to maintain diversity
        for (int i = 0; i < nbDiversity; i++)
        {
            Genome NewGenome = population[0].Clone();
            NewGenome.RandomizeWeights();
            newPopulation.Add(NewGenome);
        }
        
        population = newPopulation;
        gen++;
    }

    public bool NoPacmanLeft()
    {
        foreach ( PacmanBrain pacman in pacmans)
        {
            if (pacman.GetLives() > 0)
            {
                // Debug.Log("Pacman is alive: ");
                return false; // At least one Pacman is still alive
            }
        }
        return true; // All Pacman are dead
    }

    private void PrintPopulation()
    {
        foreach (Genome genome in population)
        {
            genome.PrintGenome();
        }
    }

    public void ResetAllGames()
    {
        if (gameManagers == null || gameManagers.Length == 0)
        {
            Debug.LogError("No GameManagers assigned to PopulationManager!");
            return;
        }

        int i = 0;
        if (gameManagers.Length != pacmans.Length || gameManagers.Length != population.Count)
        {
            Debug.LogError("GameManagers and Pacmans arrays must be of the same length!");
            Debug.LogError("GameManagers length: " + gameManagers.Length);
            Debug.LogError("Pacmans length: " + pacmans.Length);
            Debug.LogError("Population length: " + population.Count);
            return;
        }
        foreach (GameManager gameManager in gameManagers)
        {
            if (gameManager != null)
            {
                pacmans[i].ChangeGenome(population[i]);
                gameManager.NewGame();
            }
            else
            {
                Debug.LogError("GameManager reference is null!");
            }
            i++;
        }
        isGenerationComplete = false;
    }

    private void PrintPacmanGenomes()
    {
        Debug.Log("Population genome: \n");
        foreach (Genome genome in population)
        {
            genome.PrintGenome();
        }
    }
}
