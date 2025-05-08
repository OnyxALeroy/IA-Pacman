using System;
using System.Collections.Generic;
using UnityEngine;

// ------------------------------------------------------------------------------------------------

public class Graph<Location>
{
    public Dictionary<Location, List<Location>> edges
        = new Dictionary<Location, List<Location>>();

    public List<Location> Neighbors(Location id)
    {
        return edges[id];
    }
};

// ------------------------------------------------------------------------------------------------

public class GraphBuilder{
    public static Graph<(int, int)> BuildGraph(bool[,] matrix, bool showDebug = false){
        Graph<(int, int)> g = new Graph<(int, int)>();
        Dictionary<(int, int), List<(int, int)>> edges = new Dictionary<(int, int), List<(int, int)>>();

        for (int i = 0; i < matrix.GetLength(0); i++){
            for (int j = 0; j < matrix.GetLength(1); j++){
                if (matrix[i, j]){  // then the (i,j) case is free
                    List<(int, int)> neighbors = new List<(int, int)>();

                    if (i > 0){
                        if (matrix[i-1, j]){
                            neighbors.Add((i-1, j));
                        }
                    }
                    if (i < matrix.GetLength(0) - 1){
                        if (matrix[i+1, j]){
                            neighbors.Add((i+1, j));
                        }
                    }
                    if (j > 0){
                        if (matrix[i, j-1]){
                            neighbors.Add((i, j-1));
                        }
                    }
                    if (j < matrix.GetLength(1) - 1){
                        if (matrix[i, j+1]){
                            neighbors.Add((i, j+1));
                        }
                    }

                    edges[(i, j)] = neighbors;
                }
            }
        }

        g.edges = edges;

        if (showDebug){
            int availableTilesCount = 0;
            for (int i = 0; i < matrix.GetLength(0); i++){
                for (int j = 0; j < matrix.GetLength(1); j++){
                    if (matrix[i, j]){  // then the (i,j) case is free
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
};
