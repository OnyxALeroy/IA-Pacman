using UnityEngine;

public class PacmanBrain : Pacman
{
    private NeuralNetwork neuralNetwork { get; set; }
    public float fitness { get; private set; } = 0f;

    private new void Awake()
    {
        base.Awake(); // Call the base class's Awake method
        this.neuralNetwork = GetComponent<NeuralNetwork>();

        if (this.neuralNetwork == null)
        {
            Debug.LogError("NeuralNetwork component is missing on the GameObject!");
        }
    }

    private void Update()
    {
        if (this.movement == null || this.neuralNetwork == null)
        {
            Debug.LogError("Movement or NeuralNetwork is not initialized!");
            return;
        }

        // Gather inputs for the neural network
        float[] inputs = GatherInputs();

        // Get the next direction from the neural network
        Vector2 nextDirection = neuralNetwork.GetNextDirection(inputs);

        // Set Pacman's direction
        this.movement.SetDirection(nextDirection);

        // Rotate Pacman to face the direction of movement
        float angle = Mathf.Atan2(-this.movement.direction.x, this.movement.direction.y);
        this.transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    private float[] GatherInputs()
    {
        float[] inputs = new float[5];

        // Pacman's position (normalized)
        inputs[0] = this.transform.position.x / 10f; // Assuming the game area is roughly -10 to 10
        inputs[1] = this.transform.position.y / 10f;

        // Closest ghost's position (normalized)
        Ghost closestGhost = FindClosestGhost();
        if (closestGhost != null)
        {
            inputs[2] = (closestGhost.transform.position.x - this.transform.position.x) / 10f;
            inputs[3] = (closestGhost.transform.position.y - this.transform.position.y) / 10f;
        }
        else
        {
            inputs[2] = 0f;
            inputs[3] = 0f;
        }

        // Distance to the nearest pellet (normalized)
        inputs[4] = FindClosestPelletDistance() / 10f;

        // Debug the inputs
        Debug.Log($"Inputs: {string.Join(", ", inputs)}");

        return inputs;
    }

    private Ghost FindClosestGhost()
    {
        Ghost[] ghosts = FindObjectsByType<Ghost>(FindObjectsSortMode.None);
        Ghost closestGhost = null;
        float closestDistance = float.MaxValue;

        foreach (Ghost ghost in ghosts)
        {
            float distance = Vector2.Distance(this.transform.position, ghost.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestGhost = ghost;
            }
        }

        return closestGhost;
    }

    private float FindClosestPelletDistance()
    {
        float closestDistance = float.MaxValue;

        foreach (Transform pellet in GameObject.Find("Pellets").transform)
        {
            if (pellet.gameObject.activeSelf)
            {
                float distance = Vector2.Distance(this.transform.position, pellet.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
        }

        return closestDistance;
    }

    public void EvaluateFitness()
    {
        this.fitness = this.score;
    }
}
