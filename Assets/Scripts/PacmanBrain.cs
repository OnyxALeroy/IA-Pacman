using UnityEngine;

public class PacmanBrain : Pacman
{
    public Genome genome { get; set; }
    public float fitness { get; private set; } = 0f;

    private float aliveTime = 0f; // Timer to track how long Pacman is alive

    private float powerPelletActive = 0f; 

    private new void Awake()
    {
        base.Awake();
        Application.runInBackground = true;

        this.genome = new Genome();

        if (this.genome == null)
        {
            Debug.LogError("Genome component is missing on the GameObject!");
        }
    }

    private void Update()
    {
        if (this.movement == null || this.genome == null)
        {
            Debug.LogError("Movement or Genome is not initialized!");
            return;
        }

        // Increment the alive time while Pacman is alive
        if (this.lives > 0)
        {
            aliveTime += Time.deltaTime; // Increment by the time elapsed since the last frame
        }

        // Gather inputs for the neural network
        float[] inputs = GatherInputs();

        // Get the next direction from the neural network
        Vector2 nextDirection = genome.GetNextDirection(inputs);

        // Set Pacman's direction
        this.movement.SetDirection(nextDirection);

        // Rotate Pacman to face the direction of movement
        float angle = Mathf.Atan2(-this.movement.direction.x, this.movement.direction.y);
        this.transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    public override void ResetState()
    {
        base.ResetState();
    }

    public void ResetTimer()
    {
        aliveTime = 0f;
    }

    public void ChangeGenome(Genome newGenome)
    {
        genome = newGenome;
    }

    private float[] GatherInputs()
    {
        float[] inputs = new float[7];

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

        // Distance to the nearest power pellet (normalized)
        inputs[5] = FindClosestPowerPelletDistance()/10f;

        inputs[6] = powerPelletActive;

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

        private float FindClosestPowerPelletDistance()
    {
        float closestDistance = float.MaxValue;

        foreach (Transform pellet in GameObject.Find("Pellets").transform)
        {
            PowerPellet powerPellet = pellet.GetComponent<PowerPellet>();
            if (pellet.gameObject.activeSelf && powerPellet != null)
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

    public void ActivatePowerPellet(float duration)
    {
        powerPelletActive = 1f; // Set to active
        Debug.Log("PowerPellet activated!");

        // Reset the state after the duration ends
        Invoke(nameof(DeactivatePowerPellet), duration);
    }

    private void DeactivatePowerPellet()
    {
        powerPelletActive = 0f; // Set to inactive
        Debug.Log("PowerPellet deactivated!");
    }

    public float EvaluateFitness()
    {
        float calculatedFitness = (float)System.Math.Round((float)this.score + 500 * (float)this.lives + this.aliveTime * 5, 1); // Include alive time in fitness
        Debug.Log($"Evaluating Fitness: Score = {this.score}, Lives = {this.lives}, Alive Time = {aliveTime}, Calculated Fitness = {calculatedFitness}");
        return calculatedFitness;
    }
}
