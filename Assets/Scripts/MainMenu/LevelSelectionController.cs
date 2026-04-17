using UnityEngine;
using TMPro;

public class LevelSelectionController : SecondaryMenuDelegate
{
    [SerializeField] private FakeButton startButton;
    [SerializeField] private TextMeshProUGUI startButtonLabel;
    [SerializeField] private TextMeshProUGUI startGlyphLabel;


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
            startButtonLabel.SetText("Play");
            startGlyphLabel.SetText("Play");
        }
        else
        {
            startButton.interactable = false;
            startButtonLabel.SetText("Play [Locked]");
            startGlyphLabel.SetText("Play [Locked]");
        }
    }
}

