using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;

    public Pacman pacman;

    public Transform pellets;

    public int score { get; private set; }
    public int lives { get; private set; }

    private void start()
    { 
        NewGame();
    }

    private void Update()
    {
        if(this.lives <= 0 && Input.anyKeyDown)
        {
            NewRound();
        }
    }

    private void NewGame()
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
            this.ghosts[i].gameObject.SetActive(true);
        }
        this.pacman.gameObject.SetActive(true);
    }

    private void SetScore(int score)
    {
        this.score = score;
    }

    private void GameOver()
    {
        for(int i = 0; i < ghosts.Length; i++)
        {
            this.ghosts[i].gameObject.SetActive(false);
        }
        this.pacman.gameObject.SetActive(false);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
    }

    public void GhostEaten(Ghost ghost)
    {
        SetScore(this.score + ghost.GetPoints());
    }

    public void PacmanEaten()
    {
        this.pacman.gameObject.SetActive(false);
        SetLives(lives - 1);
        if(lives <= 0)
        {
            GameOver();
        } else {
            Invoke(nameof(ResetState), 3);
        }
    }
}
