using System.Collections.Generic;
using UnityEngine;

public class Evaluator : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Astar astar;
    [SerializeField] float alpha = 1.0f;
    [SerializeField] float beta = 1.0f;
    [SerializeField] float gamma = 1.0f;
    [SerializeField] float delta = 1.0f;
    [SerializeField] float epsilon = 1.0f;

    // Components ----------------------------------------------------------------------------------------------------------

    private float PelletRemainingEvaluation(GameState gameState){
        // The fewer pellets are left, the better
        return -alpha * gameState.pellets.Count;
    }

    private float DistanceToNearestPelletEvaluation(GameState gameState){
        // We want Pacman to chase food, rather than wander.

        int distance = int.MaxValue;
        foreach (Vector2Int pelletCoord in gameManager.pelletCoords.Keys){
            if (gameManager.pelletCoords[pelletCoord].gameObject.activeSelf){
                astar.setNewDestination(pelletCoord.x, pelletCoord.y);
                if (astar.CurrentPath.Count < distance){ distance = astar.CurrentPath.Count; }
            }
        }
        if (distance == int.MaxValue) { distance = 1; }

        return -beta * 1 / distance;
    }

    private float GhostDangerEvaluation(GameState gameState){
        Vector3 pacmanPosition = gameState.pacman_position;
        float score = 0.0f;

        foreach (_Ghost g in gameState.ghosts){
            if (g.activated){
                float distance = Vector3.Distance(pacmanPosition, g.position);
                if (g.is_ghost_feared){ 
                    score += delta / (1 + distance);
                } else {
                    score -= gamma / (1 + distance);
                }
            }
        }

        return score;
    }

    private float ScoreEvaluation(GameState gameState){
        return epsilon * gameState.score;
    }

    // Evaluation function -------------------------------------------------------------------------------------------------

    public float Evaluate(GameState gameState){
        return PelletRemainingEvaluation(gameState) + DistanceToNearestPelletEvaluation(gameState)
            + GhostDangerEvaluation(gameState) + ScoreEvaluation(gameState);
    }
}
