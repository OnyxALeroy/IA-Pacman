using System;
using UnityEngine;

public class Genome : MonoBehaviour
{
    public float[,] weightsInputHidden;
    public float[,] weightsHiddenOutput;
    public float fitness;

    public Genome(int inputSize, int hiddenSize, int outputSize)
    {
        weightsInputHidden = new float[inputSize, hiddenSize];
        weightsHiddenOutput = new float[hiddenSize, outputSize];
        RandomizeWeights();
    }

    public void RandomizeWeights()
    {
        for (int i = 0; i < weightsInputHidden.GetLength(0); i++)
        {
            for (int j = 0; j < weightsInputHidden.GetLength(1); j++)
            {
                weightsInputHidden[i, j] = UnityEngine.Random.Range(-1f, 1f);
            }
        }

        for (int i = 0; i < weightsHiddenOutput.GetLength(0); i++)
        {
            for (int j = 0; j < weightsHiddenOutput.GetLength(1); j++)
            {
                weightsHiddenOutput[i, j] = UnityEngine.Random.Range(-1f, 1f);
            }
        }
    }

    public Genome Clone()
    {
        Genome clone = new Genome(
            weightsInputHidden.GetLength(0),
            weightsInputHidden.GetLength(1),
            weightsHiddenOutput.GetLength(1)
        );

        Array.Copy(weightsInputHidden, clone.weightsInputHidden, weightsInputHidden.Length);
        Array.Copy(weightsHiddenOutput, clone.weightsHiddenOutput, weightsHiddenOutput.Length);
        return clone;
    }

    public void Mutate(float mutationRate)
    {
        for (int i = 0; i < weightsInputHidden.GetLength(0); i++)
        {
            for (int j = 0; j < weightsInputHidden.GetLength(1); j++)
            {
                if (UnityEngine.Random.value < mutationRate)
                    weightsInputHidden[i, j] += UnityEngine.Random.Range(-0.5f, 0.5f);
            }
        }

        for (int i = 0; i < weightsHiddenOutput.GetLength(0); i++)
        {
            for (int j = 0; j < weightsHiddenOutput.GetLength(1); j++)
            {
                if (UnityEngine.Random.value < mutationRate)
                    weightsHiddenOutput[i, j] += UnityEngine.Random.Range(-0.5f, 0.5f);
            }
        }
    }
}
