using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{

    public TextMeshProUGUI populationLabel;
    public TextMeshProUGUI civilisationTypeLabel;
    public TextMeshProUGUI levelFlavorTextLabel;

    private string[] levelFlavorTexts = {
        "We emerged victorious from all the various wars and cataclisms. From a simple cell to a mighty civilisation ruling the entire planet in peace. No single ruler, no tyrant, just a commonwealth of the people. Now, the goal is clear: To utilize the planet to the fullest, with minimal waste. This can only end well!",
        "TODO Type II",
        "TODO Type III",
    };

    private string[] civTypes = {
        "Civilisation Type I",
        "Civilisation Type II",
        "Civilisation Type III",
    };

    public void StartLevel()
    {
        SceneManager.LoadScene("Loading", LoadSceneMode.Single);
    }

    void Start()
    {
        int population = GameManager.Instance.State.Population;
        int civType = GameManager.Instance.State.CivType;

        populationLabel.text = "Population: " + population;
        civilisationTypeLabel.text = civTypes[civType - 1];
        levelFlavorTextLabel.text = levelFlavorTexts[civType - 1];
    }

}
