using UnityEngine;

public class Evaluation : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] float alpha = 0.0f;
    [SerializeField] float beta = 0.0f;
    [SerializeField] float gamma_g = 0.0f;
    [SerializeField] float delta = 0.0f;
    [SerializeField] float epsilon = 0.0f;
    [SerializeField] float omega = 0.0f;

    // Components

    private float PelletRemainingEvaluation(){
        // The fewer pellets are left, the better
        int pelletCount = 0;
        foreach(Transform pellet in gameManager.pellets)
        {
            if(pellet.gameObject.activeSelf) { pelletCount++; }
        }

        return -alpha * pelletCount;
    }

    private float DistanceToNearestPelletEvaluation(){
        // We want Pacman to chase food, rather than wander.

        int distance = -1; // Distance to the closest pellet. 

        Transform pacman = gameManager.pacman.transform;


        return -beta * 1 / distance;
    }
    private float ActiveGhostDangerEvaluation(){ return 0.0f; }
    private float ScaredGhostRewardEvaluation(){ return 0.0f; }
    private float CapsuleRemainingEvaluation(){ return 0.0f; }
    private float ScoreEvaluation(){ return 0.0f; }

    // Evaluation function

    public float Evaluate(){
        return PelletRemainingEvaluation() + DistanceToNearestPelletEvaluation() + ActiveGhostDangerEvaluation()
            + ScaredGhostRewardEvaluation() + CapsuleRemainingEvaluation() + ScoreEvaluation();
    }
}
