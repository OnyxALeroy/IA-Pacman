using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Astar))]
public class PacmanPathFollower : MonoBehaviour
{
    [SerializeField] private Pacman pacman;
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float nextWaypointDistance = 0.1f; // How close Pacman needs to get to a waypoint before moving to the next one
    
    private Astar astar;
    private Queue<(int, int)> currentPath;
    private (int, int) currentWaypoint = (-1, -1);
    private bool isFollowingPath = false;
    
    void Start()
    {
        astar = GetComponent<Astar>();
        
        if (pacman == null)
        {
            pacman = FindObjectOfType<Pacman>();
            if (pacman == null)
            {
                Debug.LogError("Pacman reference not set and could not be found!");
            }
        }
    }
    
    void Update()
    {
        // If we don't have a path or we've finished following it, get a new path
        if (!isFollowingPath)
        {
            // Only start following if there's a path available
            if (astar.currentPath != null && astar.currentPath.Count > 0)
            {
                currentPath = new Queue<(int, int)>(astar.currentPath);
                isFollowingPath = true;
                GetNextWaypoint();
            }
        }
        else
        {
            // Follow the current path
            FollowPath();
        }
    }
    
    void FollowPath()
    {
        if (currentWaypoint == (-1, -1))
        {
            if (currentPath.Count > 0)
            {
                GetNextWaypoint();
            }
            else
            {
                // We've reached the end of the path
                isFollowingPath = false;
                // Set destination to -1,-1 to trigger finding a new random destination
                astar.currentDestination = (-1, -1);
                return;
            }
        }
        
        // Convert grid waypoint to world position
        Vector3 targetPosition = GridToWorldPosition(currentWaypoint.Item1, currentWaypoint.Item2);
        
        // Calculate direction and move
        Vector3 currentPosition = pacman.transform.position;
        Vector3 direction = (targetPosition - currentPosition).normalized;
        
        // Move pacman toward the waypoint
        pacman.transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Determine pacman's rotation based on direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        pacman.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Check if we've reached the waypoint
        float distanceToWaypoint = Vector3.Distance(currentPosition, targetPosition);
        if (distanceToWaypoint < nextWaypointDistance)
        {
            GetNextWaypoint();
        }
    }
    
    void GetNextWaypoint()
    {
        if (currentPath.Count > 0)
        {
            currentWaypoint = currentPath.Dequeue();
        }
        else
        {
            currentWaypoint = (-1, -1);
            isFollowingPath = false;
        }
    }
    
    Vector3 GridToWorldPosition(int gridX, int gridY)
    {
        // Reference the A* script's conversion method directly to ensure consistency
        return astar.GridToWorldPosition(gridX, gridY);
    }
}