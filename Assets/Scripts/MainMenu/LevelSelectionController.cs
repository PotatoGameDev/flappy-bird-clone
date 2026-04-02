using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelSelectionController : SecondaryMenuDelegate
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button advanceButton;


    public override void ChangeCurrentMenuSelection(int currentSelection)
    {
        GameManager.Instance.CurrentLevel = currentSelection;
    }

    public override void FillInStatTexts(string labelTemplate, TextMeshProUGUI label)
    {
        int currentLevelSelection = GameManager.Instance.CurrentLevel;
        bool levelCompleted = currentLevelSelection < GameManager.Instance.CivTypePassed;
        bool previousLevelCompleted = currentLevelSelection - 1 < GameManager.Instance.CivTypePassed;

        long startingPopulation = GameManager.Instance.GetBasePopulation();

        long currentEnergy = GameManager.Instance.CollectedEnergy;
        long advanceEnergy = GameManager.Instance.GetAdvanceEnergy();

        label.text = string.Format(
                labelTemplate,
                previousLevelCompleted ? startingPopulation.ToString() : "??",
                levelCompleted ? "COMPLETED" : "NOT COMPLETED",
                currentEnergy + "GW",
                advanceEnergy + "GW"
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

