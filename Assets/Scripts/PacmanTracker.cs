using UnityEngine;
using UnityEngine.Tilemaps;

public class PacmanCoord : MonoBehaviour
{
    public Transform pacman;
    public Tilemap tilemap;

    void Update()
    {
        Vector3Int pacmanTileCoords = tilemap.WorldToCell(pacman.position);
        Debug.Log("Pac-Man Coords: " + pacmanTileCoords);
    }
}
