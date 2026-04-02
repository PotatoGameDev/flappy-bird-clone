using UnityEngine;
using TMPro;

public class SecondaryMenuController : MonoBehaviour
{
    [SerializeField] private SecondaryMenuDelegate controller;

    [SerializeField] private TextMeshProUGUI selectionCurrent;
    [SerializeField] private TextMeshProUGUI selectionL;
    [SerializeField] private TextMeshProUGUI selectionR;

    [SerializeField] private GameObject[] selectionContents;
    [SerializeField] private TextMeshProUGUI[] statLabels;
    private string[] statLabelTemplates;


    [SerializeField] private string[] availableOptions;

    // TODO: This should go to a separate controller, just for upgrades
    void OnEnable()
    {
        UpgradesManager.Instance.OnUpgrade += HandleUpgrade;
    }

    void OnDisable()
    {
        UpgradesManager.Instance.OnUpgrade -= HandleUpgrade;
    }

    private void HandleUpgrade(Upgrade u)
    {
        UpdateMenus();
    }

    void Start()
    {
        if (statLabelTemplates == null && statLabels.Length > 0)
        {
            statLabelTemplates = new string[statLabels.Length];

            for (int i = 0; i < statLabels.Length; i++)
            {
                statLabelTemplates[i] = statLabels[i].text;
            }
        }

        UpdateMenus();
    }

    private int currentSelection;

    private void UpdateMenus()
    {
        SetSelectionOptions();

        // Makes the selected contents active, and all the non selected non active, duh...
        for (int i = 0; i < selectionContents.Length; i++)
        {
            selectionContents[i].SetActive(false);
        }
        selectionContents[currentSelection].SetActive(true);


        if (controller != null)
        {
            if (statLabels.Length > 0)
            {
                controller.FillInStatTexts(
                        statLabelTemplates[currentSelection],
                        statLabels[currentSelection]
                        );
            }

            controller.UpdateMenu();
        }
    }

    private void SetSelectionOptions()
    {
        // Sets main selection option labels, the ones at the top:

        // Left
        selectionL.text = "";
        if (currentSelection > 0)
        {
            selectionL.text = availableOptions[currentSelection - 1];
        }

        // Center
        selectionCurrent.text = availableOptions[currentSelection];

        // Right
        selectionR.text = "";
        if (currentSelection < availableOptions.Length - 1)
        {
            selectionR.text = availableOptions[currentSelection + 1];
        }
    }

    public void OnGoRight()
    {
        if (currentSelection == availableOptions.Length - 1) return;
        currentSelection++;

        if (controller != null)
        {
            controller.ChangeCurrentMenuSelection(currentSelection);
        }
        UpdateMenus();
    }

    public void OnGoLeft()
    {
        if (currentSelection == 0) return;
        currentSelection--;
        if (controller != null)
        {
            controller.ChangeCurrentMenuSelection(currentSelection);
        }
        UpdateMenus();
    }
}

public abstract class SecondaryMenuDelegate : MonoBehaviour
{
    public abstract void ChangeCurrentMenuSelection(int current);

    public abstract void FillInStatTexts(string labelTemplate, TextMeshProUGUI statLabel);

    public abstract void UpdateMenu();

}
