using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;

    [SerializeField] public Pacman pacman;

    public Transform pellets;

    protected int ghostMultiplier { get; set; } = 1;

    protected void Start()
    {
        pacman = GetComponentInChildren<Pacman>();
        NewGame();
    }

    // protected void Update()
    // {
    //     if (this.pacman.GetLives() <= 0 && Input.anyKeyDown)
    //     {
    //         NewRound();
    //     }
    // }

    public void NewGame()
    {
        this.pacman.SetScore(0);
        this.pacman.SetLives(3);
        if (this.pacman is PacmanBrain pacmanBrain)
        {
            pacmanBrain.ResetTimer();
        }
        NewRound();
    }

    public void NewRound()
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
        this.pacman.SetScore(this.pacman.GetScore() + (ghost.GetPoints() * ghostMultiplier));
        this.ghostMultiplier++;
    }

    public void PacmanEaten()
    {
        this.pacman.gameObject.SetActive(false);
        this.pacman.SetLives(this.pacman.GetLives() - 1);
        if (this.pacman.GetLives() <= 0)
        {
            GameOver();
        } else {
            this.pacman.SetScore(this.pacman.GetScore()*0.9f);
            Invoke(nameof(ResetState), 3);
        }
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        this.pacman.SetScore(this.pacman.GetScore() + pellet.points);

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
            if (ghosts[i].feared != null)
            {
                ghosts[i].feared.Enable(powerPellet.duration);
            }
        }

        if (pacman is PacmanBrain pacmanBrain)
        {
            pacmanBrain.ActivatePowerPellet(powerPellet.duration);
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
