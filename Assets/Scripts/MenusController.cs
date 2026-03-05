using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenusController : MonoBehaviour
{
    void Start()
    {
        currentMainSelection = 1; // We start in level selection
        UpdateMainSelectionMenus();
        UpdateSystemSelectionMenus();
        UpdateLevelSelectionMenus();

        UpdateEnergyLabel();
    }

    void OnEnable()
    {
        UpgradesManager.Instance.OnUpgrade += HandleUpgrade;
    }

    void OnDisable()
    {
        UpgradesManager.Instance.OnUpgrade -= HandleUpgrade;
    }

    // Main Selection
    [SerializeField] private TextMeshProUGUI mainSelectionCurrent;
    [SerializeField] private TextMeshProUGUI mainSelectionL;
    [SerializeField] private TextMeshProUGUI mainSelectionR;

    [SerializeField] private GameObject[] mainSelectionContents;

    [SerializeField] private Button startButton;

    private readonly string[,] mainSelectionOptions = {
        { "", "System", "Level Select" },
        { "System", "Level Select", "Upgrades" },
        { "Level Select", "Upgrades", "" },
    };

    private int currentMainSelection = 0;

    private void UpdateMainSelectionMenus()
    {
        mainSelectionL.text = mainSelectionOptions[currentMainSelection, 0];
        mainSelectionCurrent.text = mainSelectionOptions[currentMainSelection, 1];
        mainSelectionR.text = mainSelectionOptions[currentMainSelection, 2];

        for (int i = 0; i < mainSelectionContents.Length; i++)
        {
            mainSelectionContents[i].SetActive(false);
        }
        mainSelectionContents[currentMainSelection].SetActive(true);

    }

    public void OnMainR()
    {
        if (currentMainSelection >= mainSelectionOptions.GetLength(0) - 1) return;
        currentMainSelection++;
        UpdateMainSelectionMenus();
    }

    public void OnMainL()
    {
        if (currentMainSelection == 0) return;
        currentMainSelection--;
        UpdateMainSelectionMenus();
    }

    // Level Selection
    [SerializeField] private TextMeshProUGUI levelSelectionCurrent;
    [SerializeField] private TextMeshProUGUI levelSelectionL;
    [SerializeField] private TextMeshProUGUI levelSelectionR;

    [SerializeField] private GameObject[] levelSelectionContents;
    [SerializeField] private TextMeshProUGUI[] levelSelectionStatLabels;

    private string[] levelFlavorTexts = {
        "We emerged victorious from all the various wars and cataclisms. From a simple cell to a mighty civilization ruling the entire planet in peace. No single ruler, no tyrant, just a commonwealth of the people. Now, the goal is clear: To utilize the planet to the fullest, with minimal waste. This can only end well!",
        "TODO Type II",
        "TODO Type III",
    };

    private readonly string[,] levelSelectionOptions = {
        { "", "Type I Civilization", "Type II Civilization" },
        { "Type I Civilization", "Type II Civilization", "Type III Civilization" },
        { "Type II Civilization", "Type III Civilization", "" },
    };

    private int currentLevelSelection = 0;

    private void UpdateLevelSelectionMenus()
    {
        levelSelectionL.text = levelSelectionOptions[currentLevelSelection, 0];
        levelSelectionCurrent.text = levelSelectionOptions[currentLevelSelection, 1];
        levelSelectionR.text = levelSelectionOptions[currentLevelSelection, 2];

        for (int i = 0; i < levelSelectionContents.Length; i++)
        {
            levelSelectionContents[i].SetActive(false);
        }
        levelSelectionContents[currentLevelSelection].SetActive(true);

        // Filling in the stat texts
        bool levelCompleted = currentLevelSelection < GameManager.Instance.State.CivTypePassed;
        bool previousLevelCompleted = currentLevelSelection - 1 < GameManager.Instance.State.CivTypePassed;

        long startingPopulation = GameManager.Instance.State.GetStartingPopulation(currentLevelSelection);
        long survivingPopulation = GameManager.Instance.State.GetSurvivingPopulation(currentLevelSelection);

        long currentEnergy = GameManager.Instance.State.CollectedEnergy;

        levelSelectionStatLabels[currentLevelSelection].text = string.Format(
                levelSelectionStatLabels[currentLevelSelection].text,
                previousLevelCompleted ? startingPopulation.ToString() : "??",
                levelCompleted ? survivingPopulation.ToString() : "NOT COMPLETED",
                currentEnergy + "GW"
                );

        // Update the start button, if the previous level has been completed, then this level can be started
        if (previousLevelCompleted)
        {
            startButton.interactable = true;
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Play";
        }
        else
        {
            startButton.interactable = false;
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Locked";
        }

    }

    public void OnLevelR()
    {
        if (currentLevelSelection == levelSelectionContents.Length) return;
        currentLevelSelection++;
        UpdateLevelSelectionMenus();
    }

    public void OnLevelL()
    {
        if (currentLevelSelection == 0) return;
        currentLevelSelection--;
        UpdateLevelSelectionMenus();
    }

    public void OnLevelSelectStartClicked()
    {
        SceneManager.LoadScene("Loading");
    }

    // Upgrades Selection

    [SerializeField] private TextMeshProUGUI energyLabel;
    private string energyLabelTemplate;

    private void HandleUpgrade(Upgrade upgrade)
    {
        UpdateEnergyLabel();
    }

    private void UpdateEnergyLabel()
    {
        if (energyLabelTemplate == null)
        {
            energyLabelTemplate = energyLabel.text;
        }
        energyLabel.text = string.Format(energyLabelTemplate, GameManager.Instance.State.CollectedEnergy);
    }

    // System Selection

    [SerializeField] private TextMeshProUGUI systemSelectionCurrent;
    [SerializeField] private TextMeshProUGUI systemSelectionL;
    [SerializeField] private TextMeshProUGUI systemSelectionR;

    [SerializeField] private GameObject[] systemSelectionContents;

    private readonly string[,] systemSelectionOptions = {
        { "", "Main", "Settings" },
        { "Main", "Settings", "Highscores" },
        { "Settings", "Highscores", "" },
    };

    private int currentSystemSelection = 0;

    private void UpdateSystemSelectionMenus()
    {
        systemSelectionL.text = systemSelectionOptions[currentSystemSelection, 0];
        systemSelectionCurrent.text = systemSelectionOptions[currentSystemSelection, 1];
        systemSelectionR.text = systemSelectionOptions[currentSystemSelection, 2];

        for (int i = 0; i < systemSelectionContents.Length; i++)
        {
            systemSelectionContents[i].SetActive(false);
        }
        systemSelectionContents[currentSystemSelection].SetActive(true);
    }

    public void OnSystemR()
    {
        if (currentSystemSelection == systemSelectionContents.Length) return;
        currentSystemSelection++;
        UpdateSystemSelectionMenus();
    }

    public void OnSystemL()
    {
        if (currentSystemSelection == 0) return;
        currentSystemSelection--;
        UpdateSystemSelectionMenus();
    }

}
