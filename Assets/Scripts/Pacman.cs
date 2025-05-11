using UnityEngine;

public class Pacman : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    public Movement movement { get; private set; }
    protected int lives = 3;
    protected float score = 0;
    public Transform StartingPosition;
    
    protected void Awake()
    {
        this.movement = GetComponent<Movement>();
        this.circleCollider = GetComponent<CircleCollider2D>(); // Initialize the circleCollider
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.movement.SetDirection(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            this.movement.SetDirection(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.movement.SetDirection(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.movement.SetDirection(Vector2.right);
        }

        float angle = Mathf.Atan2(-this.movement.direction.x, this.movement.direction.y);
        this.transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    public void SetStartingPosition(Transform start)
    {
        this.StartingPosition = start;
    }

    public Transform GetStartingPosition()
    {
        return this.StartingPosition;
    }

    public virtual void ResetState()
    {
        enabled = true;
        spriteRenderer.enabled = true;
        circleCollider.enabled = true;
        movement.ResetState();

        // Reset Pacman's position to the starting point
        if (StartingPosition != null)
        {
            transform.position = StartingPosition.position;
        }

        gameObject.SetActive(true);
    }


    public int GetLives()
    {
        return this.lives;
    }

    public void SetLives(int lives)
    {
        this.lives = lives; 
    }
    public float GetScore()
    {
        return this.score;
    }

    public void SetScore(float score)
    {
        this.score = score; 
    }
}
