using System.Collections.Generic;
using UnityEngine;

public class ComportementalAI : MonoBehaviour
{

    [SerializeField] TilemapDebugger mapDebugger;
    [SerializeField] Transform pellets;
    [SerializeField] Astar astar;
    [SerializeField] PacmanPathFollower pathFollower;
    [SerializeField] bool activateKillerBehaviour = false;
    [SerializeField] bool activateHungryBehaviour = false;

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
            if (activateHungryBehaviour){
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
            } else if(activateKillerBehaviour){
                // TODO: Goto the nearest Pac-gum, then haunt ghosts
            }

            Debug.Log($"Target is ({target.Item1}, {target.Item2})");
            pathFollower.SetNextWaypoint(target);
        }
    }
}
