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
        float[] inputs = new float[15]; // Updated size to include power pellet inputs

        // Pacman's position (normalized)
        inputs[0] = this.transform.position.x / 10f; // Assuming the game area is roughly -10 to 10
        inputs[1] = this.transform.position.y / 10f;

        // Closest ghost's position (normalized) and direction
        Ghost closestGhost = FindClosestGhost();
        if (closestGhost != null)
        {
            Vector2 relativeGhostPosition = closestGhost.transform.position - this.transform.position;
            inputs[2] = relativeGhostPosition.x / 10f; // X distance
            inputs[3] = relativeGhostPosition.y / 10f; // Y distance

            // Direction to the closest ghost
            inputs[4] = relativeGhostPosition.x < 0 ? -1f : 1f; // Left (-1) or Right (1)
            inputs[5] = relativeGhostPosition.y < 0 ? -1f : 1f; // Bottom (-1) or Top (1)
        }
        else
        {
            inputs[2] = 0f;
            inputs[3] = 0f;
            inputs[4] = 0f;
            inputs[5] = 0f;
        }

        // Closest pellet's position (normalized) and direction
        Vector2 closestPelletPosition = FindClosestPelletPosition();
        if (closestPelletPosition != Vector2.zero)
        {
            Vector2 relativePelletPosition = closestPelletPosition - (Vector2)this.transform.position;
            inputs[6] = relativePelletPosition.x / 10f; // X distance
            inputs[7] = relativePelletPosition.y / 10f; // Y distance

            // Direction to the closest pellet
            inputs[8] = relativePelletPosition.x < 0 ? -1f : 1f; // Left (-1) or Right (1)
            inputs[9] = relativePelletPosition.y < 0 ? -1f : 1f; // Bottom (-1) or Top (1)
        }
        else
        {
            inputs[6] = 0f;
            inputs[7] = 0f;
            inputs[8] = 0f;
            inputs[9] = 0f;
        }

        // Closest power pellet's position (normalized) and direction
        Vector2 closestPowerPelletPosition = FindClosestPowerPelletPosition();
        if (closestPowerPelletPosition != Vector2.zero)
        {
            Vector2 relativePowerPelletPosition = closestPowerPelletPosition - (Vector2)this.transform.position;
            inputs[10] = relativePowerPelletPosition.x / 10f; // X distance
            inputs[11] = relativePowerPelletPosition.y / 10f; // Y distance

            // Direction to the closest power pellet
            inputs[12] = relativePowerPelletPosition.x < 0 ? -1f : 1f; // Left (-1) or Right (1)
            inputs[13] = relativePowerPelletPosition.y < 0 ? -1f : 1f; // Bottom (-1) or Top (1)
        }
        else
        {
            inputs[10] = 0f;
            inputs[11] = 0f;
            inputs[12] = 0f;
            inputs[13] = 0f;
        }

        // Power pellet active status
        inputs[14] = powerPelletActive;

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

    private Vector2 FindClosestPelletPosition()
    {
        float closestDistance = float.MaxValue;
        Vector2 closestPelletPosition = Vector2.zero;

        foreach (Transform pellet in GameObject.Find("Pellets").transform)
        {
            if (pellet.gameObject.activeSelf)
            {
                float distance = Vector2.Distance(this.transform.position, pellet.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPelletPosition = pellet.position;
                }
            }
        }

        return closestPelletPosition;
    }

    private Vector2 FindClosestPowerPelletPosition()
    {
        float closestDistance = float.MaxValue;
        Vector2 closestPowerPelletPosition = Vector2.zero;

        foreach (Transform pellet in GameObject.Find("Pellets").transform)
        {
            PowerPellet powerPellet = pellet.GetComponent<PowerPellet>();
            if (pellet.gameObject.activeSelf && powerPellet != null)
            {
                float distance = Vector2.Distance(this.transform.position, pellet.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPowerPelletPosition = pellet.position;
                }
            }
        }

        return closestPowerPelletPosition;
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
