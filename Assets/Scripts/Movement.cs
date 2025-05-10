using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class Movement : MonoBehaviour
{
    public float speed = 8.0f;
    public float speedMultiplier = 1.0f;

    public Vector2 initialDirection = Vector2.zero;
    public LayerMask obstacleLayer;

    public Rigidbody2D rb {get; private set;}
    public Vector2 direction { get; private set; }
    public Vector2 nextDirection { get; private set; }
    public Vector3 startPosition { get; private set; }

    public void Awake()
    {
        this.rb = GetComponent<Rigidbody2D>();
        this.startPosition = this.transform.position;
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        this.speedMultiplier = 1.0f;
        this.direction = this.initialDirection;
        this.nextDirection = Vector2.zero;
        this.transform.position = this.startPosition;
        this.rb.bodyType = RigidbodyType2D.Dynamic;
        this.enabled = true;
    }

    public void Update()
    {
        if(this.nextDirection != Vector2.zero)
        {
            SetDirection(this.nextDirection);
        }
    }

    private void FixedUpdate()
    {
        Vector2 position = this.rb.position;
        Vector2 translation = this.direction * this.speed * this.speedMultiplier * Time.fixedDeltaTime;
        this.rb.MovePosition(position + translation);
    }

    public void SetDirection(Vector2 newDirection, bool forced = false)
    {
        if (forced || !Occupied(newDirection))
        {
            this.direction = newDirection;
            this.nextDirection = Vector2.zero;
        }
        else
        {
            this.nextDirection = newDirection;
        }
    }

    public bool Occupied(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.75f, 0f, direction, 0.5f, obstacleLayer);
        return hit.collider != null;
    }
}
