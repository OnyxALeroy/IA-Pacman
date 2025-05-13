using System.Collections.Generic;
using UnityEngine;

public class ComportementalAI : MonoBehaviour
{

    [SerializeField] TilemapDebugger mapDebugger;
    [SerializeField] Transform pellets;
    [SerializeField] Astar astar;
    [SerializeField] PacmanPathFollower pathFollower;
    [SerializeField] List<Ghost> ghosts = new List<Ghost>();
    [SerializeField] int dangerRadius = 3;

    private Dictionary<Vector2Int, Transform> pelletCoords;

    private void Start(){
        pelletCoords = new Dictionary<Vector2Int, Transform>();
        Vector2 topLeftTileCoord = new Vector2(mapDebugger.positionInGrid.x, mapDebugger.positionInGrid.y);
        Vector3 tileSize = mapDebugger.tilemap.cellSize;
        foreach(Transform pellet in pellets)
        {
            Vector3 pelletCenter = pellet.position;
            Vector2Int pC = new Vector2Int((int)((pelletCenter.y - topLeftTileCoord.y)/tileSize.y), (int)((pelletCenter.x - topLeftTileCoord.x)/tileSize.x));
            pC = new Vector2Int(-pC.x, pC.y);
            pelletCoords[pC] = pellet;
        }        
    }

    public void HandleUpdate(){
        if (!pathFollower.IsFollowingPath){
            (int, int) target = (-1, -1);

            Graph<(int, int)> graph = GraphBuilder.BuildGraph(astar.MapMatrix);
            List<(int, int)> reachableTiles = graph.GetAllReachableLocations(astar.GetPacmanPositionInGrid(), dangerRadius);
            if (CheckIfGhostNearby(reachableTiles)){
                (int, int) safePoint = GetSafestTile(reachableTiles);
                astar.setNewDestination(astar.GetPacmanPositionInGrid().Item1, astar.GetPacmanPositionInGrid().Item2, safePoint.Item1, safePoint.Item2, false);
                if (astar.CurrentPath.Count > 0){
                    target = astar.CurrentPath.Peek();
                }
            } else {
                Vector2Int nearestPelletCoords = new Vector2Int(-1, -1);
                (int, int) newTarget = (-1, -1);
                int distance = int.MaxValue;
                Vector2Int pacmanCoords = new Vector2Int(astar.GetPacmanPositionInGrid().Item1, astar.GetPacmanPositionInGrid().Item2);
                foreach (Vector2Int pelletCoord in pelletCoords.Keys){
                    if (pelletCoords[pelletCoord].gameObject.activeSelf){
                        astar.setNewDestination(pacmanCoords.x, pacmanCoords.y, pelletCoord.x, pelletCoord.y, false);
                        if (astar.CurrentPath.Count < distance && astar.CurrentPath.Count > 0){
                            nearestPelletCoords = pelletCoord;
                            distance = astar.CurrentPath.Count;
                            newTarget = astar.CurrentPath.Peek();
                        }
                    }
                }
                target = newTarget;
            }
            pathFollower.SetNextWaypoint(target);
        }
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------

    private bool CheckIfGhostNearby(List<(int, int)> tiles){
        bool res = false;
        foreach (Ghost ghost in ghosts){
            Vector3Int ghostCoord = mapDebugger.tilemap.WorldToCell(ghost.transform.position);
            if (tiles.Contains((-ghostCoord.y, ghostCoord.x))){
                res = true;
                break;
            }
        }

        return res;
    }

    private (int, int) GetSafestTile(List<(int, int)> reachableTiles){
        (int, int) safestTile = (-1, -1);
        int maxTotalDistance = int.MinValue;

        foreach ((int x, int y) in reachableTiles)
        {
            int totalDistance = 0;

            foreach (Ghost ghost in ghosts)
            {
                Vector3Int ghostCoord = mapDebugger.tilemap.WorldToCell(ghost.transform.position);
                Debug.LogError($"Ghost coords: ({-ghostCoord.y}, {ghostCoord.x})");
                Debug.LogError($"Tile considered: ({x}, {y})");
                astar.setNewDestination(x, y, -ghostCoord.y, ghostCoord.x, false);
                Debug.LogWarning($"Ghost path count: {astar.CurrentPath.Count}");

                if (astar.CurrentPath.Count == 0)
                {
                    continue;
                }

                totalDistance += astar.CurrentPath.Count;
            }

            if (totalDistance > maxTotalDistance)
            {
                maxTotalDistance = totalDistance;
                safestTile = (x, y);
            }
        }

        Debug.LogWarning($"Safest tile: ({safestTile.Item1}, {safestTile.Item2})");

        return safestTile;
    }
}
