using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] TilemapDebugger mapDebugger;

    public Ghost[] ghosts;

    public Pacman pacman;

    public Transform pellets;

    public Dictionary<Vector2Int, Transform> pelletCoords { get; private set; } = new Dictionary<Vector2Int, Transform>();
    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; }
    public int lives { get; private set; } = 3;

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
        pelletCoords = new Dictionary<Vector2Int, Transform>();
        Vector2 topLeftTileCoord = new Vector2(mapDebugger.positionInGrid.x, mapDebugger.positionInGrid.y);
        Vector3 tileSize = mapDebugger.tilemap.cellSize;
        foreach(Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);

            // Adding its coords in grid to the dict
            Vector3 pelletCenter = pellet.position;
            Vector2Int pC = new Vector2Int((int)((pelletCenter.y - topLeftTileCoord.y)/tileSize.y), (int)((pelletCenter.x - topLeftTileCoord.x)/tileSize.x));
            pelletCoords[pC] = pellet;
        }
        ResetState();
    }

    private void ResetState()
    {
        ResetGhostMultiplier();
        for(int i = 0; i < ghosts.Length; i++)
        {
            this.ghosts[i].ResetState();
        }
        this.pacman.ResetState();
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
        SetScore(this.score + (ghost.GetPoints()*ghostMultiplier));
        this.ghostMultiplier++;
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

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        SetScore(this.score + pellet.points);

        if(!HasRemainingPellets())
        {
            this.pacman.gameObject.SetActive(false);
            Invoke(nameof(NewRound), 3.0f);
        }
    }

    public void PowerPelletEaten(PowerPellet powerPellet)
    {
        // TODO : Ghosts have to flee

        Invoke(nameof(ResetGhostMultiplier), powerPellet.duration);
        CancelInvoke();
        PelletEaten(powerPellet);        
    }

    private bool HasRemainingPellets()
    {
        foreach(Transform pellet in this.pellets)
        {
            if(pellet.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    private void ResetGhostMultiplier()
    {
        this.ghostMultiplier = 1;
    }
}
