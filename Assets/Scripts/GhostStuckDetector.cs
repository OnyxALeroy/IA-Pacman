using UnityEngine;

[RequireComponent(typeof(Ghost))]
public class GhostStuckDetector : MonoBehaviour
{
    private Ghost ghost;
    private Vector3 lastPosition;
    private float stuckTimer;
    private float stuckThreshold = 1.0f; // Time in seconds to consider the ghost stuck
    private float positionThreshold = 0.01f; // Minimum movement to consider the ghost unstuck
    private Vector2 lastForcedDirection; // Tracks the last forced direction to avoid oscillation

    private void Awake()
    {
        ghost = GetComponent<Ghost>();
    }

    private void Start()
    {
        lastPosition = transform.position;
        lastForcedDirection = Vector2.zero;
    }

    private void Update()
    {
        // Check if the ghost has moved significantly
        if (Vector3.Distance(transform.position, lastPosition) < positionThreshold)
        {
            stuckTimer += Time.deltaTime;

            // If the ghost is stuck for too long, force a direction change
            if (stuckTimer >= stuckThreshold)
            {
                ForceChangeDirection();
                stuckTimer = 0; // Reset the timer after forcing a direction change
            }
        }
        else
        {
            // Reset the timer if the ghost is moving
            stuckTimer = 0;
        }

        lastPosition = transform.position;
    }

    private void ForceChangeDirection()
    {
        Debug.Log("Ghost is stuck! Forcing direction change.");

        // Get the available directions from the ghost's current position
        Node currentNode = GetCurrentNode();
        if (currentNode != null)
        {
            Vector2 newDirection = Vector2.zero;
            float maxDistance = float.MinValue;

            foreach (Vector2 availableDirection in currentNode.availableDirections)
            {
                // Avoid reversing direction or oscillating between two nodes
                if (availableDirection != -ghost.movement.direction && availableDirection != lastForcedDirection)
                {
                    // Prioritize directions that lead farther from the current position
                    Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y, 0.0f);
                    float distance = (ghost.target.position - newPosition).sqrMagnitude;

                    if (distance > maxDistance)
                    {
                        newDirection = availableDirection;
                        maxDistance = distance;
                    }
                }
            }

            // If a valid new direction is found, set it
            if (newDirection != Vector2.zero)
            {
                ghost.movement.SetDirection(newDirection, forced: true);
                lastForcedDirection = newDirection; // Update the last forced direction
                return;
            }
        }

        // If no valid direction is found, choose a random direction as a fallback
        ChooseRandomDirection(currentNode);
    }

    private void ChooseRandomDirection(Node currentNode)
    {
        if (currentNode != null)
        {
            foreach (Vector2 availableDirection in currentNode.availableDirections)
            {
                // Avoid reversing direction
                if (availableDirection != -ghost.movement.direction)
                {
                    ghost.movement.SetDirection(availableDirection, forced: true);
                    lastForcedDirection = availableDirection; // Update the last forced direction
                    return;
                }
            }
        }

        // As a last resort, reverse the current direction
        ghost.movement.SetDirection(-ghost.movement.direction, forced: true);
        lastForcedDirection = -ghost.movement.direction; // Update the last forced direction
    }

    private Node GetCurrentNode()
    {
        // Check if the ghost is currently on a node
        Collider2D collider = Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Node"));
        return collider != null ? collider.GetComponent<Node>() : null;
    }
}