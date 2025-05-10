using UnityEngine;
using System.Collections;

public class Pacman : MonoBehaviour {
    [SerializeField]
    float tileSize = 1.0f;
    [SerializeField]
    bool alpha_zero = true;
    [SerializeField]
    MonteCarlo monteCarlo;
    private bool isMoving = false;
    float move_delay = 0.2f;
    float tile_size = 1.0f;

    public Movement movement { get; private set; }

    private void Awake() { this.movement = GetComponent<Movement>(); }

    private void Update() {
        if (isMoving)
            return;
        if (alpha_zero) {
            Vector2 direction = monteCarlo.GetBestDirection(this.transform.position);
            // Debug.Log("AI Direction: " + direction);
            // monteCarlo.print_tree();
            this.movement.SetDirection(direction);
            if (direction != Vector2.zero) {
                StartCoroutine(MoveOneTile(direction));
            }
        } else {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                this.movement.SetDirection(Vector2.up);
            } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                this.movement.SetDirection(Vector2.down);
            } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
                this.movement.SetDirection(Vector2.left);
            } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
                this.movement.SetDirection(Vector2.right);
            }
        }
        // float angle = Mathf.Atan2(-this.movement.direction.x, this.movement.direction.y);
        // this.transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }
    private IEnumerator MoveOneTile(Vector2 direction) {
        isMoving = true;

        Vector3 targetPosition = transform.position + new Vector3(direction.x, direction.y, 0) * tileSize;

        // Optional: Check for collisions here with MovementHelper if needed
        transform.position = targetPosition;

        float angle = Mathf.Atan2(-direction.x, direction.y);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);

        yield return new WaitForSeconds(move_delay);
        isMoving = false;
    }

    public void ResetState() {
        this.movement.ResetState();
        this.gameObject.SetActive(true);
        isMoving = false;
    }
}
