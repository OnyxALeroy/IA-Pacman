using UnityEngine;
using UnityEngine.Tilemaps;

public class PacmanCoord : MonoBehaviour
{
    public Transform pacman;
    public Tilemap tilemap;

    public Vector3Int GetPacmanCoords(){
        return tilemap.WorldToCell(pacman.position);
    }
}
