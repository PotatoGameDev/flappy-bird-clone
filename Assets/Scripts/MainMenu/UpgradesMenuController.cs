using UnityEngine;
using TMPro;

using UnityEngine.EventSystems;


public class UpgradesMenuController : SecondaryMenuDelegate
{
    [SerializeField] private FakeButton startButton;
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


            upgradePanel.SetSelected(false);
        }
        UpgradesManager.Instance.OnUpgrade -= HandleUpgrade;
    }

    private void HandleUpgrade(Upgrade u)
    {
        FillInStatTexts();
        FillInUpgradeDescriptions();
    }

    private void OnUpgradeSelected(UpgradePanelController selectedUpgrade)
    {

        foreach (UpgradePanelController upgradePanel in upgradePanels)
        {
            if (selectedUpgrade == upgradePanel)
            {
                continue;
            }

            upgradePanel.SetSelected(false);
            //
        }

        FillInUpgradeDescriptions();
    }

    private void FillInUpgradeDescriptions()
    {
        UpgradePanelController selectedUpgrade = GetSelectedUpgrade();

        if (selectedUpgrade != null)
        {
            UpgradeId ident = GetSelectedUpgrade().GetUpgradeIdent();
            Upgrade upgrade = UpgradesManager.Instance.GetUpgrade(ident);

            selectedUpgradeNameLabel.SetText(upgrade.Name);
            selectedUpgradeDescriptionLabel.SetText(
                    UpgradesManager.Instance.GetUpgradeDescription(ident)
                    );
        }
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
        // Update the start button, if the previous level has been completed, then this level can be started
        startButton.gameObject.SetActive(true);
        if (GameManager.Instance.CanPlayLevel())
        {
            startButton.interactable = true;
            startButton.GetComponentInChildren<TextMeshProUGUI>()
                .SetText("Play");
        }
        else
        {
            startButton.interactable = false;
            startButton.GetComponentInChildren<TextMeshProUGUI>()
                .SetText("Locked");
        }
    }

    private UpgradePanelController GetSelectedUpgrade()
    {
        foreach (UpgradePanelController upgradePanel in upgradePanels)
        {
            if (upgradePanel.Selected)
            {
                return upgradePanel;
            }
        }

        // Maybe return the first one?
        return null;
    }
}
