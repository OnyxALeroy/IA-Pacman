using UnityEngine;

public class Ghost : MonoBehaviour
{
    public Movement movement { get; private set; }
    public GhostHome home { get; private set; }
    public GhostChase chase { get; private set; }
    public GhostFeared feared { get; private set; }
    public GhostScatter scatter { get; private set; }
    [SerializeField] public GhostBehaviour initialBehaviour;

    [SerializeField] private GameManager gameManager; // Reference to the GameManager

    public Transform target;

    public int points = 200;

    private void Awake()
    {
        this.chase = GetComponent<GhostChase>();
        this.feared = GetComponent<GhostFeared>();
        this.home = GetComponent<GhostHome>();
        this.scatter = GetComponent<GhostScatter>();
        this.movement = GetComponent<Movement>();
    }

    private void Start()
    {
        ResetState();
    }

    public int GetPoints()
    {
        return points;
    }

    public void ResetState()
    {
        this.gameObject.SetActive(true);
        this.movement.ResetState();

        this.feared.Disable();
        this.chase.Disable();
        this.scatter.Disable();

        if (this.home != this.initialBehaviour)
        {
            this.home.Enable();
        }

        if (this.initialBehaviour != null)
        {
            this.initialBehaviour.Enable();
        }
    }

    public void SetPosition(Vector3 position)
    {
        position.z = transform.position.z;
        transform.position = position;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            if (this.feared.enabled)
            {
                this.gameManager.GhostEaten(this);
            }
            else
            {
                this.gameManager.PacmanEaten();
            }
        }
    }
}
