using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class SecondaryMenuController : MonoBehaviour
{
    [SerializeField] private SecondaryMenuDelegate controller;

    [SerializeField] private TextMeshProUGUI selectionCurrent;
    [SerializeField] private FakeButton selectionL;
    private TextMeshProUGUI selectionLabelL;
    [SerializeField] private FakeButton selectionR;
    private TextMeshProUGUI selectionLabelR;

    [SerializeField] private GameObject[] selectionContents;
    [SerializeField] private TextMeshProUGUI[] statLabels;
    private string[] statLabelTemplates;

    [SerializeField] private GameObject defaultButton;

    [SerializeField] private string[] availableOptions;

    void Awake()
    {
        selectionLabelL = selectionL.GetComponentInChildren<TextMeshProUGUI>(true);
        selectionLabelR = selectionR.GetComponentInChildren<TextMeshProUGUI>(true);

        if (statLabelTemplates == null && statLabels.Length > 0)
        {
            statLabelTemplates = new string[statLabels.Length];

            for (int i = 0; i < statLabels.Length; i++)
            {
                statLabelTemplates[i] = statLabels[i].text;
            }
        }

        if (controller != null)
        {
            currentSelection = controller.InitCurrentMenuSelection();
        }
    }


    void OnEnable()
    {
        // make sure we don't try to select the same button again
        if (defaultButton != null && EventSystem.current.currentSelectedGameObject != defaultButton)
        {
            Debug.Log("1");

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultButton);
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
            if (i == currentSelection)
            {
                selectionContents[currentSelection].SetActive(true);
            }
            else
            {
                selectionContents[i].SetActive(false);
            }
        }

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
        selectionLabelL.SetText("");
        if (currentSelection > 0)
        {
            selectionL.gameObject.SetActive(true);
            selectionLabelL.SetText(availableOptions[currentSelection - 1]);
        }
        else
        {
            selectionL.gameObject.SetActive(false);
        }

        // Center
        selectionCurrent.SetText(availableOptions[currentSelection]);

        // Right
        selectionLabelR.SetText("");
        if (currentSelection < availableOptions.Length - 1)
        {
            selectionR.gameObject.SetActive(true);
            selectionLabelR.SetText(availableOptions[currentSelection + 1]);
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
