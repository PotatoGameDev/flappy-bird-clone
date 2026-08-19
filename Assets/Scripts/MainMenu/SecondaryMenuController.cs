using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class SecondaryMenuController : MonoBehaviour
{
    [SerializeField] private SecondaryMenuDelegate controller;

    [SerializeField] private TextMeshProUGUI selectionCurrent;
    [SerializeField] private FakeButton selectionL;
    [SerializeField] private TextMeshProUGUI selectionLabelL;
    [SerializeField] private FakeButton selectionR;
    [SerializeField] private TextMeshProUGUI selectionLabelR;

    [SerializeField] private SubmenuContent[] selectionContents;
    [SerializeField] private TextMeshProUGUI[] statLabels;
    private string[] statLabelTemplates;

    [SerializeField] private string[] availableOptions;

    [SerializeField] private GameObject[] glyphs;

    private int currentSelection;

    void Awake()
    {
        if (statLabelTemplates == null && statLabels.Length > 0)
        {
            statLabelTemplates = new string[statLabels.Length];

            for (int i = 0; i < statLabels.Length; i++)
            {
                if (statLabels[i] != null)
                {
                    statLabelTemplates[i] = statLabels[i].text;
                }
            }
        }

        if (controller != null)
        {
            currentSelection = controller.InitCurrentMenuSelection();
        }
    }

    void OnEnable()
    {
        SelectDefaultControl();

        UpdateMenus();

        Loc.OnLanguageChanged += UpdateMenus;
    }

    void OnDisable()
    {
        Loc.OnLanguageChanged -= UpdateMenus;
    }

    private void SelectDefaultControl()
    {
        selectionContents[currentSelection].SelectDefalutControl();
    }

    private void UpdateMenus()
    {
        SetSelectionOptions();

        // Makes the selected contents active, and all the non selected non active, duh...
        for (int i = 0; i < selectionContents.Length; i++)
        {
            if (i == currentSelection)
            {
                selectionContents[currentSelection].gameObject.SetActive(true);
            }
            else
            {
                selectionContents[i].gameObject.SetActive(false);
            }
        }

        if (controller != null)
        {
            if (statLabels.Length > 0)
            {
                TextMeshProUGUI label = statLabels[currentSelection];

                if (label != null)
                {
                    string template = statLabelTemplates[currentSelection];
                    controller.FillInStatTexts(
                            template,
                            label
                            );
                }
            }

            controller.UpdateMenu();
        }

        foreach (GameObject glyph in glyphs)
        {
            glyph.SetActive(false);
        }

        foreach (GameObject glyph in selectionContents[currentSelection]
                .inputGlyphsActive)
        {
            glyph.SetActive(true);
        }
    }

    private void SetSelectionOptions()
    {
        // Sets main selection option labels, the ones at the top:
        // Left
        selectionLabelL.SetText("");
        if (currentSelection > 0)
        {
            selectionL.gameObject.SetActive(true);
            selectionLabelL.SetText(Loc.Get(availableOptions[currentSelection - 1]));
        }
        else
        {
            selectionL.gameObject.SetActive(false);
        }

        // Center
        selectionCurrent.SetText(Loc.Get(availableOptions[currentSelection]));

        // Right
        selectionLabelR.SetText("");
        if (currentSelection < availableOptions.Length - 1)
        {
            selectionR.gameObject.SetActive(true);
            selectionLabelR.SetText(Loc.Get(availableOptions[currentSelection + 1]));

            // We set this here for a reset - it gets overwritten in level select
            // controller, and when we switch menu, it has to be reset to 
            // interactable again.
            selectionR.Interactable = true;
        }
        else
        {
            selectionR.gameObject.SetActive(false);
        }
    }

    public void OnGoRight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }
        OnGoRight();
    }

    public void OnGoLeft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }
        OnGoLeft();
    }

    public void OnGoRight()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        if (currentSelection == availableOptions.Length - 1) return;
        currentSelection++;

        if (controller != null)
        {
            controller.ChangeCurrentMenuSelection(currentSelection);
        }

        SelectDefaultControl();

        UpdateMenus();
    }

    public void OnGoLeft()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        if (currentSelection == 0) return;
        currentSelection--;

        if (controller != null)
        {
            controller.ChangeCurrentMenuSelection(currentSelection);
        }

        SelectDefaultControl();

        UpdateMenus();
    }

    public void GoToMain()
    {
        currentSelection = 0;

        if (controller != null)
        {
            controller.ChangeCurrentMenuSelection(currentSelection);
        }

        UpdateMenus();
    }
}

public abstract class SecondaryMenuDelegate : MonoBehaviour
{
    public virtual void ChangeCurrentMenuSelection(int current) { }

    public virtual void FillInStatTexts(string labelTemplate, TextMeshProUGUI statLabel) { }

    public virtual void UpdateMenu() { }

    public virtual int InitCurrentMenuSelection()
    {
        return 0;
    }

}
