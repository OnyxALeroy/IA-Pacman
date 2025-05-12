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
    [SerializeField] List<Ghost> ghosts = new List<Ghost>();
    [SerializeField] bool doPathConsiderGhosts = true;
    [SerializeField] bool canUserInteract = false;
    [SerializeField] bool drawPaths = false;
    [SerializeField] bool debugMode = false;

    // A* components
    public (int, int) currentStart;
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
        if (drawPaths){ InitializeLineRenderers(); }

        mapDebugger.StartTilemapDebugger();
        mapMatrix = mapDebugger.transposedWalkableMatrix;
        if (debugMode) { PrintMatrixInConsole(); }

        mapGraph = GraphBuilder.BuildGraph(mapMatrix, drawPaths);
        GraphBuilder.DrawGraph(mapGraph, tilemap);
        setNewDestination(GetPacmanPositionInGrid().Item1, GetPacmanPositionInGrid().Item2, GetPacmanPositionInGrid().Item1, GetPacmanPositionInGrid().Item2);
        hasDestinationChanged = true;
        if (debugMode) { Debug.Log($"Default destination set: {currentDestination}"); }
    }

    // --------------------------------------------------------------------------------------------

    public void setNewDestination(int startX, int startY, int targetX, int targetY, bool doConsiderGhosts = false){
        currentStart = (startX, startY);
        currentDestination = (targetX, targetY);
        if (debugMode) { Debug.Log($"Attempting to go to ({targetX}, {targetY}), with MapMatrix={mapMatrix[targetX, targetY]}"); }
        hasDestinationChanged = true;
        currentPath = FindShortestPath(doConsiderGhosts);
    }

    void Update(){
        if (Input.GetMouseButtonDown(0) && canUserInteract)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cell = tilemap.WorldToCell(world);
            if (debugMode) { Debug.Log("Clicked tilemap cell: " + cell); }

            // Convert the clicked cell to our grid coordinates
            (int y, int x) = (cell.x, -cell.y);
            
            // Check if this is a valid tile and set it as destination
            if (!mapMatrix[x, y])
            {
                setNewDestination(GetPacmanPositionInGrid().Item1, GetPacmanPositionInGrid().Item2, x, y, doPathConsiderGhosts);            
                if (debugMode) { Debug.Log($"New destination set: {currentDestination}"); }
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

            if (drawPaths){
                if (debugMode) { Debug.Log($"Path calculation complete. Path count: {currentPath.Count}"); }
                string pathPoints = "Path points: ";
                foreach (var point in currentPath)
                {
                    pathPoints += $"({point.Item1},{point.Item2}) ";
                }
                if (debugMode) { Debug.Log(pathPoints); }

                Vector3 startingPosition = pacman.transform.position;
                Vector3 endingPosition = GridToWorldPosition(currentDestination.Item2, currentDestination.Item1);

                DrawStraightLine(startingPosition, endingPosition);

                if (currentPath.Count > 0)
                {
                    if (debugMode) { Debug.Log($"Drawing path with {currentPath.Count} points"); }
                    DrawAStarPath(new Queue<(int, int)>(currentPath));
                }
                else
                {
                    if (debugMode) { Debug.LogWarning("No path found to draw"); }
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

    public Queue<(int, int)> FindShortestPath(bool doConsiderGhosts)
    {
        // Check if destination is valid
        if (mapMatrix[currentDestination.Item1, currentDestination.Item2])
        {
            if (debugMode) { Debug.LogError($"Destination {currentDestination} is not a valid tile!"); }
            return new Queue<(int, int)>();
        }

        // If considering Ghosts, removing their position from the graph
        Graph<(int, int)> graph = mapGraph.DeepCopy();
        if (doConsiderGhosts){
            foreach (Ghost ghost in ghosts){
                Vector3Int ghostCoords = tilemap.WorldToCell(ghost.transform.position);
                (int, int) ghostPosInGrid = (-ghostCoords.y, ghostCoords.x);

                if (graph.edges.ContainsKey(ghostPosInGrid)){
                    graph.edges.Remove(ghostPosInGrid);
                }
                foreach ((int, int) key in graph.edges.Keys){
                    if (graph.edges[key].Contains(ghostPosInGrid)){
                        graph.edges[key].Remove(ghostPosInGrid);
                    }
                }
            }
        }

        // Check if the initial position and destination are in the graph
        if (!graph.edges.ContainsKey(currentStart))
        {
            Debug.LogError($"Initial position {currentStart} is not in the graph!");
            return new Queue<(int, int)>();
        }
        
        if (!graph.edges.ContainsKey(currentDestination))
        {
            Debug.LogError($"Destination {currentDestination} is not in the graph!");
            return new Queue<(int, int)>();
        }

        // Priority queue would be better but we'll use a list
        List<(int, int)> openSet = new List<(int, int)>{ currentStart };
        HashSet<(int, int)> closedSet = new HashSet<(int, int)>();
        
        // Track the path with a previous node dictionary
        Dictionary<(int, int), (int, int)> cameFrom = new Dictionary<(int, int), (int, int)>();
        
        // Cost from start to each node
        Dictionary<(int, int), int> gScore = new Dictionary<(int, int), int>();
        gScore[currentStart] = 0;
        
        // Estimated total cost from start to goal through each node
        Dictionary<(int, int), int> fScore = new Dictionary<(int, int), int>();
        fScore[currentStart] = CalculateHeuristic(currentStart, currentDestination);
        
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
                return ReconstructPath(cameFrom, current, graph);
            }
            
            openSet.Remove(current);
            closedSet.Add(current);
            
            // Make sure this node exists in the graph
            if (!graph.edges.ContainsKey(current))
            {
                Debug.LogError($"Node {current} not found in graph during A* search!");
                continue;
            }
            
            // Check each neighbor
            foreach (var neighbor in graph.edges[current])
            {
                // Skip if already evaluated
                if (closedSet.Contains(neighbor))
                    continue;

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
    private Queue<(int, int)> ReconstructPath(Dictionary<(int, int), (int, int)> cameFrom, (int, int) current, Graph<(int, int)> graph)
    {
        // Build path in reverse
        List<(int, int)> path = new List<(int, int)>();
        
        while (cameFrom.ContainsKey(current))
        {
            // Verify this is a valid edge in our graph
            (int, int) previous = cameFrom[current];
            
            // Double check that this edge exists in the graph
            if (!graph.edges.ContainsKey(previous) || !graph.edges[previous].Contains(current))
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
        foreach (var node in path){
            result.Enqueue(node);
        }

        return result;
    }

    // --------------------------------------------------------------------------------------------

    public (int, int) GetPacmanPositionInGrid(){
        Vector3Int coords = pacmanCoord.GetPacmanCoords();
        if (debugMode) { Debug.Log($"Pacman Coords = ({-coords.y}, {coords.x})"); }
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
            
            if (debugMode) { Debug.Log($"Added path point: Grid({pathArray[i].Item2}, {pathArray[i].Item1}) -> World({worldPos})"); }
        }

        if (debugMode) { Debug.Log($"Total points in path visualization: {worldPositions.Count}"); }

        // Set line renderer positions
        aStarLineRenderer.positionCount = worldPositions.Count;
        for (int i = 0; i < worldPositions.Count; i++)
        {
            aStarLineRenderer.SetPosition(i, worldPositions[i]);
            if (debugMode) { Debug.Log($"Set line position {i} to {worldPositions[i]}"); }
        }
    }
}
