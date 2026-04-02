using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradesMenuController : SecondaryMenuDelegate
{
    private UpgradeId selectedUpgrade;
    [SerializeField] private Button startButton;
    [SerializeField] private Button advanceButton;

    public override void ChangeCurrentMenuSelection(int currentSelection)
    {
        // Nothing
    }

    public override void FillInStatTexts(string labelTemplate, TextMeshProUGUI statLabel)
    {
        long currentEnergy = GameManager.Instance.CollectedEnergy;
        long currentPopulation = GameManager.Instance.GetBasePopulation();

        // This sets the current stat label based on the format in the label on UI
        // Keep synched with the UI:
        // TODO: Maybe do it so that we do not need to keep it synched?

        // TODO: CHange to SetText
        statLabel.text = string.Format(
                labelTemplate,
                currentEnergy + "GW",
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
