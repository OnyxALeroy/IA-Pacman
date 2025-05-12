using UnityEngine;

public class GhostChase : GhostBehaviour
{

    private void OnEnable()
    {
        // Debug.Log("Chase enabled");
    }

    private void OnDisable()
    {
        this.ghost.scatter.Enable();
        // Debug.Log("Chase disabled");
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();
        
        if (node != null && enabled && !ghost.feared.enabled) {
            Vector2 direction = Vector2.zero;
            float minDistance = float.MaxValue;

            foreach (Vector2 availableDirection in node.availableDirections) {
                Vector3 newPosition = this.transform.position + new Vector3(availableDirection.x, availableDirection.y, 0.0f);
                float distance = (this.ghost.target.position - newPosition).sqrMagnitude;

                if (distance < minDistance) {
                    direction = availableDirection;
                    minDistance = distance;
                }
            }

            this.ghost.movement.SetDirection(direction);
        }
    }
}

