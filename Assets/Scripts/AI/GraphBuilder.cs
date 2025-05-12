using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// ------------------------------------------------------------------------------------------------

public class Graph<Location>
{
    public Dictionary<Location, List<Location>> edges
        = new Dictionary<Location, List<Location>>();

    public List<Location> Neighbors(Location id)
    {
        return edges[id];
    }
    
    // ---------------------------------------------------------------------------------------------------------------------

    public Graph<Location> DeepCopy(){
        Graph<Location> g = new Graph<Location>();
        
        Dictionary<Location, List<Location>> newEdges = new Dictionary<Location, List<Location>>();
        foreach (Location key in edges.Keys){
            newEdges[key] = edges[key];
        }

        g.edges = newEdges;
        return g;
    }
};

// ------------------------------------------------------------------------------------------------

public class GraphBuilder{
    public static Graph<(int, int)> BuildGraph(bool[,] matrix, bool showDebug = false){
        Graph<(int, int)> g = new Graph<(int, int)>();
        Dictionary<(int, int), List<(int, int)>> edges = new Dictionary<(int, int), List<(int, int)>>();

        for (int i = 0; i < matrix.GetLength(0); i++){
            for (int j = 0; j < matrix.GetLength(1); j++){
                if (!matrix[i, j]){  // then the (i,j) case is free
                    List<(int, int)> neighbors = new List<(int, int)>();

                    if (i > 0){
                        if (!matrix[i - 1, j]){
                            neighbors.Add((i - 1, j));
                        }
                    }
                    if (i < matrix.GetLength(0) - 1){
                        if (!matrix[i + 1, j]){
                            neighbors.Add((i + 1, j));
                        }
                    }
                    if (j > 0){
                        if (!matrix[i, j - 1]){
                            neighbors.Add((i, j - 1));
                        }
                    }
                    if (j < matrix.GetLength(1) - 1){
                        if (!matrix[i, j + 1]){
                            neighbors.Add((i, j + 1));
                        }
                    }

                    edges[(i, j)] = neighbors;
                }
            }
        }

        // Adding map folding
        for (int i = 0; i < matrix.GetLength(0); i++){
            if (!matrix[i, 0] && !matrix[i, matrix.GetLength(1) - 1]){
                edges[(i, 0)].Add((i, matrix.GetLength(1) - 1));
                edges[(i, matrix.GetLength(1) - 1)].Add((i, 0));
            }
        }
        for (int j = 0; j < matrix.GetLength(1); j++){
            if (!matrix[0, j] && !matrix[matrix.GetLength(0) - 1, j]){
                edges[(0, j)].Add((matrix.GetLength(0) - 1, j));
                edges[(matrix.GetLength(0) - 1, j)].Add((0, j));
            }
        }

        g.edges = edges;

        if (showDebug){
            int availableTilesCount = 0;
            for (int i = 0; i < matrix.GetLength(0); i++){
                for (int j = 0; j < matrix.GetLength(1); j++){
                    if (!matrix[i, j]){  // then the (i,j) case is free
                        availableTilesCount += 1;
                    }
                }
            }

            Debug.Log("Graph construction complete");
            Debug.Log($"Total nodes in graph: {g.edges.Count}, from a map with {availableTilesCount} available Tiles.");

            // Choose a few random nodes to check their connections
            int nodeCount = 0;
            foreach (var node in g.edges.Keys)
            {
                if (nodeCount < 5) // Display info for the first 5 nodes only
                {
                    Debug.Log($"Node at ({node.Item1},{node.Item2}) has {g.edges[node].Count} neighbors");
                    string neighborInfo = "Connects to: ";
                    foreach (var neighbor in g.edges[node])
                    {
                        neighborInfo += $"({neighbor.Item1},{neighbor.Item2}) ";
                    }
                    Debug.Log(neighborInfo);
                    nodeCount++;
                }
                else
                {
                    break;
                }
            }
        }

        return g;
    }

    public static void DrawGraph(Graph<(int, int)> graph, Tilemap tilemap)
    {
        // Remove any existing visualization
        GameObject existingViz = GameObject.Find("GraphVisualization");
        if (existingViz != null)
        {
            GameObject.Destroy(existingViz);
        }
        
        // Create a parent GameObject for the graph visualization
        GameObject graphVisualObject = new GameObject("GraphVisualization");
        
        // Create node indicators
        foreach (var node in graph.edges.Keys)
        {
            GameObject nodeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nodeObj.name = $"Node_{node.Item1}_{node.Item2}";
            nodeObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            nodeObj.transform.position = GridToWorldPosition(node.Item2, node.Item1, tilemap);
            nodeObj.GetComponent<Renderer>().material.color = Color.yellow;
            nodeObj.transform.parent = graphVisualObject.transform;
        }
        
        // Dictionary to track suspicious edges
        Dictionary<string, bool> edgeValidity = new Dictionary<string, bool>();
        int validEdges = 0;
        int suspiciousEdges = 0;
        
        // First pass: Check all edges and mark suspicious ones
        foreach (var node in graph.edges.Keys)
        {
            foreach (var neighbor in graph.edges[node])
            {
                // Check if this is a valid move (one grid unit apart)
                int dX = Math.Abs(node.Item1 - neighbor.Item1);
                int dY = Math.Abs(node.Item2 - neighbor.Item2);
                bool isValidMove = (dX == 1 && dY == 0) || (dX == 0 && dY == 1);
                
                // Generate a consistent edge ID
                string edgeId = GetEdgeId(node, neighbor);
                
                if (!edgeValidity.ContainsKey(edgeId))
                {
                    edgeValidity[edgeId] = isValidMove;
                    
                    if (isValidMove)
                        validEdges++;
                    else
                        suspiciousEdges++;
                }
            }
        }
        
        // Second pass: Draw all edges
        foreach (var node in graph.edges.Keys)
        {
            foreach (var neighbor in graph.edges[node])
            {
                string edgeId = GetEdgeId(node, neighbor);
                
                // Skip if we've already drawn this edge
                if (!edgeValidity.ContainsKey(edgeId))
                    continue;
                    
                bool isValid = edgeValidity[edgeId];
                
                // Skip this edge if already drawn
                if (!edgeValidity.ContainsKey(edgeId))
                    continue;
                    
                // Remove from dictionary to mark as drawn
                bool isValidMove = edgeValidity[edgeId];
                edgeValidity.Remove(edgeId);
                
                // Create line renderer for this edge
                GameObject edgeObject = new GameObject($"Edge_{node.Item1}_{node.Item2}_to_{neighbor.Item1}_{neighbor.Item2}");
                LineRenderer lineRenderer = edgeObject.AddComponent<LineRenderer>();
                lineRenderer.positionCount = 2;
                lineRenderer.startWidth = 0.03f;
                lineRenderer.endWidth = 0.03f;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                
                // Set color based on validity
                if (isValidMove)
                {
                    lineRenderer.startColor = Color.cyan;
                    lineRenderer.endColor = Color.cyan;
                }
                else
                {
                    // Mark suspicious edges in red
                    lineRenderer.startColor = Color.red;
                    lineRenderer.endColor = Color.red;
                    Debug.LogWarning($"⚠️ Suspicious edge: ({node.Item1},{node.Item2}) -> ({neighbor.Item1},{neighbor.Item2}), " +
                                    $"dX={Math.Abs(node.Item1 - neighbor.Item1)}, dY={Math.Abs(node.Item2 - neighbor.Item2)}");
                }
                
                // Set positions
                Vector3 startPos = GridToWorldPosition(node.Item2, node.Item1, tilemap);
                Vector3 endPos = GridToWorldPosition(neighbor.Item2, neighbor.Item1, tilemap);
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, endPos);
                
                edgeObject.transform.parent = graphVisualObject.transform;
            }
        }
        
        Debug.Log($"Graph visualization completed: {validEdges} valid edges, {suspiciousEdges} suspicious edges");
    }

    // Helper to get a consistent edge ID regardless of edge direction
    private static string GetEdgeId((int, int) node1, (int, int) node2)
    {
        // Create a unique identifier that's the same regardless of direction
        if (node1.Item1 < node2.Item1 || (node1.Item1 == node2.Item1 && node1.Item2 < node2.Item2))
            return $"{node1.Item1}_{node1.Item2}_{node2.Item1}_{node2.Item2}";
        else
            return $"{node2.Item1}_{node2.Item2}_{node1.Item1}_{node1.Item2}";
    }

    // Helper method to convert grid positions to world positions
    private static Vector3 GridToWorldPosition(int gridX, int gridY, Tilemap tilemap)
    {
        Vector3Int tilePosition = new Vector3Int(gridX, -gridY, 0);
        return tilemap.CellToWorld(tilePosition) + tilemap.layoutGrid.cellSize / 2f;
    }
};
