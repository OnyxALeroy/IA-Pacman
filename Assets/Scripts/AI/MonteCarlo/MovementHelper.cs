using System.Collections.Generic;
using UnityEngine;

public class MovementHelper {
    public static List<Vector2> get_available_directions(Vector3 position, LayerMask obstacleLayer) {
        List<Vector2> available = new();
        List<Vector2> directions =
            new() { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1) };

        foreach (Vector2 dir in directions) {
            if (!IsOccupied(position, dir, obstacleLayer)) {
                available.Add(dir);
            }
        }

        return available;
    }

    private static bool IsOccupied(Vector2 position, Vector2 direction, LayerMask obstacleLayer) {
        // copy of the function in pacman movement but with df parameters
        RaycastHit2D hit = Physics2D.BoxCast(position, Vector2.one * 0.75f, 0f, direction, 0.5f, obstacleLayer);
        return hit.collider != null;
    }
}
