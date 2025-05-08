using UnityEngine;
using System;

public class NeuralNetwork : MonoBehaviour
{
    private int inputSize = 5; // Example: Pacman's position, ghost positions, pellet positions, etc.
    private int hiddenSize = 10; // Number of neurons in the hidden layer
    private int outputSize = 4; // Up, Down, Left, Right

    private float[,] weightsInputHidden; // Weights between input and hidden layer
    private float[,] weightsHiddenOutput; // Weights between hidden and output layer
    private float[] hiddenLayer; // Values of the hidden layer
    private float[] outputLayer; // Values of the output layer

    private void Start()
    {
        InitializeWeights();
    }

    private void InitializeWeights()
    {
        // Initialize weights with random values
        weightsInputHidden = new float[inputSize, hiddenSize];
        weightsHiddenOutput = new float[hiddenSize, outputSize];

        for (int i = 0; i < inputSize; i++)
        {
            for (int j = 0; j < hiddenSize; j++)
            {
                weightsInputHidden[i, j] = UnityEngine.Random.Range(-1f, 1f);
            }
        }

        for (int i = 0; i < hiddenSize; i++)
        {
            for (int j = 0; j < outputSize; j++)
            {
                weightsHiddenOutput[i, j] = UnityEngine.Random.Range(-1f, 1f);
            }
        }
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
            hiddenLayer[i] = (float)Math.Tanh(inputToHiddenSum); // Activation function
        }

        // Hidden to Output
        for (int i = 0; i < outputSize; i++)
        {
            float hiddenToOutputSum = 0f;
            for (int j = 0; j < hiddenSize; j++)
            {
                hiddenToOutputSum += hiddenLayer[j] * weightsHiddenOutput[j, i];
            }
            outputLayer[i] = (float)Math.Tanh(hiddenToOutputSum); // Activation function
        }

        // Debug the output layer
        Debug.Log($"Outputs: {string.Join(", ", outputLayer)}");

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
        switch (maxIndex)
        {
            case 0: return Vector2.up;
            case 1: return Vector2.down;
            case 2: return Vector2.left;
            case 3: return Vector2.right;
            default: return Vector2.zero;
        }
    }
}
