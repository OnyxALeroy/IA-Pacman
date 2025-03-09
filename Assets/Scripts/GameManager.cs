using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;

    public Pacman pacman

    public Transform pellets;

    public int score { get; private set; }
    public int lives { get; private set; }

    private void start()
    { 
        NewGame(;)
    }

    privatee void NewGame()
    {
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        foreach(Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }
        ResetState();
    }
    
    private void ResetState()
    {
        for(int i = 0; i < ghosts.Length; i++)
        {
            this.ghosts[i].gameObject.setActive(true);
        }
        this.pacman.gameObject.setActive(true);
    }

    private SetScore(int score)
    {
        this.score = score;
    }

    private void GameOver()
    {
                for(int i = 0; i < ghosts.Length; i++)
        {
            this.ghosts[i].gameObject.setActive(false);
        }
        this.pacman.gameObject.setActive(false);
    }

    private SetLives(int lives)
    {
        this.lives = lives;
    }

    public void GhostEaten(Ghost ghost)
    {
        SetScore(this.score + ghost.GetPoints());
    }

    public void PacmanEaten()
    {
        this.pacman.gameObject.setActive(false);
        SetLives(lives - 1);
        if(lives <= 0)
        {
            GameOver();
        } else {
            Invoke(nameof(ResetState()), 3);
        }
    }
}
