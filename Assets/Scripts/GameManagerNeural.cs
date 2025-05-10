using UnityEngine;

public class GameManagerNeural : GameManager
{
    private new void Start()
    {
        Debug.Log("GameManager " + name + " Pacman reference: " + pacman.name);

        base.Start(); // Call the base class's Start method
    }
}
