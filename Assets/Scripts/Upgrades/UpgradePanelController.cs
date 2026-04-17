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
    [SerializeField] private InputActionReference navigationAction;
    [SerializeField] private UpgradePanelController navigateUp;
    [SerializeField] private UpgradePanelController navigateDown;

    public event Action<UpgradePanelController> PanelSelected;

    public bool Selected
    {
        get; private set;
    }

    void Start()
    {
        nameLabel.text = UpgradesManager.Instance.GetUpgrade(upgradeIdent).Name;
        UpdateUi();
        UpgradesManager.Instance.OnUpgrade += HandleUpgrade;
    }

    void OnEnable()
    {
        UpdateUi();
        if (defaultOnEnable && !Selected)
        {
            SetSelected(true);
        }
    }

    void OnDisable()
    {
        navigationAction.action.performed -= OnNavigate;
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
            buttonLabel.SetText("Maxed");
            upgradeButton.interactable = false;
        }
        else if (u.Level == 0)
        {
            buttonLabel.SetText("Buy ({0}GW)", price);
            upgradeButton.interactable = true;
        }
        else
        {
            buttonLabel.SetText("Upgrade ({0}GW)", price);
            upgradeButton.interactable = true;
        }

        if (price > GameManager.Instance.CollectedEnergy)
        {
            upgradeButton.interactable = false;
            buttonLabel.SetText("Too Poor ({0}GW)", price);
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
        if (selected)
        {
            if (!Selected)
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

            navigationAction.action.performed += OnNavigate;
        }
        else
        {
            borderImage.SetActive(false);
            borderImageDisabled.SetActive(false);

            navigationAction.action.performed -= OnNavigate;
        }

        Selected = selected;
    }
}
