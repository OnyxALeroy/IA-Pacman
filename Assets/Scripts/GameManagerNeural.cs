using UnityEngine;

public class GameManagerNeural : GameManager
{
    private new void Start()
    {
        base.Start(); // Call the base class's Start method
    }

    private new void Update()
    {
        base.Update(); // Call the base class's Update method

        // Additional behavior for neural network-based Pacman
        if (this.pacman.GetLives() <= 0 && Input.anyKeyDown)
        {
            NewRound();
        }
    }

    private void NewGame()
    {
        this.pacman.SetScore(0);
        this.pacman.SetLives(1);
        NewRound();
    }

    private void NewRound()
    {
        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }
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

    public new void GhostEaten(Ghost ghost)
    {
        this.pacman.SetScore(this.pacman.GetScore() + (ghost.GetPoints() * ghostMultiplier));
        this.ghostMultiplier++;
    }

    public new void PacmanEaten()
    {
        this.pacman.gameObject.SetActive(false);
        this.pacman.SetLives(this.pacman.GetLives() - 1);
        if (this.pacman.GetLives() <= 0)
        {
            GameOver();
        }
        else
        {
            Invoke(nameof(ResetState), 3);
        }
    }

    public new void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        this.pacman.SetScore(this.pacman.GetScore() + pellet.points);

        if (!HasRemainingPellets())
        {
            this.pacman.gameObject.SetActive(false);
            Invoke(nameof(NewRound), 3.0f);
        }
    }

    public new void PowerPelletEaten(PowerPellet powerPellet)
    {
        // TODO: Ghosts have to flee

        Invoke(nameof(ResetGhostMultiplier), powerPellet.duration);
        CancelInvoke();
        PelletEaten(powerPellet);
    }
}
