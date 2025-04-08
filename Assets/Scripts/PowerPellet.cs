using UnityEngine;

public class PowerPellet : Pellet
{
    public float duration = 8f;

    protected override void Eat()
    {
        FindFirstObjectByType<GameManager>().PowerPelletEaten(this);
    }

}