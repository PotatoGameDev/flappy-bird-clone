using UnityEngine;
using TMPro;

public class LevelSelectionController : SecondaryMenuDelegate
{
    [SerializeField] private FakeButton startButton;
    [SerializeField] private TextMeshProUGUI startButtonLabel;
    [SerializeField] private TextMeshProUGUI startGlyphLabel;

    [SerializeField] private FakeButton selectionR;


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

        label.SetText(string.Format(
                Loc.Get("level_select_stats"),
                previousLevelCompleted ? startingPopulation.ToString() : "??",
                levelCompleted ? Loc.Get("level_select_stats_complete") : Loc.Get("level_select_stats_not_complete"),
                currentEnergy + "GW",
                advanceEnergy + "GW"
        ));

    }

    public override void UpdateMenu()
    {
        GameManager gm = GameManager.Instance;
        // Update the start button, if the previous level has been completed, then this level can be started
        startButton.gameObject.SetActive(true);
        if (gm.CanPlayLevel())
        {
            startButton.Interactable = true;
            startButtonLabel.SetText(Loc.Get("common_buttons_play"));
            startGlyphLabel.SetText(Loc.Get("common_glyphs_play"));
        }
        else
        {
            startButton.Interactable = false;
            startButtonLabel.SetText(Loc.Get("common_buttons_play_locked"));
            startGlyphLabel.SetText(Loc.Get("common_glyphs_play_locked"));


        }

        selectionR.Interactable = gm.CanPlayLevel(gm.CurrentLevel + 1);
    }
}

