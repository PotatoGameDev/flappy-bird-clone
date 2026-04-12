using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelSelectionController : SecondaryMenuDelegate
{
    [SerializeField] private Button startButton;

    void Start()
    {
        UpdateMenu();
    }

    public override void ChangeCurrentMenuSelection(int currentSelection)
    {
        GameManager.Instance.CurrentLevel = currentSelection;
    }

    public override int InitCurrentMenuSelection()
    {
        return GameManager.Instance.CurrentLevel;
    }

    public override void FillInStatTexts(
            string labelTemplate,
            TextMeshProUGUI label)
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
}

