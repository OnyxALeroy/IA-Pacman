using UnityEngine;
using System.Collections.Generic;

public class Genome
{
    private int inputSize = 7; // Example: Pacman's position, ghost positions, pellet positions, etc.
    private int hiddenSize = 10; // Number of neurons in the hidden layer
    private int outputSize = 4; // Up, Down, Left, Right
    public float[,] weightsInputHidden;
    public float[,] weightsHiddenOutput;

    private float[] hiddenLayer; // Values of the hidden layer
    private float[] outputLayer; // Values of the output layer

    private float fitness; 

    public Genome()
    {
        weightsInputHidden = new float[inputSize, hiddenSize];
        weightsHiddenOutput = new float[hiddenSize, outputSize];
        RandomizeWeights();
    }

    public Genome(int inputSize, int hiddenSize, int outputSize)
    {
        weightsInputHidden = new float[inputSize, hiddenSize];
        weightsHiddenOutput = new float[hiddenSize, outputSize];
        RandomizeWeights();
    }

    public Genome(GenomeData data)
    {
        weightsInputHidden = UnflattenArray(data.weightsInputHidden, data.inputSize, data.hiddenSize);
        weightsHiddenOutput = UnflattenArray(data.weightsHiddenOutput, data.hiddenSize, data.outputSize);
        fitness = data.fitness;
    }

    // Helper method to unflatten a 1D list into a 2D array
    private float[,] UnflattenArray(List<float> list, int rows, int cols)
    {
        float[,] array = new float[rows, cols];
        int index = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array[i, j] = list[index++];
            }
        }
        return array;
    }

    public void RandomizeWeights()
    {
        for (int i = 0; i < weightsInputHidden.GetLength(0); i++)
            for (int j = 0; j < weightsInputHidden.GetLength(1); j++)
                weightsInputHidden[i, j] = Random.Range(-1f, 1f);

        for (int i = 0; i < weightsHiddenOutput.GetLength(0); i++)
            for (int j = 0; j < weightsHiddenOutput.GetLength(1); j++)
                weightsHiddenOutput[i, j] = Random.Range(-1f, 1f);
    }

    public Genome Clone()
    {
        var clone = new Genome(weightsInputHidden.GetLength(0), weightsInputHidden.GetLength(1), weightsHiddenOutput.GetLength(1));

        for (int i = 0; i < weightsInputHidden.GetLength(0); i++)
            for (int j = 0; j < weightsInputHidden.GetLength(1); j++)
                clone.weightsInputHidden[i, j] = weightsInputHidden[i, j];

        for (int i = 0; i < weightsHiddenOutput.GetLength(0); i++)
            for (int j = 0; j < weightsHiddenOutput.GetLength(1); j++)
                clone.weightsHiddenOutput[i, j] = weightsHiddenOutput[i, j];

        return clone;
    }

    public Genome Breed(Genome other)
    {
        Genome child = new Genome(inputSize, hiddenSize, outputSize);

        for (int i = 0; i < weightsInputHidden.GetLength(0); i++)
            for (int j = 0; j < weightsInputHidden.GetLength(1); j++)
                child.weightsInputHidden[i, j] = Random.Range(0f, 1f) < 0.6f ? weightsInputHidden[i, j] : other.weightsInputHidden[i, j];

        for (int i = 0; i < weightsHiddenOutput.GetLength(0); i++)
            for (int j = 0; j < weightsHiddenOutput.GetLength(1); j++)
                child.weightsHiddenOutput[i, j] = Random.Range(0f, 1f) < 0.6f ? weightsHiddenOutput[i, j] : other.weightsHiddenOutput[i, j];

        return child;
    }

    public void Mutate(float mutationRate)
    {
        for (int i = 0; i < weightsInputHidden.GetLength(0); i++)
            for (int j = 0; j < weightsInputHidden.GetLength(1); j++)
                if (Random.value < mutationRate)
                    weightsInputHidden[i, j] += Random.Range(-0.5f, 0.5f);

        for (int i = 0; i < weightsHiddenOutput.GetLength(0); i++)
            for (int j = 0; j < weightsHiddenOutput.GetLength(1); j++)
                if (Random.value < mutationRate)
                    weightsHiddenOutput[i, j] += Random.Range(-0.5f, 0.5f);
    }

    public Vector2 GetNextDirection(float[] inputs)
    {
        // Forward propagation
        hiddenLayer = new float[hiddenSize];
        outputLayer = new float[outputSize];

        // Input to Hidden
        for (int i = 0; i < hiddenSize; i++)
        {
            float inputToHiddenSum = 0f;
            for (int j = 0; j < inputSize; j++)
            {
                inputToHiddenSum += inputs[j] * weightsInputHidden[j, i];
            }
            hiddenLayer[i] = (float)System.Math.Tanh(inputToHiddenSum); // Activation function
        }

        // Hidden to Output
        for (int i = 0; i < outputSize; i++)
        {
            float hiddenToOutputSum = 0f;
            for (int j = 0; j < hiddenSize; j++)
            {
                hiddenToOutputSum += hiddenLayer[j] * weightsHiddenOutput[j, i];
            }
            outputLayer[i] = (float)System.Math.Tanh(hiddenToOutputSum); // Activation function
        }
        
        // Determine the direction with the highest output value
        int maxIndex = 0;
        for (int i = 1; i < outputSize; i++)
        {
            if (outputLayer[i] > outputLayer[maxIndex])
            {
                maxIndex = i;
            }
        }

        // Map the output index to a direction
        return maxIndex switch
        {
            0 => Vector2.up,
            1 => Vector2.down,
            2 => Vector2.left,
            3 => Vector2.right,
            _ => Vector2.zero,
        };
    }

    public void PrintGenome()
    {
        Debug.Log("Genome Weights:");
        Debug.Log("Input to Hidden Layer:");
        for (int i = 0; i < weightsInputHidden.GetLength(0); i++)
        {
            string row = "";
            for (int j = 0; j < weightsInputHidden.GetLength(1); j++)
            {
                row += weightsInputHidden[i, j] + " ";
            }
            Debug.Log(row);
        }

        Debug.Log("Hidden to Output Layer:");
        for (int i = 0; i < weightsHiddenOutput.GetLength(0); i++)
        {
            string row = "";
            for (int j = 0; j < weightsHiddenOutput.GetLength(1); j++)
            {
                row += weightsHiddenOutput[i, j] + " ";
            }
            Debug.Log(row);
        }
    }

    public void SetFitness(float fitness)
    {
        this.fitness = fitness;
    }

    public float GetFitness()
    {
        return fitness;
    }

}
