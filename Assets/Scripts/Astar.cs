using UnityEngine;
using UnityEngine.Tilemaps;

public class Astar : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] Pacman pacman;

    private Vector2 mapSize;
    private bool[,] mapMatrix;

    void Start()
    {
        BoundsInt bounds = tilemap.cellBounds;
        mapSize = new Vector2(bounds.size.x, bounds.size.y);

        mapMatrix = new bool[bounds.size.x, bounds.size.y];        
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                bool hasTile = tilemap.HasTile(cellPosition);
                mapMatrix[x - bounds.xMin, y - bounds.yMin] = hasTile;
            }
        }
    }

    void Update()
    {
        Debug.Log(GetPacmanPositionInGrid());
        // If you want to print the matrix, uncomment the next line
        // PrintMatrixInConsole();
    }

    // --------------------------------------------------------------------------------------------

    public Vector2 GetPacmanPositionInGrid(){
        Vector2 pacmanAbsolutePosition = pacman.GetPositionInGrid();
        float xPos = pacmanAbsolutePosition.x + Mathf.FloorToInt((mapSize.x + 1) / 2);
        float yPos = mapSize.y - (pacmanAbsolutePosition.y + Mathf.FloorToInt((mapSize.y) / 2) + 3);
        return new Vector2(xPos, yPos);
    }

    // --------------------------------------------------------------------------------------------

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
}
