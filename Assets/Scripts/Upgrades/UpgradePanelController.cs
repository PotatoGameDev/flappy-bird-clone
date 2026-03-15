using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradePanelController : MonoBehaviour
{
    [SerializeField] private UpgradeId upgradeIdent;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI levelLabel;

    void Awake()
    {
        nameLabel.text = UpgradesManager.Instance.GetUpgrade(upgradeIdent).Name;
        UpdateUi();
    }

    void OnEnable() => UpgradesManager.Instance.OnUpgrade += HandleUpgrade;
    void OnDisable() => UpgradesManager.Instance.OnUpgrade -= HandleUpgrade;

    public void OnUpgradeClicked()
    {
        UpgradesManager.Instance.Buy(upgradeIdent);
        UpdateUi();
        // TODO Play satisfying sound
    }

    private void OnValidate()
    {
        if (nameLabel != null) nameLabel.text = upgradeIdent.ToString();
    }

    void UpdateUi()
    {
        Upgrade u = UpgradesManager.Instance.GetUpgrade(upgradeIdent);

        // Unlock and rename the button
        TextMeshProUGUI buttonLabel = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();

        // TODO Introduce per-upgrade, level varying price balance
        int price = u.GetTotalPrice();

        if (u.Level == u.MaxLevel)
        {
            buttonLabel.text = "Maxed";
            upgradeButton.interactable = false;
        }
        else if (u.Level == 0)
        {
            buttonLabel.text = string.Format("Buy ({0}GW)", price);
            upgradeButton.interactable = true;
        }
        else
        {
            buttonLabel.text = string.Format("Upgrade ({0}GW)", price);
            upgradeButton.interactable = true;
        }

        if (price > GameManager.Instance.CollectedEnergy)
            upgradeButton.interactable = false;

        // Update the level label
        levelLabel.text = u.Level.ToString();

        // TODO: remove when upgrades are ready:
        if (u.Ident == UpgradeId.AccretioSuction || u.Ident == UpgradeId.StarLifting || u.Ident == UpgradeId.ToorboBoost || u.Ident == UpgradeId.EnergyShield)
        {
            upgradeButton.interactable = false;
            buttonLabel.text = "UNAVAILABLE";
        }
    }

    void HandleUpgrade(Upgrade upgrade)
    {
        UpdateUi();
    }
}
