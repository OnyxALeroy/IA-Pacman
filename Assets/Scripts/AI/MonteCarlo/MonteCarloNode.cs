using System.Collections.Generic;
using UnityEngine;

public class _Pellet {
    public Vector3 position;
    public bool active;
    public int points = 10;
    public bool is_power_pellet;
	public Vector2Int coord;

    public _Pellet(Vector3 position, bool active, bool is_power_pellet) {
        this.position = position;
        this.active = active;
        this.is_power_pellet = is_power_pellet;
    }
    public bool is_near(Vector3 pacman_position) {
        float distance = 0.5f;
        return active && Vector3.Distance(pacman_position, position) < distance;
    }
    public void eat() { active = false; }
}

public class _Ghost {
    public Vector3 position { get; set; }
    public bool activated { get; set; }
    public float duration { get; set; }
    public float timer;
    public Vector2 coord;

    // à changer s'il faut
    private Vector2 grid_center = new Vector2(13, -7);

    public _Ghost(Vector3 position, int number) {
        this.position = position;
        this.timer = 0f;
        switch (number) {
            case 0:
                this.duration = 0.0f;
                this.activated = true;
                break;
            case 1:
                this.duration = 15.0f;
                break;
            case 2:
                this.duration = 20.0f;
                break;
            case 3:
                this.duration = 25.0f;
                break;
            default:
                this.duration = 0;
                break;
        }
    }
    public void update_timer(float delta_time) {
        if (!activated) {
            timer += delta_time;
            if (timer >= duration) {
                activated = true;
            }
        }
    }
}

public class GameState {
    public Vector3 pacman_position { get; set; }
	public float fright_timer;
    public LayerMask obstacle_layer { get; set; }
    public List<_Ghost> ghosts { get; set; }
    public Dictionary<Vector2Int, _Pellet> pellets = new Dictionary<Vector2Int, _Pellet>();
    public int score;
	public bool is_frightened;

    public GameState(Vector3 pacman_position, LayerMask obstacle_layer, List<_Ghost> ghosts,
                     Dictionary<Vector2Int, _Pellet> pellets, int score) {
        this.pacman_position = pacman_position;
        this.obstacle_layer = obstacle_layer;
        this.ghosts = ghosts;
        this.pellets = pellets;
        this.score = score;
		this.fright_timer = 0f;
		this.fright_timer = 0f;
		this.is_frightened = false;
    }

    public GameState clone() { return new GameState(pacman_position, obstacle_layer, ghosts, pellets, score); }
}
// ----------------------------------------------------------------------------

public class MonteCarloNode {
    public GameState game_state;
    public MonteCarloNode parent;
    public Dictionary<Vector2, MonteCarloNode> children;

    public int visit_count;
    public Vector2? previous_move;
    public float average_reward;
    private const float c = 1.4f;

    public MonteCarloNode(Vector2 previous_move, MonteCarloNode parent, float average_reward, GameState game_state) {
        this.game_state = game_state;
        this.parent = parent;
        this.previous_move = previous_move;
        this.average_reward = average_reward;
        this.visit_count = 0;
        this.children = new Dictionary<Vector2, MonteCarloNode>();
    }
    public float calculate_ucb() {
        if (visit_count == 0)
            // force exploration for unvisited nodes
            return float.MaxValue;
        float Q = average_reward;
        float n = visit_count;
        float N = parent.visit_count;
        float P = network_probability();
        float exploration_term = c * P * Mathf.Sqrt(N) / (1 + n);
        float score = Q + exploration_term;
        return score;
    }
    public float network_probability() {
        // à remplacer par le reseau de neurones
        return 0.0f;
    }

    public MonteCarloNode selectChild() {
        // selects best child based on its value of ucb
        float max_ucb = float.MinValue;
        MonteCarloNode selected_node = null;
        foreach (var child in children.Values) {
            float ucb_value = child.calculate_ucb();
            if (ucb_value > max_ucb) {
                max_ucb = ucb_value;
                selected_node = child;
            }
        }
        return selected_node;
    }
    public void print_node() {
        string moveStr = previous_move.HasValue ? previous_move.Value.ToString() : "None";
        Debug.Log(
            "================== Node ==================\n" + $"Pacman position: {game_state.pacman_position}\n" +
            $"GameState:\n  Ghost1: {game_state.ghosts[0].position}, {game_state.ghosts[0].duration}\n  Ghost2: {game_state.ghosts[1].position}, {game_state.ghosts[1].duration}\n" +
            $"  Ghost3: {game_state.ghosts[2].position}, {game_state.ghosts[2].duration}\n  Ghost4: {game_state.ghosts[3].position}, {game_state.ghosts[3].duration}\n" +
            $"Previous Move: {moveStr}\n" + $"Average Reward: {average_reward}\n" + $"Visit Count: {visit_count}\n" +
            "==========================================");
    }
}
