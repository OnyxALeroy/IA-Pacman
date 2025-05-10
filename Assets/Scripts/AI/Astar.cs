using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Astar : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] PacmanCoord pacmanCoord;
    [SerializeField] Pacman pacman;
    [SerializeField] bool canUserInteract = false;
    [SerializeField] bool debugMode = false;

    // A* components
    public (int, int) currentDestination;
    private Queue<(int, int)> currentPath = new Queue<(int, int)>();
    private bool hasDestinationChanged = false;
    public Queue<(int, int)> CurrentPath => currentPath;

    // Grid-relative attributes
    private bool[,] mapMatrix;
    private Vector2Int mapSize;
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

        // FIXME: call the method to get this
        mapMatrix = null;
        mapSize = new Vector2Int(mapMatrix.GetLength(0), mapMatrix.GetLength(1));
        if (debugMode) { PrintMatrixInConsole(); }

        mapGraph = GraphBuilder.BuildGraph(mapMatrix, debugMode);
        setNewDestination(GetPacmanPositionInGrid().Item1, GetPacmanPositionInGrid().Item2);
        hasDestinationChanged = true;
        Debug.Log($"Default destination set: {currentDestination}");
    }

    // --------------------------------------------------------------------------------------------

    public void setNewDestination(int targetX, int targetY){
        currentDestination = (targetX, targetY);
        hasDestinationChanged = true;
        currentPath = FindShortestPath();
    }

    void Update(){
        if (Input.GetMouseButtonDown(0) && canUserInteract)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cell = tilemap.WorldToCell(world);
            Debug.Log("Clicked tilemap cell: " + cell);

            // Convert the clicked cell to our grid coordinates
            (int x, int y) = (cell.x, -cell.y);
            
            // Check if this is a valid tile and set it as destination
            if (x >= 0 && x < mapSize.x && y >= 0 && y < mapSize.y && mapMatrix[x, y])
            {
                currentDestination = (x, y);
                hasDestinationChanged = true;
                Debug.Log($"New destination set: {currentDestination}");
            }
            else
            {
                Debug.LogWarning($"Invalid destination at grid position ({x},{y})");
                Debug.LogWarning($"Accounted map size ({mapSize.x},{mapSize.y})");
                Debug.LogWarning($"HasTile? = {mapMatrix[x, y]}");
            }
        }

        // Recalculate path if destination has changed
        if (hasDestinationChanged){
            if (aStarLineRenderer != null) { aStarLineRenderer.positionCount = 0; }

            hasDestinationChanged = false;

            if (debugMode){
                Debug.Log($"Path calculation complete. Path count: {currentPath.Count}");
                string pathPoints = "Path points: ";
                foreach (var point in currentPath)
                {
                    pathPoints += $"({point.Item1},{point.Item2}) ";
                }
                Debug.Log(pathPoints);

                Vector3 startingPosition = pacman.transform.position;
                Vector3 endingPosition = GridToWorldPosition(currentDestination.Item1, currentDestination.Item2);

                DrawStraightLine(startingPosition, endingPosition);

                if (currentPath.Count > 0)
                {
                    Debug.Log($"Drawing path with {currentPath.Count} points");
                    DrawAStarPath(new Queue<(int, int)>(currentPath));
                }
                else
                {
                    Debug.LogWarning("No path found to draw");
                }
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    public Vector3 GridToWorldPosition(int gridX, int gridY)
    {
        Vector3Int tilePosition = new Vector3Int(gridX, -gridY, 0);
        return tilemap.CellToWorld(tilePosition) + tilemap.layoutGrid.cellSize / 2f;
    }

    // Calculate heuristic (Manhattan distance)
    private int CalculateHeuristic((int, int) node, (int, int) goal)
    {
        return Math.Abs(node.Item1 - goal.Item1) + Math.Abs(node.Item2 - goal.Item2);
    }

    private (int, int) GetMinimizingNode(List<(int, int)> nodes, Dictionary<(int, int), int> costsFromStart, (int, int) goal){
        if (nodes.Count == 0) { Debug.LogError("Cannot find minimizing node from empty list"); return (-1, -1); }

        (int, int) minimizingNode = nodes[0]; // Default to first node
        int minimalCost = costsFromStart[minimizingNode] + CalculateHeuristic(minimizingNode, goal);

        foreach((int, int) node in nodes){
            int cost = costsFromStart[node] + CalculateHeuristic(node, goal);

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

        // Check if destination is valid
        if (!mapMatrix[currentDestination.Item1, currentDestination.Item2])
        {
            Debug.LogError($"Destination {currentDestination} is not a valid tile!");
            return new Queue<(int, int)>();
        }

        List<(int, int)> toExplore = new List<(int, int)>{ initialPosition }; // Nodes to consider
        List<(int, int)> explored = new List<(int, int)>(); // Considered nodes
        Dictionary<(int, int), (int, int)> fathers = new Dictionary<(int, int), (int, int)>();

        // Initialize costs from start point
        Dictionary<(int, int), int> costsFromStart = new Dictionary<(int, int), int>{ {initialPosition, 0} };
        
        // A* algorithm
        while(toExplore.Count > 0){
            // Safeguard check
            if (toExplore.Count == 0) break;
            
            (int, int) currentNode = GetMinimizingNode(toExplore, costsFromStart, currentDestination);
            
            if (currentNode.Equals(currentDestination)){
                return RebuildPathFromDestination(currentNode, fathers);
            }
            
            // Explore neighbors
            toExplore.Remove(currentNode);
            explored.Add(currentNode);
            
            // Check if the node exists in the graph before getting neighbors
            if (!mapGraph.edges.ContainsKey(currentNode))
            {
                Debug.LogError($"Node {currentNode} not found in graph!");
                continue;
            }
            
            foreach((int, int) neighbor in mapGraph.Neighbors(currentNode)){
                int tentativeCost = costsFromStart[currentNode] + 1; // Cost to move is 1
                
                // Initialize cost if not yet set
                if (!costsFromStart.ContainsKey(neighbor))
                {
                    costsFromStart[neighbor] = int.MaxValue;
                }
                
                if (tentativeCost < costsFromStart[neighbor])
                {
                    // Found a better path
                    fathers[neighbor] = currentNode;
                    costsFromStart[neighbor] = tentativeCost;
                    
                    if (explored.Contains(neighbor))
                    { 
                        explored.Remove(neighbor); 
                    }
                    
                    if (!toExplore.Contains(neighbor))
                    { 
                        toExplore.Add(neighbor); 
                    }
                }
            }
        }
        
        Debug.LogWarning($"Could not find path to {currentDestination}");
        return new Queue<(int, int)>();
    }

    // --------------------------------------------------------------------------------------------

    public (int, int) GetPacmanPositionInGrid(){
        Vector3Int coords = pacmanCoord.GetPacmanCoords();
        return (coords.x, -coords.y);
    }

    // --------------------------------------------------------------------------------------------

    private void PrintMatrixInConsole(){
        int rows = mapMatrix.GetLength(0);
        int cols = mapMatrix.GetLength(1);

        Debug.Log("Map Matrix:");
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
        if (straightLineRenderer == null) return;
        
        straightLineRenderer.positionCount = 2;  // Set two points for the line
        straightLineRenderer.SetPosition(0, start);
        straightLineRenderer.SetPosition(1, end);
    }

    // Draw the A* path
    void DrawAStarPath(Queue<(int, int)> path)
    {
        if (aStarLineRenderer == null) { Debug.LogError("aStarLineRenderer is null"); return; }
        if (path.Count == 0) { Debug.LogWarning("Path is empty, nothing to draw"); return; }
        
        List<Vector3> worldPositions = new List<Vector3>();
        
        // Add Pacman's current position as start
        Vector3 startPos = pacman.transform.position;
        worldPositions.Add(startPos);
        Debug.Log($"Starting path draw at: {startPos}");
        
        // Add all path points
        foreach (var step in path)
        {
            Vector3 worldPos = GridToWorldPosition(step.Item1, step.Item2);
            worldPositions.Add(worldPos);
            Debug.Log($"Added path point: Grid({step.Item1},{step.Item2}) -> World({worldPos})");
        }

        Debug.Log($"Total points in path visualization: {worldPositions.Count}");
        
        // Set line renderer positions
        aStarLineRenderer.positionCount = worldPositions.Count;
        for (int i = 0; i < worldPositions.Count; i++)
        {
            aStarLineRenderer.SetPosition(i, worldPositions[i]);
            Debug.Log($"Set line position {i} to {worldPositions[i]}");
        }
    }
}
