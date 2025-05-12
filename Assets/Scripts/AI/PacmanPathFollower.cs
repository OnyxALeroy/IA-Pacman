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
    public bool IsFollowingPath => isFollowingPath;

    private void Start(){
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

    private void Update(){
        if (isFollowingPath) { Goto(); }
    }
    
    private void Goto(){
        // Convert grid waypoint to world position
        Vector3 targetPosition = GridToWorldPosition(currentWaypoint.Item2, currentWaypoint.Item1);
        Debug.LogWarning($"targetPosition = ({targetPosition.x}, {targetPosition.y})");

        // Calculate direction and move
        Vector3 currentPosition = pacman.transform.position;
        Vector3 direction = (targetPosition - currentPosition).normalized;

        // Move pacman toward the waypoint
        pacman.transform.position += direction * moveSpeed * Time.deltaTime;

        // Determine pacman's rotation based on direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        pacman.transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        // Check if we've reached the waypoint
        float distanceToWaypoint = Vector3.Distance(currentPosition, targetPosition);
        if (distanceToWaypoint < nextWaypointDistance){
            SetNextWaypoint((-1, -1));
        }
    }

    public void SetNextWaypoint((int, int) waypoint){
        currentWaypoint = waypoint;
        isFollowingPath = !(waypoint == (-1, -1));
    }
    
    Vector3 GridToWorldPosition(int gridX, int gridY){
        return astar.GridToWorldPosition(gridX, gridY);
    }
}
