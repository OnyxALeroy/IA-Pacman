using UnityEngine;
using System.Collections;

public class Pacman : MonoBehaviour {
  [SerializeField] float tileSize = 1.0f;
  [SerializeField] bool alpha_zero = true;
  [SerializeField] bool comportemental_ai = false;
  [SerializeField] MonteCarlo monteCarlo;
  [SerializeField] ComportementalAI comportementalAI;
  private bool isMoving = false;
  float move_delay = 0.2f;
  float tile_size = 1.0f;

  [SerializeField] private SpriteRenderer spriteRenderer;
  private CircleCollider2D circleCollider;
  public Movement movement { get; private set; }

  public Transform Start; // Reference to Pacman's starting position

  private void Awake() {
    this.movement = GetComponent<Movement>();
    this.circleCollider =
        GetComponent<CircleCollider2D>(); // Initialize the circleCollider
  }

  private void Update() {
    if (isMoving)
      return;
    if (alpha_zero) {
      Vector2 direction = monteCarlo.GetBestDirection(this.transform.position);
      // monteCarlo.print_tree();
      this.movement.SetDirection(direction);
      Debug.Log("AI Direction: " + direction);
      if (direction != Vector2.zero) {
        StartCoroutine(MoveOneTile(direction));
      }
    } else if (comportemental_ai) {
      comportementalAI.HandleUpdate();
    } else {
      if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
        this.movement.SetDirection(Vector2.up);
      } else if (Input.GetKeyDown(KeyCode.S) ||
                 Input.GetKeyDown(KeyCode.DownArrow)) {
        this.movement.SetDirection(Vector2.down);
      } else if (Input.GetKeyDown(KeyCode.A) ||
                 Input.GetKeyDown(KeyCode.LeftArrow)) {
        this.movement.SetDirection(Vector2.left);
      } else if (Input.GetKeyDown(KeyCode.D) ||
                 Input.GetKeyDown(KeyCode.RightArrow)) {
        this.movement.SetDirection(Vector2.right);
      }
    }
    // float angle = Mathf.Atan2(-this.movement.direction.x,
    // this.movement.direction.y); this.transform.rotation =
    // Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
  }
  private IEnumerator MoveOneTile(Vector2 direction) {
    isMoving = true;

    Vector3 targetPosition =
        transform.position +
        new Vector3(direction.x, direction.y, 0) * tileSize;

    // Optional: Check for collisions here with MovementHelper if needed
    transform.position = targetPosition;

    float angle = Mathf.Atan2(-direction.x, direction.y);
    transform.rotation =
        Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);

    yield return new WaitForSeconds(move_delay);
    isMoving = false;
  }

  public void ResetState() {
    enabled = true;
    spriteRenderer.enabled = true;
    circleCollider.enabled = true;
    movement.ResetState();

    // Reset Pacman's position to the starting point
    if (Start != null) {
      transform.position = Start.position;
    }

    gameObject.SetActive(true);
    Debug.Log("Pacman Reset State");
    isMoving = false;
  }
}
