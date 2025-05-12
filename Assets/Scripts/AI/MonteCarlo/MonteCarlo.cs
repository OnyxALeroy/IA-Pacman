using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonteCarlo : MonoBehaviour {
    [SerializeField]
    Pacman pacman;
    [SerializeField]
    GameManager game_manager;
    [SerializeField]
    Evaluator evaluator;

    private const int iterations_nb = 100;
    private const int simulation_depth = 10;
    MonteCarloNode root;
    private MonteCarloNode current_node;
    private float timer = 0f;

    // TODO: respawn ghosts if they are dead
    void Start() { InitializeRoot(pacman.transform.position); }
    void Update() {
        foreach (_Ghost ghost in root.game_state.ghosts) {
            ghost.update_timer(Time.deltaTime);
        }
    }

    public Vector2 run_simulation() {
        for (int i = 0; i < iterations_nb; i++) {
            MonteCarloNode selected_node = select(root);
            float reward = simulate(selected_node);
            backpropagate(selected_node, reward);
        }
        return select_best_move(root);
    }

    // TODO: manage ghosts after they are deactivated so i do not get a null pointer reference

    private MonteCarloNode select(MonteCarloNode node) {
        while (!is_terminal(node)) {
            if (!is_complete(node)) {
                return expand_node(node);
            } else {
                node = node.selectChild();
            }
        }
        return node;
    }
    private MonteCarloNode expand_node(MonteCarloNode node) {
        List<Vector2> valid_moves =
            MovementHelper.get_available_directions(node.game_state.pacman_position, node.game_state.obstacle_layer);
        List<Vector2> unexplored_moves = valid_moves.Where(move => !node.children.ContainsKey(move)).ToList();
        Vector2 move = unexplored_moves[UnityEngine.Random.Range(0, unexplored_moves.Count)];
        Vector3 new_position = node.game_state.pacman_position + new Vector3(move.x, move.y, 0);
        GameState new_game_state = node.game_state.clone();
        new_game_state.pacman_position = new_position;
        // TODO: vérifier si le pacman ne mange pas de pellets
        MonteCarloNode new_node = new MonteCarloNode(move, node, 0.0f, new_game_state);
        move_ghosts(new_game_state);
        check_pellet_position(new_game_state);
        node.children[move] = new_node;
        return new_node;
    }
    // TODO: gérér ce qui se passe lorsque le pacman mange une capsule

    private bool is_complete(MonteCarloNode node) {
        List<Vector2> valid_moves =
            MovementHelper.get_available_directions(node.game_state.pacman_position, node.game_state.obstacle_layer);
        foreach (Vector2 move in valid_moves) {
            if (!node.children.ContainsKey(move)) {
                return false;
            }
        }
        return true;
    }
    private bool is_terminal(MonteCarloNode node) {
        // for now a node is terminal if we do not have directions to move to
        List<Vector2> valid_moves =
            MovementHelper.get_available_directions(node.game_state.pacman_position, node.game_state.obstacle_layer);
        return valid_moves.Count == 0;
    }

    private void backpropagate(MonteCarloNode node, float reward) {
        MonteCarloNode current_node = node;
        while (current_node != null) {
            current_node.visit_count++;
            current_node.average_reward += (reward - current_node.average_reward) / current_node.visit_count;
            current_node = current_node.parent;
        }
    }

    private float simulate(MonteCarloNode node) {
        // from the expanded node we simulate a full game using random moves until a terminal condition is met ex: game
        // over or we reach max simulation depth
        // evaluate the result after this simulation and we return it at the end
        GameState game_state = node.game_state.clone();
        Vector3 current_position = game_state.pacman_position;
        float reward = 0f;
        for (int i = 0; i < simulation_depth; i++) {
            if (is_game_over(game_state)) {
                return -100f;
            }
            List<Vector2> valid_moves =
                MovementHelper.get_available_directions(current_position, game_state.obstacle_layer);
            if (valid_moves.Count == 0) {
                break;
            }
            Vector2 random_move = valid_moves[Random.Range(0, valid_moves.Count)];
            current_position += new Vector3(random_move.x, random_move.y, 0);
            game_state.pacman_position = current_position;
            move_ghosts(game_state);
            check_pellet_position(game_state);
            reward += evaluator.Evaluate(game_state);
        }
        return reward;
    }
    private void check_pellet_position(GameState game_state) {
        Vector3Int pacman_position_int = Vector3Int.RoundToInt(game_state.pacman_position);
        Dictionary<Vector3Int, _Pellet> pellets = game_state.pellets;

        if (pellets.ContainsKey(pacman_position_int) && pellets[pacman_position_int].active) {
            pellets[pacman_position_int].eat();
            game_state.score += pellets[pacman_position_int].points;
			if (pellets[pacman_position_int].is_power_pellet){
				game_state.is_frightened = true;
				// TODO: à commencer à tiquer le timer
				// et à remettre l'état à false après un certain temps
			}
        }
    }
    private void move_ghosts(GameState game_state) {
        foreach (var ghost in game_state.ghosts) {
            if (ghost.activated) {
                List<Vector2> available_ghost_directions =
                    MovementHelper.get_available_directions(ghost.position, game_state.obstacle_layer);
                if (available_ghost_directions.Count == 0) {
                    return;
                }
                Vector3 random_dir = available_ghost_directions[Random.Range(0, available_ghost_directions.Count)];
                Vector3 new_ghost_position = ghost.position + new Vector3(random_dir.x, random_dir.y, 0);
                Vector2 new_ghost_position_coord = ghost.coord + new Vector2(random_dir.x, random_dir.y);
                ghost.position = new_ghost_position;
                ghost.coord = new_ghost_position_coord;
            }
        }
    }

    private Vector2 select_best_move(MonteCarloNode root) {
        Vector2 best_move = Vector2.zero;
        float max_reward = float.MinValue;

        foreach (var el in root.children) {
            Vector2 direction = el.Key;
            MonteCarloNode child = el.Value;
            if (child.average_reward > max_reward) {
                max_reward = child.average_reward;
                best_move = direction;
            }
        }
        return best_move;
    }
    private bool is_game_over(GameState gameState) {
        foreach (var ghost in gameState.ghosts) {
            if (ghost.activated && Vector3.Distance(gameState.pacman_position, ghost.position) < 0.5f) {
                return true;
            }
        }
        // TODO: idem si tous les points sont collectés
        return false;
    }
    public Vector2 GetBestDirection(Vector3 currentPosition) {
        InitializeRoot(currentPosition);
        return run_simulation();
    }

    public void InitializeRoot(Vector3 pacmanPosition) {
        Ghost[] ghosts = game_manager.ghosts;
        List<_Ghost> _ghosts = new List<_Ghost>();
        for (int i = 0; i < ghosts.Length; i++) {
            Vector3 ghost_position;
            if (ghosts[i] != null && ghosts[i].gameObject.activeInHierarchy) {
                ghost_position = ghosts[i].transform.position;
            } else {
                ghost_position = Vector3.negativeInfinity;
            }

            _Ghost _ghost = new _Ghost(ghost_position, i);
            _ghosts.Add(_ghost);
        }
        Transform pellets = game_manager.pellets;
        Dictionary<Vector3Int, _Pellet> _pellets = new Dictionary<Vector3Int, _Pellet>();
        foreach (Transform pellet in pellets) {
            Vector3Int key = Vector3Int.RoundToInt(pellet.position);
            bool is_power_pellet;
            Debug.Log(pellet.GetComponent<Pellet>());
            if (pellet.GetComponent<Pellet>() is PowerPellet) {
                is_power_pellet = true;
            } else {
                is_power_pellet = false;
            }
            _pellets[key] = new _Pellet(pellet.position, pellet.gameObject.activeInHierarchy, is_power_pellet);
        }

        GameState game_state = new GameState(pacmanPosition, pacman.movement.obstacleLayer, _ghosts, _pellets, 0);
        this.root = new MonteCarloNode(Vector2.negativeInfinity, null, 0.0f, game_state);
        current_node = root;
    }

    public void print_tree() {
        Debug.Log("=== MONTE CARLO TREE STRUCTURE ===");
        PrintNodeRecursive(root, 0, "ROOT");
        Debug.Log("=== END OF TREE STRUCTURE ===");
    }
    private void PrintNodeRecursive(MonteCarloNode node, int depth, string moveLabel) {
        string indent = new string(' ', depth * 4);

        string parentMoveLabel = node.parent != null ? GetDirectionLabel(node.parent.previous_move) : "None";
        Debug.Log(
            $"{indent}├─ {moveLabel}: [Parent Move: {parentMoveLabel}, Visits: {node.visit_count}, Reward: {node.average_reward:F2}, " +
            $"Position: {node.game_state.pacman_position}] ");

        int totalChildren = node.children.Count;
        if (totalChildren > 0) {
            Debug.Log($"{indent}│  ({totalChildren} children)");

            int childCount = 0;
            foreach (var childEntry in node.children) {
                childCount++;
                Vector2 move = childEntry.Key;
                MonteCarloNode childNode = childEntry.Value;

                // Limit depth to avoid massive output
                if (depth < 4) // Adjust this value based on how deep you want to go
                {
                    // Create a label for this move
                    string directionLabel = GetDirectionLabel(move);
                    PrintNodeRecursive(childNode, depth + 1, directionLabel);
                } else if (depth == 4 && childCount == 1) {
                    // Just indicate there are more nodes but don't print them
                    Debug.Log($"{indent}    └─ ... ({totalChildren} more nodes at depth {depth+1})");
                    break;
                }
            }
        } else {
            Debug.Log($"{indent}│  (Leaf node)");
        }
    }
    private string GetDirectionLabel(Vector2? move) {
        if (!move.HasValue)
            return "NULL";

        if (move.Value == Vector2.up)
            return "UP";
        if (move.Value == Vector2.down)
            return "DOWN";
        if (move.Value == Vector2.left)
            return "LEFT";
        if (move.Value == Vector2.right)
            return "RIGHT";
        if (move.Value == Vector2.negativeInfinity)
            return "START";

        return $"Move({move.Value.x},{move.Value.y})";
    }
}
