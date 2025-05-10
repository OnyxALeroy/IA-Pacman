using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDebugger : MonoBehaviour
{
    public Tilemap tilemap;
    public Vector3Int debugCellPosition = new Vector3Int(0, 0, 0);
    public bool[,] walkableMatrix;

    void Start()
    {
        GenerateWalkableMatrix();
    }

    void OnDrawGizmos()
    {
        if (tilemap == null) return;

        Vector3 worldPos = tilemap.CellToWorld(debugCellPosition);
        Gizmos.color = Color.red;
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

        DebugMatrix();
    }

    private void DebugMatrix()
    {
        for (int y = walkableMatrix.GetLength(1) - 1; y >= 0; y--)
        {
            string row = "";
            for (int x = 0; x < walkableMatrix.GetLength(0); x++)
            {
                row += walkableMatrix[x, y] ? "1 " : "0 ";
            }
        }
    }
}