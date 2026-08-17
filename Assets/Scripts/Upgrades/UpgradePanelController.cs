using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public class UpgradePanelController : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private UpgradeId upgradeIdent;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private GameObject borderImage;
    [SerializeField] private GameObject borderImageDisabled;
    [SerializeField] private MenuSoundManager soundManager;
    [SerializeField] private bool defaultOnEnable;

    [Header("Navigation")]
    [SerializeField] private UpgradePanelController navigateUp;
    [SerializeField] private UpgradePanelController navigateDown;

    public event Action<UpgradePanelController> PanelSelected;

    public bool Selected
    {
        get; private set;
    }

    void Start()
    {
        nameLabel.SetText(Loc.Get(Upgrade.GetLocKey(upgradeIdent)));
        UpdateUi();
        UpgradesManager.Instance.OnUpgrade += HandleUpgrade;
    }

    void OnEnable()
    {
        nameLabel.SetText(Loc.Get(Upgrade.GetLocKey(upgradeIdent)));
        UpdateUi();
        if (defaultOnEnable && !Selected)
        {
            SetSelected(true);
        }
    }

    void OnDisable()
    {
        SetSelected(false);
    }

    public void NavigateUp()
    {
        navigateUp.SetSelected(true);
    }

    public void NavigateDown()
    {
        navigateDown.SetSelected(true);
    }

    void OnDestroy()
    {
        UpgradesManager.Instance.OnUpgrade -= HandleUpgrade;
    }

    void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (!Selected)
        {
            return;
        }

        if (!ctx.action.WasPressedThisFrame())
        {
            return;
        }

        Vector2 value = ctx.ReadValue<Vector2>();
        if (value != Vector2.zero)
        {
            if (value.y > 0)
            {
                navigateUp.SetSelected(true);
            }
            if (value.y < 0)
            {
                navigateDown.SetSelected(true);
            }
        }
    }

    public void OnUpgradeClicked()
    {
        UpgradesManager.Instance.Buy(upgradeIdent);
        UpdateUi();
        soundManager.PlayUpgrade();
    }

    public UpgradeId GetUpgradeIdent()
    {
        return upgradeIdent;
    }

    private void OnValidate()
    {
        if (nameLabel != null)
        {
            nameLabel.text = upgradeIdent.ToString();
        }
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
            buttonLabel.SetText(Loc.Get("upgrades_panel_button_maxed_out"));
            upgradeButton.interactable = false;
        }
        else if (u.Level == 0)
        {
            buttonLabel.SetText(Loc.Get("upgrades_panel_button_buy", price));
            upgradeButton.interactable = true;
        }
        else
        {
            buttonLabel.SetText(Loc.Get("upgrades_panel_button_upgrade", price));
            upgradeButton.interactable = true;
        }

        if (price > GameManager.Instance.CollectedEnergy)
        {
            buttonLabel.SetText(Loc.Get("upgrades_panel_button_too_poor", price));
            upgradeButton.interactable = false;
        }

        // This makes the label the same color as the button
        if (!upgradeButton.interactable)
        {
            buttonLabel.color = upgradeButton.colors.disabledColor;
            nameLabel.color = upgradeButton.colors.disabledColor;
            levelLabel.color = upgradeButton.colors.disabledColor;
        }
        else
        {
            buttonLabel.color = upgradeButton.colors.normalColor;
            nameLabel.color = upgradeButton.colors.normalColor;
            levelLabel.color = upgradeButton.colors.normalColor;
        }

        // Update the level label
        levelLabel.SetText("{0}", u.Level);
    }

    private bool ButtonInteractable()
    {
        Upgrade u = UpgradesManager.Instance.GetUpgrade(upgradeIdent);
        int price = u.GetTotalPrice();

        if (u.Level == u.MaxLevel)
        {
            return false;
        }

        if (price > GameManager.Instance.CollectedEnergy)
        {
            return false;
        }

        return true;
    }

    void HandleUpgrade(Upgrade upgrade)
    {
        UpdateUi();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetSelected(true);
    }

    public void SelectButton()
    {
        EventSystem.current.SetSelectedGameObject(upgradeButton.gameObject);
    }

    public void SetSelected(bool selected)
    {
        if (selected && !Selected)
        {
            soundManager.PlaySelect();
        }

        Selected = selected;
        if (selected)
        {
            if (ButtonInteractable())
            {
                borderImage.SetActive(true);
                borderImageDisabled.SetActive(false);
                SelectButton();
            }
            else
            {
                borderImage.SetActive(false);
                borderImageDisabled.SetActive(true);
            }

            PanelSelected?.Invoke(this);
        }
        else
        {
            borderImage.SetActive(false);
            borderImageDisabled.SetActive(false);
        }
    }
}
