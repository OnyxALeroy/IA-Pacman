using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenomeData
{
    public List<float> weightsInputHidden; // Flattened for serialization
    public List<float> weightsHiddenOutput; // Flattened for serialization
    public int inputSize;
    public int hiddenSize;
    public int outputSize;
    public float fitness;

    public GenomeData(Genome genome)
    {
        inputSize = genome.weightsInputHidden.GetLength(0);
        hiddenSize = genome.weightsInputHidden.GetLength(1);
        outputSize = genome.weightsHiddenOutput.GetLength(1);

        weightsInputHidden = FlattenArray(genome.weightsInputHidden);
        weightsHiddenOutput = FlattenArray(genome.weightsHiddenOutput);
        fitness = genome.GetFitness();
    }

    // Helper method to flatten a 2D array into a 1D list
    private List<float> FlattenArray(float[,] array)
    {
        List<float> list = new();
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                list.Add(array[i, j]);
            }
        }
        return list;
    }
}