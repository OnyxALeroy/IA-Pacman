using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Astar : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] Vector2Int mapSize;
    [SerializeField] Pacman pacman;
    [SerializeField] Func<(int, int), int> heuristic;
    [SerializeField] bool debugMode = false;

    // A* components
    public (int, int) currentDestination = (-1, -1);
    public Queue<(int, int)> currentPath = new Queue<(int, int)>();
    private bool hasDestinationChanged = false;

    // Grid-relative attributes
    private bool[,] mapMatrix;
    private Graph<(int, int)> mapGraph;

    // Debug Components (for drawing the paths)
    private GameObject straightLineObject;
    private GameObject aStarLineObject;
    private LineRenderer straightLineRenderer;
    private LineRenderer aStarLineRenderer;

    // --------------------------------------------------------------------------------------------

    private void InitializeLineRenderers(){
        // Create a GameObject for the straight line
        straightLineObject = new GameObject("StraightLine");
        straightLineRenderer = straightLineObject.AddComponent<LineRenderer>();
        straightLineRenderer.positionCount = 0;
        straightLineRenderer.startWidth = 0.05f;
        straightLineRenderer.endWidth = 0.05f;
        straightLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        straightLineRenderer.startColor = Color.red;
        straightLineRenderer.endColor = Color.red;

        // Create a GameObject for the A* path
        aStarLineObject = new GameObject("AStarPath");
        aStarLineRenderer = aStarLineObject.AddComponent<LineRenderer>();
        aStarLineRenderer.positionCount = 0;
        aStarLineRenderer.startWidth = 0.05f;
        aStarLineRenderer.endWidth = 0.05f;
        aStarLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        aStarLineRenderer.startColor = Color.green;
        aStarLineRenderer.endColor = Color.green;
    }

    void Start(){
        if (debugMode){ InitializeLineRenderers(); }

        mapMatrix = new bool[mapSize.x, mapSize.y];

        // Determine the bottom-right corner of the area we want to examine
        BoundsInt bounds = tilemap.cellBounds;
        int startX = bounds.xMax - mapSize.x;
        int startY = bounds.yMin;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                // We go right-to-left and bottom-to-top in the tilemap
                Vector3Int cellPosition = new Vector3Int(startX + (mapSize.x - 1 - x), startY + y, 0);
                bool hasTile = tilemap.HasTile(cellPosition);
                mapMatrix[x, y] = hasTile;
            }
        }


        if (debugMode) { PrintMatrixInConsole(); }

        mapGraph = GraphBuilder.BuildGraph(mapMatrix);
    }

    // --------------------------------------------------------------------------------------------

    void Update(){


        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"MapSize {mapSize}");
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cell = tilemap.WorldToCell(world);
            cell.y = - cell.y;
            Debug.Log("Clicked tilemap cell: " + cell);

            // Highlight that cell
            Vector3 center = tilemap.GetCellCenterWorld(cell);
            Debug.DrawLine(center - Vector3.one * 0.3f, center + Vector3.one * 0.3f, Color.yellow, 2f);
        }

        // FIXME: for now, a random destination is set each time Pacman reaches the previous one
        if (currentDestination == (-1, -1)){
            currentDestination = GetRandomFreeLocation();
            hasDestinationChanged = true;
        }

        // If the wanted destination has changed, the Astar has to recalculate
        if (hasDestinationChanged){
            // currentPath = FindShortestPath();
            hasDestinationChanged = false;

            if (debugMode){
                // TODO: add debug drawings
                Vector3 startingPosition = pacman.transform.position;
                Vector3Int endingTilePosition = new Vector3Int(currentDestination.Item1, currentDestination.Item2, 0);
                Vector3 endingPosition = tilemap.CellToWorld(endingTilePosition) + tilemap.layoutGrid.cellSize / 2f;

                Debug.Log($"MapSize = {mapSize}");
                Debug.Log($"Starting position = {GetPacmanPositionInGrid()}");
                Debug.Log($"Ending position = {currentDestination}");
                DrawStraightLine(startingPosition, endingPosition);
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    private (int, int) GetMinimizingNode(List<(int, int)> nodes, Dictionary<(int, int), int> costsFromStart){
        (int, int) minimizingNode = (-1, -1);
        int minimalCost = int.MaxValue;

        foreach((int, int) node in nodes){
            int cost = costsFromStart[node] + heuristic(node);

            if (cost < minimalCost){
                minimalCost = cost;
                minimizingNode = node;
            }
        }

        return minimizingNode;
    }

    private Queue<(int, int)> RebuildPathFromDestination((int, int) pathDestination, Dictionary<(int, int), (int, int)> fathers){
        Queue<(int, int)> path = new Queue<(int, int)>();
        (int, int) node = pathDestination;
        
        // Building the path node by node (in reverse)
        while(fathers.ContainsKey(node)){
            path.Enqueue(node);
            node = fathers[node];
        }

        // Reverting the path (so that it's Start -> Finish, instead of Finish -> Start)
        Stack<(int, int)> stack = new Stack<(int, int)>();
        while (path.Count > 0) { stack.Push(path.Dequeue()); }
        while (stack.Count > 0) { path.Enqueue(stack.Pop()); }

        return path;
    }

    public Queue<(int, int)> FindShortestPath(){
        (int, int) initialPosition = GetPacmanPositionInGrid();
        List<(int, int)> toExplore = new List<(int, int)>{ initialPosition }; // Nodes to consider
        List<(int, int)> explored = new List<(int, int)>(); // Considered nodes
        Dictionary<(int, int), (int, int)> fathers = new Dictionary<(int, int), (int, int)>();

        // Initializing all costs from the start point (to infinity, except from the starting position)
        Dictionary<(int, int), int> costsFromStart = new Dictionary<(int, int), int>{ {initialPosition, 0} };
        for (int i = 0; i < mapSize.x; i++){
            for (int j = 0; j < mapSize.y; j++){
                if ((i, j) != initialPosition){
                    costsFromStart[(i, j)] = int.MaxValue;
                }
            }
        }

        // Finding the shortest path...
        // NOTE: alls costs between nodes are 1 here (that's why it's hardcoded)
        while(toExplore.Count > 0){
            (int, int) currentConsideredNode = GetMinimizingNode(toExplore, costsFromStart);
            if (currentConsideredNode == currentDestination){
                return RebuildPathFromDestination(currentConsideredNode, fathers);
            } else {
                // Explore every unvisited successors of the current node
                toExplore.Remove(currentConsideredNode);
                explored.Add(currentConsideredNode);
                foreach((int, int) successor in mapGraph.Neighbors(currentConsideredNode)){
                    if ( (!toExplore.Contains(successor) && !explored.Contains(successor)) || (1 < costsFromStart[successor]) ){
                        // Either successor hasn't been visited yet, or this new path is shorter than the previous one
                        fathers[successor] = currentConsideredNode;
                        costsFromStart[successor] = costsFromStart[currentConsideredNode] + 1;

                        // Successor has to be revisited again, due to the changes of its paths
                        if (explored.Contains(successor)){ explored.Remove(successor); }
                        if (!toExplore.Contains(successor)){ toExplore.Add(successor); }
                    }
                }
            }
        }
        // If nothing has been returned yet, then the wanted destination can't be reach
        return new Queue<(int, int)>();
    }

    // --------------------------------------------------------------------------------------------

    public (int, int) GetPacmanPositionInGrid(){
        return pacman.GetPositionInGrid(mapSize);
    }

    // --------------------------------------------------------------------------------------------

    // Useful only for debug or heuristics
    private (int, int) GetRandomFreeLocation(){
        List<(int, int)> freeCells = new List<(int, int)>();
        for (int x = 0; x < mapMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < mapMatrix.GetLength(1); y++)
            {
                if (mapMatrix[x, y])
                {
                    freeCells.Add((x, y));
                }
            }
        }

        if (freeCells.Count == 0)
            return (0, 0); // Fallback in case no free cell is found

        return freeCells[UnityEngine.Random.Range(0, freeCells.Count)];
    }

    private void PrintMatrixInConsole(){
        int rows = mapMatrix.GetLength(0);
        int cols = mapMatrix.GetLength(1);

        for (int y = cols - 1; y >= 0; y--)
        {
            string line = "";
            for (int x = 0; x < rows; x++)
            {
                line += mapMatrix[x, y] ? "1 " : "0 ";
            }
            Debug.Log(line);
        }

        Debug.Log("---------------------------------------");
    }

    // Draw the straight line path
    void DrawStraightLine(Vector3 start, Vector3 end)
    {
        straightLineRenderer.positionCount = 2;  // Set two points for the line
        straightLineRenderer.SetPosition(0, start);
        straightLineRenderer.SetPosition(1, end);
    }

    // Draw the A* path
    void DrawAStarPath(Queue<(int, int)> path)
    {
        aStarLineRenderer.positionCount = path.Count;  // Set the number of points
        int index = 0;
        foreach (var step in path)
        {
            // Assuming grid coordinates are translated to world space
            Vector3 worldPosition = new Vector3(step.Item1, 0, step.Item2);  // Replace with correct scale/adjustment
            aStarLineRenderer.SetPosition(index, worldPosition);
            index++;
        }
    }
}
