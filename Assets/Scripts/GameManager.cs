using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;

    public Pacman pacman;

    public Transform pellets;

    protected int ghostMultiplier { get; set; } = 1;

    protected void Start()
    { 
        NewGame();
    }

    protected void Update()
    {
        if (pacman.GetLives() <= 0 && Input.anyKeyDown)
        {
            NewRound();
        }
    }

    private void NewGame()
    {
        pacman.SetScore(0);
        pacman.SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }
        this.pacman.SetScore(0);
        ResetState();
    }
    
    private void ResetState()
    {
        ResetGhostMultiplier();
        for (int i = 0; i < ghosts.Length; i++)
        {
            this.ghosts[i].ResetState();
        }
        this.pacman.ResetState();
    }


    private void GameOver()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            this.ghosts[i].gameObject.SetActive(false);
        }
        this.pacman.gameObject.SetActive(false);
    }

    public void GhostEaten(Ghost ghost)
    {
        pacman.SetScore(pacman.GetScore() + (ghost.GetPoints() * ghostMultiplier));
        this.ghostMultiplier++;
    }

    public void PacmanEaten()
    {
        this.pacman.gameObject.SetActive(false);
        pacman.SetLives(pacman.GetLives() - 1);
        if (pacman.GetLives() <= 0)
        {
            GameOver();

        } else {
            this.pacman.SetScore(this.pacman.GetScore()-100);
            Invoke(nameof(ResetState), 3);
        }
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        pacman.SetScore(pacman.GetScore() + pellet.points);

        if (!HasRemainingPellets())
        {
            this.pacman.gameObject.SetActive(false);
            Invoke(nameof(NewRound), 3.0f);
        }
    }

    public void PowerPelletEaten(PowerPellet powerPellet)
    {
        for (int i = 0; i < ghosts.Length; i++) 
        {
            if (ghosts[i].feared == null)
            {
                Debug.LogError($"Ghost {i} does not have a feared component!");
            }
            else
            {
                ghosts[i].feared.Enable(powerPellet.duration);
                Debug.Log($"Ghost {i} is now feared for {powerPellet.duration} seconds.");
            }
        }

        Invoke(nameof(ResetGhostMultiplier), powerPellet.duration);
        CancelInvoke();
        PelletEaten(powerPellet);        
    }

    protected bool HasRemainingPellets()
    {
        foreach (Transform pellet in this.pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    protected void ResetGhostMultiplier()
    {
        this.ghostMultiplier = 1;
    }
}
