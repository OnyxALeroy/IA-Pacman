using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDebugger : MonoBehaviour
{
    public Tilemap tilemap; // drag your tilemap here in Inspector
    public Vector3Int debugCellPosition = new Vector3Int(0, 0, 0); // tile to debug

    void OnDrawGizmos()
    {
        if (tilemap == null) return;

        Vector3 worldPos = tilemap.CellToWorld(debugCellPosition);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldPos + tilemap.cellSize / 2, 0.1f); // center of the tile
    }
}