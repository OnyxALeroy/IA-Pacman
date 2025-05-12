using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Astar : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] TilemapDebugger mapDebugger;
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
        aStarLineRenderer.startWidth = 0.5f;
        aStarLineRenderer.endWidth = 0.5f;
        aStarLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        aStarLineRenderer.startColor = Color.green;
        aStarLineRenderer.endColor = Color.green;
    }

    void Start(){
        if (debugMode){ InitializeLineRenderers(); }

        mapDebugger.StartTilemapDebugger();
        mapMatrix = mapDebugger.transposedWalkableMatrix;
        if (debugMode) { PrintMatrixInConsole(); }

        mapGraph = GraphBuilder.BuildGraph(mapMatrix, debugMode);
        GraphBuilder.DrawGraph(mapGraph, tilemap);
        setNewDestination(GetPacmanPositionInGrid().Item1, GetPacmanPositionInGrid().Item2);
        hasDestinationChanged = true;
        Debug.Log($"Default destination set: {currentDestination}");
    }

    // --------------------------------------------------------------------------------------------

    public void setNewDestination(int targetX, int targetY){
        currentDestination = (targetX, targetY);
        Debug.Log($"Attempting to go to ({targetX}, {targetY}), with MapMatrix={mapMatrix[targetX, targetY]}");
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
            (int y, int x) = (cell.x, -cell.y);
            
            // Check if this is a valid tile and set it as destination
            if (!mapMatrix[x, y])
            {
                setNewDestination(x, y);
                Debug.Log($"New destination set: {currentDestination}");
            }
            else
            {
                Debug.LogWarning($"Invalid destination at grid position ({x},{y})");
                Debug.LogWarning($"mapMatrix[x, y] = {mapMatrix[x, y]}");
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
                Vector3 endingPosition = GridToWorldPosition(currentDestination.Item2, currentDestination.Item1);

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

    public Queue<(int, int)> FindShortestPath()
    {
        (int, int) initialPosition = GetPacmanPositionInGrid();

        // Check if destination is valid
        if (mapMatrix[currentDestination.Item1, currentDestination.Item2])
        {
            Debug.LogError($"Destination {currentDestination} is not a valid tile!");
            return new Queue<(int, int)>();
        }

        // Check if the initial position and destination are in the graph
        if (!mapGraph.edges.ContainsKey(initialPosition))
        {
            Debug.LogError($"Initial position {initialPosition} is not in the graph!");
            return new Queue<(int, int)>();
        }
        
        if (!mapGraph.edges.ContainsKey(currentDestination))
        {
            Debug.LogError($"Destination {currentDestination} is not in the graph!");
            return new Queue<(int, int)>();
        }

        // Priority queue would be better but we'll use a list
        List<(int, int)> openSet = new List<(int, int)>{ initialPosition };
        HashSet<(int, int)> closedSet = new HashSet<(int, int)>();
        
        // Track the path with a previous node dictionary
        Dictionary<(int, int), (int, int)> cameFrom = new Dictionary<(int, int), (int, int)>();
        
        // Cost from start to each node
        Dictionary<(int, int), int> gScore = new Dictionary<(int, int), int>();
        gScore[initialPosition] = 0;
        
        // Estimated total cost from start to goal through each node
        Dictionary<(int, int), int> fScore = new Dictionary<(int, int), int>();
        fScore[initialPosition] = CalculateHeuristic(initialPosition, currentDestination);
        
        while (openSet.Count > 0)
        {
            // Find node in openSet with lowest fScore
            (int, int) current = GetLowestFScoreNode(openSet, fScore);
            
            if (debugMode)
                Debug.Log($"Evaluating node: ({current.Item1}, {current.Item2}) with fScore: {fScore[current]}");
            
            // Check if we've reached the destination
            if (current.Equals(currentDestination))
            {
                if (debugMode)
                    Debug.Log("Found path to destination!");
                return ReconstructPath(cameFrom, current);
            }
            
            openSet.Remove(current);
            closedSet.Add(current);
            
            // Make sure this node exists in the graph
            if (!mapGraph.edges.ContainsKey(current))
            {
                Debug.LogError($"Node {current} not found in graph during A* search!");
                continue;
            }
            
            // Check each neighbor
            foreach (var neighbor in mapGraph.edges[current])
            {
                // Skip if already evaluated
                if (closedSet.Contains(neighbor))
                    continue;
                
                // Validate this is a legitimate move (adjacent tiles only)
                int dX = Math.Abs(current.Item1 - neighbor.Item1);
                int dY = Math.Abs(current.Item2 - neighbor.Item2);
                
                if (dX > 1 || dY > 1 || (dX == 1 && dY == 1))
                {
                    Debug.LogWarning($"Skipping invalid edge: {current} -> {neighbor}, dX={dX}, dY={dY}");
                    continue;
                }
                
                // Calculate tentative gScore
                int tentativeGScore = gScore.ContainsKey(current) ? gScore[current] + 1 : int.MaxValue;
                
                // Initialize neighbor scores if needed
                if (!gScore.ContainsKey(neighbor))
                    gScore[neighbor] = int.MaxValue;
                
                if (!fScore.ContainsKey(neighbor))
                    fScore[neighbor] = int.MaxValue;
                
                // Skip if this path is not better
                if (tentativeGScore >= gScore[neighbor])
                    continue;
                
                // This path is better, record it
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + CalculateHeuristic(neighbor, currentDestination);
                
                // Add to open set if not already there
                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);
            }
        }
        
        Debug.LogWarning($"Could not find path to {currentDestination}");
        return new Queue<(int, int)>();
    }

    // Find node with lowest fScore
    private (int, int) GetLowestFScoreNode(List<(int, int)> nodes, Dictionary<(int, int), int> fScore)
    {
        if (nodes.Count == 0)
            return (-1, -1);
            
        (int, int) lowestNode = nodes[0];
        int lowestScore = fScore.ContainsKey(lowestNode) ? fScore[lowestNode] : int.MaxValue;
        
        foreach (var node in nodes)
        {
            int score = fScore.ContainsKey(node) ? fScore[node] : int.MaxValue;
            if (score < lowestScore)
            {
                lowestScore = score;
                lowestNode = node;
            }
        }
        
        return lowestNode;
    }

    // Reconstruct path from destination back to start
    private Queue<(int, int)> ReconstructPath(Dictionary<(int, int), (int, int)> cameFrom, (int, int) current)
    {
        // Build path in reverse
        List<(int, int)> path = new List<(int, int)>();
        
        while (cameFrom.ContainsKey(current))
        {
            // Verify this is a valid edge in our graph
            (int, int) previous = cameFrom[current];
            
            // Double check that this edge exists in the graph
            if (!mapGraph.edges.ContainsKey(previous) || !mapGraph.edges[previous].Contains(current))
            {
                Debug.LogError($"Invalid path step! No edge between {previous} and {current}");
                // We could break here, but let's continue to see the full problematic path
            }
            
            // Add node to path
            path.Add(current);
            current = previous;
        }
        
        // Reverse to get start-to-destination order
        path.Reverse();
        
        if (debugMode)
        {
            Debug.Log($"Path created with {path.Count} steps");
            string pathStr = "Path: ";
            foreach (var node in path)
                pathStr += $"({node.Item1},{node.Item2}) ";
            Debug.Log(pathStr);
        }
        
        // Convert to queue
        Queue<(int, int)> result = new Queue<(int, int)>();
        foreach (var node in path)
            result.Enqueue(node);
            
        return result;
    }


    private Queue<(int, int)> ValidateAndRebuildPath((int, int) pathDestination, Dictionary<(int, int), (int, int)> fathers){
        Queue<(int, int)> path = new Queue<(int, int)>();
        List<(int, int)> pathNodes = new List<(int, int)>();
        (int, int) node = pathDestination;
        
        // Building the path node by node (in reverse)
        while(fathers.ContainsKey(node)){
            pathNodes.Add(node);
            node = fathers[node];
        }
        
        // Reverse the path nodes
        pathNodes.Reverse();
        
        // Validate each step in the path - there must be an edge between consecutive nodes
        (int, int) prevNode = node; // This is our starting point, which wasn't added to pathNodes
        bool pathIsValid = true;
        
        foreach (var currentNode in pathNodes)
        {
            // Check if there's an edge from prevNode to currentNode
            if (!mapGraph.edges.ContainsKey(prevNode) || !mapGraph.edges[prevNode].Contains(currentNode))
            {
                Debug.LogError($"Invalid path step! No edge between {prevNode} and {currentNode}");
                pathIsValid = false;
                break;
            }
            
            prevNode = currentNode;
        }
        
        if (!pathIsValid)
        {
            Debug.LogError("Path validation failed! The path contains invalid edges.");
            return new Queue<(int, int)>();
        }
        
        // If path is valid, rebuild it as a queue
        foreach (var step in pathNodes)
        {
            path.Enqueue(step);
        }
        
        return path;
    }

    // --------------------------------------------------------------------------------------------

    public (int, int) GetPacmanPositionInGrid(){
        Vector3Int coords = pacmanCoord.GetPacmanCoords();
        Debug.Log($"Pacman Coords = ({-coords.y}, {coords.x})");
        return (-coords.y, coords.x);
    }

    // --------------------------------------------------------------------------------------------

    private void PrintMatrixInConsole(){
        int rows = mapMatrix.GetLength(0);
        int cols = mapMatrix.GetLength(1);

        Debug.Log("Map Matrix:");
        for (int x = 0; x < rows; x++)
        {
            string line = "";
            for (int y = cols - 1; y >= 0; y--)
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
        
        // Convert path points to world positions
        var pathArray = path.ToArray(); // Convert to array to preserve order
        for (int i = 0; i < pathArray.Length; i++)
        {
            Vector3 worldPos = GridToWorldPosition(pathArray[i].Item2, pathArray[i].Item1);
            worldPositions.Add(worldPos);
            Debug.Log($"Added path point: Grid({pathArray[i].Item2}, {pathArray[i].Item1}) -> World({worldPos})");
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
