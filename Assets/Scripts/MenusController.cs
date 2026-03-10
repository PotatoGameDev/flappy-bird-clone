using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenusController : MonoBehaviour
{
    void Start()
    {
        currentMainSelection = 1; // We start in level selection

        UpdateAllLabels();
    }

    void OnEnable()
    {
        UpgradesManager.Instance.OnUpgrade += HandleUpgrade;
        GameManager.Instance.OnGameStateChanged += UpdateGameStateRelatedLabels;
    }

    void OnDisable()
    {
        UpgradesManager.Instance.OnUpgrade -= HandleUpgrade;
        GameManager.Instance.OnGameStateChanged -= UpdateGameStateRelatedLabels;
    }

    private void UpdateGameStateRelatedLabels(GameState state)
    {
        UpdateAllLabels();
    }

    private void UpdateAllLabels()
    {
        UpdateMainSelectionMenus();
        UpdateSystemSelectionMenus();
        UpdateLevelSelectionMenus();

        UpdateEnergyLabel();
        UpdatePopulationLabel();
    }

    // Main Selection
    [SerializeField] private TextMeshProUGUI mainSelectionCurrent;
    [SerializeField] private TextMeshProUGUI mainSelectionL;
    [SerializeField] private TextMeshProUGUI mainSelectionR;

    [SerializeField] private GameObject[] mainSelectionContents;

    [SerializeField] private Button[] startButtons;
    [SerializeField] private Button[] advanceButtons;

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
    private string[] levelSelectionStatLabelTemplates;

    private readonly string[,] levelSelectionOptions = {
        { "", "Type I Civilization", "Type II Civilization" },
        { "Type I Civilization", "Type II Civilization", "Type III Civilization" },
        { "Type II Civilization", "Type III Civilization", "" },
    };

    private int CurrentLevelSelection
    {
        get
        {
            return GameManager.Instance.CurrentLevel;
        }
        set
        {
            GameManager.Instance.CurrentLevel = value;
        }
    }

    private void UpdateLevelSelectionMenus()
    {
        levelSelectionL.text = levelSelectionOptions[CurrentLevelSelection, 0];
        levelSelectionCurrent.text = levelSelectionOptions[CurrentLevelSelection, 1];
        levelSelectionR.text = levelSelectionOptions[CurrentLevelSelection, 2];

        for (int i = 0; i < levelSelectionContents.Length; i++)
        {
            levelSelectionContents[i].SetActive(false);
        }
        levelSelectionContents[CurrentLevelSelection].SetActive(true);

        // Filling in the stat texts
        bool levelCompleted = CurrentLevelSelection < GameManager.Instance.CivTypePassed;
        bool previousLevelCompleted = CurrentLevelSelection - 1 < GameManager.Instance.CivTypePassed;

        long startingPopulation = GameManager.Instance.GetBasePopulation();

        long currentEnergy = GameManager.Instance.CollectedEnergy;
        long advanceEnergy = GameManager.Instance.GetAdvanceEnergy();

        if (levelSelectionStatLabelTemplates == null)
        {
            levelSelectionStatLabelTemplates = new string[levelSelectionStatLabels.Length];

            for (int i = 0; i < levelSelectionStatLabels.Length; i++)
            {
                levelSelectionStatLabelTemplates[i] = levelSelectionStatLabels[i].text;
            }
        }

        levelSelectionStatLabels[CurrentLevelSelection].text = string.Format(
                levelSelectionStatLabelTemplates[CurrentLevelSelection],
                previousLevelCompleted ? startingPopulation.ToString() : "??",
                levelCompleted ? "COMPLETED" : "NOT COMPLETED",
                currentEnergy + "GW",
                advanceEnergy + "GW"
        );

        // Update the Advance button:
        bool canAdvance = GameManager.Instance.CanAdvanceLevel();
        foreach (Button advBtn in advanceButtons)
        {
            advBtn.gameObject.SetActive(canAdvance);
        }

        // Update the start button, if the previous level has been completed, then this level can be started
        if (GameManager.Instance.CurrentLevel <= GameManager.Instance.CivTypePassed)
        {
            foreach (Button strtBtn in startButtons)
            {
                strtBtn.interactable = true;
                strtBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Play";
            }
        }
        else
        {
            foreach (Button strtBtn in startButtons)
            {
                strtBtn.interactable = false;
                strtBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Locked";
            }
        }
    }

    public void OnLevelR()
    {
        if (CurrentLevelSelection == levelSelectionContents.Length - 1) return;
        CurrentLevelSelection++;
        UpdateLevelSelectionMenus();
    }

    public void OnLevelL()
    {
        if (CurrentLevelSelection == 0) return;
        CurrentLevelSelection--;
        UpdateLevelSelectionMenus();
    }

    public void OnLevelSelectStartClicked()
    {
        SceneManager.LoadScene("Loading");
    }

    public void OnLevelSelectAdvanceClicked()
    {
        if (CurrentLevelSelection == levelSelectionContents.Length - 1) return;

        GameManager.Instance.UnlockNextPhase();

        UpdateLevelSelectionMenus();
    }

    // Upgrades Selection

    [SerializeField] private TextMeshProUGUI energyLabel;
    private string energyLabelTemplate;

    [SerializeField] private TextMeshProUGUI populationLabel;
    private string populationLabelTemplate;

    private void HandleUpgrade(Upgrade upgrade)
    {
        UpdateEnergyLabel();
        UpdatePopulationLabel();
    }

    private void UpdateEnergyLabel()
    {
        if (energyLabelTemplate == null)
        {
            energyLabelTemplate = energyLabel.text;
        }
        energyLabel.text = string.Format(energyLabelTemplate, GameManager.Instance.CollectedEnergy);
    }

    private void UpdatePopulationLabel()
    {
        if (populationLabelTemplate == null)
        {
            populationLabelTemplate = populationLabel.text;
        }
        populationLabel.text = string.Format(populationLabelTemplate, GameManager.Instance.GetBasePopulation());
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
