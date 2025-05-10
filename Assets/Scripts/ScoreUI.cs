using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private Pacman[] pacman;
    [SerializeField] private PopulationManager population;
    private TextMeshProUGUI text;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        text.text = "Best Score: " + population.GetBestFitness() + "\n" +
                    "Generation: " + population.gen + "\n";
        for (int i = 0; i < pacman.Length; i++)
        {
            if (pacman[i] != null)
            {
                text.text += "Score " + i + " : " + pacman[i].GetScore() + "\n";
            }
        }
    }
}
