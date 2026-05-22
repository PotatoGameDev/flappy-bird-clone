using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;


public class UpgradesMenuController : SecondaryMenuDelegate
{
    [SerializeField] private FakeButton startButton;
    [SerializeField] private TextMeshProUGUI startButtonLabel;
    [SerializeField] private TextMeshProUGUI startGlyphLabel;

    [SerializeField] private TextMeshProUGUI selectedUpgradeNameLabel;
    [SerializeField] private TextMeshProUGUI selectedUpgradeStatsLabel;
    [SerializeField] private TextMeshProUGUI selectedUpgradeDescriptionLabel;

    [Header("Navigation")]
    [SerializeField] private InputActionReference navigationAction;

    private UpgradePanelController[] upgradePanels;

    void Awake()
    {
        upgradePanels = GetComponentsInChildren<UpgradePanelController>(true);
    }

    void Start()
    {
        UpdateMenu();
        FillInStatTexts();
    }

    void OnEnable()
    {
        UpgradesManager.Instance.OnUpgrade += HandleUpgrade;
        foreach (UpgradePanelController upgradePanel in upgradePanels)
        {
            upgradePanel.PanelSelected += OnUpgradeSelected;
        }

        navigationAction.action.performed += OnNavigate;

        FillInStatTexts();
        FillInUpgradeDescriptions();
    }

    void OnDisable()
    {
        UpgradesManager.Instance.OnUpgrade -= HandleUpgrade;
        foreach (UpgradePanelController upgradePanel in upgradePanels)
        {
            upgradePanel.PanelSelected -= OnUpgradeSelected;
        }
        navigationAction.action.performed -= OnNavigate;
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

            selectedUpgradeNameLabel.SetText(Loc.Get(upgrade.LocKey));
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
                Loc.Get("upgrades_current_stats"),
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
            startButtonLabel.SetText(Loc.Get("common_buttons_play"));
            startGlyphLabel.SetText(Loc.Get("common_glyphs_play"));
        }
        else
        {
            startButton.interactable = false;
            startButtonLabel.SetText(Loc.Get("common_buttons_play_locked"));
            startGlyphLabel.SetText(Loc.Get("common_glyphs_play_locked"));
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

    void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (!ctx.action.WasPressedThisFrame())
        {
            return;
        }

        Vector2 value = ctx.ReadValue<Vector2>();
        if (value != Vector2.zero)
        {
            if (value.y > 0)
            {
                GetSelectedUpgrade().NavigateUp();
            }
            if (value.y < 0)
            {
                GetSelectedUpgrade().NavigateDown();
            }
        }
    }
}
