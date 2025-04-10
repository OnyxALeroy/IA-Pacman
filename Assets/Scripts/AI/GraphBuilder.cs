using System;
using System.Collections.Generic;

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
    public static Graph<(int, int)> BuildGraph(bool[,] matrix){
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
        return g;
    }
};
