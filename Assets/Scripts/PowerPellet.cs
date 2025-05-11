using UnityEngine;

public class PowerPellet : Pellet
{
    public float duration = 8f;

    protected virtual void Eat()
    {
        GetComponentInParent<GameManager>().PowerPelletEaten(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Pacman pacman = other.GetComponent<Pacman>();
        if (other.gameObject.layer == LayerMask.NameToLayer("Pacman")) {
            Eat();
        }
    }

}