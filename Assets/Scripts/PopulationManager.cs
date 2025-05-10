using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    [SerializeField] private PacmanBrain[] pacmans;
    [SerializeField] private GameManager[] gameManagers;
    public int populationSize;
    public float mutationRate = 0.15f;

    private float BestFitness;

    public int gen = 0;

    private List<Genome> population = new();

    private void Start()
    {
        BestFitness = 0;
        populationSize = pacmans.Length;
        SetPopulation();
        PrintPopulation();
        Debug.Log("Population size: " + population.Count);
        Debug.Log("Population initialized with " + populationSize + " genomes.");
    }

    private void Update()
    {
        if (NoPacmanLeft())
        {
            EvolvePopulation();
            ResetAllGames();
        }
    }

    public float GetBestFitness()
    {
        return BestFitness;
    }

    void SetPopulation()
    {
        for (int i = 0; i < populationSize; i++)
        {
            Genome g = pacmans[i].genome;
            population.Add(g);
        }
    }

    public void EvolvePopulation()
    {   
        for (int i = 0; i < population.Count; i++)
        {
            Debug.Log("Evaluating fitness for Pacman " + i + ": " + pacmans[i].EvaluateFitness());
            population[i].SetFitness(pacmans[i].EvaluateFitness());
        }
        population.Sort((a, b) => b.GetFitness().CompareTo(a.GetFitness()));
        List<Genome> newPopulation = new();
        Genome best = population[0];
        BestFitness = best.GetFitness();
        newPopulation.Add(best);
        for (int i = 1; i < populationSize-2; i ++)
        {
            Genome weak = population[i];
            Genome child = best.Breed(weak);
            child.Mutate(mutationRate);

            newPopulation.Add(child);
        }
        for (int j = populationSize-2; j < populationSize; j++)
        {
            Genome NewGenome = population[j].Clone();
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
            return;
        }
        foreach (GameManager gameManager in gameManagers)
        {
            if (gameManager != null)
            {
                Debug.Log("NewGame of GameManager: " + gameManager.name);
                pacmans[i].ChangeGenome(population[i]);
                gameManager.NewGame();
            }
            else
            {
                Debug.LogError("GameManager reference is null!");
            }
            i++;
        }
    }
}
