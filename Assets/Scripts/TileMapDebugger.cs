using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDebugger : MonoBehaviour
{
    public Tilemap tilemap;
    public Vector3Int debugCellPosition = new Vector3Int(0, 0, 0);
    public bool[,] walkableMatrix;
    public bool[,] transposedWalkableMatrix;

    public void StartTilemapDebugger()
    {
        GenerateWalkableMatrix();
    }
    public Vector3 positionInGrid = new Vector3(0, 0, 0);

    void OnDrawGizmos()
    {
        if (tilemap == null) return;

        Vector3 worldPos = tilemap.CellToWorld(debugCellPosition);
        Gizmos.color = Color.red;
        positionInGrid = worldPos + tilemap.cellSize / 2;
        Gizmos.DrawSphere(worldPos + tilemap.cellSize / 2, 0.1f);
    }

    private void GenerateWalkableMatrix()
    {
        BoundsInt bounds = tilemap.cellBounds;
        bool[,] fullMatrix = new bool[bounds.size.x, bounds.size.y];

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(cellPosition);
                fullMatrix[x - bounds.xMin, y - bounds.yMin] = tile != null;
            }
        }

        TrimMatrix(fullMatrix, bounds);

        BuildTransposedMatrix();
    }

    private void TrimMatrix(bool[,] fullMatrix, BoundsInt bounds)
    {
        int minX = bounds.size.x, maxX = 0, minY = bounds.size.y, maxY = 0;

        for (int x = 0; x < fullMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < fullMatrix.GetLength(1); y++)
            {
                if (fullMatrix[x, y])
                {
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }
        }

        walkableMatrix = new bool[maxX - minX + 1, maxY - minY + 1];

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                walkableMatrix[x - minX, y - minY] = fullMatrix[x, y];
            }
        }

        DebugMatrix(walkableMatrix);
    }

    private void DebugMatrix(bool[,] matrix)
    {
        for (int y = matrix.GetLength(1) - 1; y >= 0; y--)
        {
            string row = "";
            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                row += matrix[x, y] ? "1 " : "0 ";
            }
            // Debug.Log(row);
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------

    private void BuildTransposedMatrix(){
        transposedWalkableMatrix = new bool[walkableMatrix.GetLength(1), walkableMatrix.GetLength(0)];
        for (int y = walkableMatrix.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < walkableMatrix.GetLength(0); x++)
            {
                transposedWalkableMatrix[walkableMatrix.GetLength(1) - y - 1, x] = walkableMatrix[x, y];
            }
        }
        // Debug.Log("Transposable Walkable Matrix =");
        for (int x = 0; x < transposedWalkableMatrix.GetLength(0); x++){
            string row = "";
            for (int y = 0; y < transposedWalkableMatrix.GetLength(1); y++){
                row += transposedWalkableMatrix[x, y] ? "1 " : "0 ";
            }
            // Debug.Log(row);
        }
    }
}
