using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradesMenuController : SecondaryMenuDelegate
{
    private UpgradePanelController selectedUpgrade;

    [SerializeField] private Button startButton;
    [SerializeField] private Button advanceButton;


    [SerializeField] private TextMeshProUGUI selectedUpgradeNameLabel;
    [SerializeField] private TextMeshProUGUI selectedUpgradeStatsLabel;
    private string selectedUpgradeStatsLabelTemplate;
    [SerializeField] private TextMeshProUGUI selectedUpgradeDescriptionLabel;

    private UpgradePanelController[] upgradePanels;

    void Awake()
    {
        upgradePanels = GetComponentsInChildren<UpgradePanelController>(true);
        selectedUpgradeStatsLabelTemplate = selectedUpgradeStatsLabel.text;
    }

    void Start()
    {
        UpdateMenu();
        FillInStatTexts();
    }

    void OnEnable()
    {
        foreach (UpgradePanelController upgradePanel in upgradePanels)
        {
            upgradePanel.PanelSelected += OnUpgradeSelected;
        }
        UpgradesManager.Instance.OnUpgrade += HandleUpgrade;
    }

    void OnDisable()
    {
        foreach (UpgradePanelController upgradePanel in upgradePanels)
        {
            upgradePanel.PanelSelected -= OnUpgradeSelected;
        }
        UpgradesManager.Instance.OnUpgrade -= HandleUpgrade;
    }

    private void HandleUpgrade(Upgrade u)
    {
        FillInStatTexts();
    }

    private void OnUpgradeSelected(UpgradePanelController upgradePanel)
    {
        selectedUpgrade = upgradePanel;

        UpgradeId ident = selectedUpgrade.GetUpgradeIdent();
        Upgrade upgrade = UpgradesManager.Instance.GetUpgrade(ident);

        selectedUpgradeNameLabel.SetText(upgrade.Name);
        selectedUpgradeDescriptionLabel.SetText(upgrade.Description);

    }

    public override void ChangeCurrentMenuSelection(int currentSelection)
    {
        selectedUpgrade = null;

        selectedUpgradeNameLabel.SetText("-");
        selectedUpgradeDescriptionLabel.SetText("-");
    }

    // TODO Get rid of from the abstract class
    public override void FillInStatTexts(string labelTemplate, TextMeshProUGUI statLabel)
    {
    }

    private void FillInStatTexts()
    {
        long currentEnergy = GameManager.Instance.CollectedEnergy;
        long currentPopulation = GameManager.Instance.GetBasePopulation();

        // This sets the current stat label based on the format in the label on UI
        // Keep synched with the UI:
        // TODO: Maybe do it so that we do not need to keep it synched?

        selectedUpgradeStatsLabel.SetText(
                selectedUpgradeStatsLabelTemplate,
                currentEnergy,
                currentPopulation
        );
    }

    public override void UpdateMenu()
    {
        // Update the Advance button:
        bool canAdvance = GameManager.Instance.CanAdvanceLevel();

        bool levelCompleted = true;

        advanceButton.gameObject.SetActive(canAdvance || levelCompleted);
        if (levelCompleted)
        {
            TextMeshProUGUI buttonLabel = advanceButton.transform.GetComponentInChildren<TextMeshProUGUI>();
            buttonLabel.SetText("Troll the boss");
        }

        // Update the start button, if the previous level has been completed, then this level can be started
        int currentLevelSelection = GameManager.Instance.CurrentLevel;
        if (currentLevelSelection <= GameManager.Instance.CivTypePassed)
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
}
