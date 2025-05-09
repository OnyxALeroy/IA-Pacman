using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public GameManager game; // Assign in Inspector
    private TextMeshProUGUI text;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (game != null)
        {
            text.text = "Score: " + game.GetScore();
        }
    }
}
