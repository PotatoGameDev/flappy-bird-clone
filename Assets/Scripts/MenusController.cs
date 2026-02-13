using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenusController : MonoBehaviour
{
    // Main Selection
    [SerializeField] private TextMeshProUGUI mainSelectionCurrent;
    [SerializeField] private TextMeshProUGUI mainSelectionL;
    [SerializeField] private TextMeshProUGUI mainSelectionR;

    [SerializeField] private GameObject[] mainSelectionContents;


    private readonly string[,] mainSelectionOptions = {
        { "", "Main Menu", "Level Select" },
        { "Main Menu", "Level Select", "Upgrades" },
        { "Level Select", "Upgrades", "" },
    };

    private int currentMainSelection = 0;

    // Secondary Selection
    [SerializeField] private TextMeshProUGUI secondarySelectionCurrent;
    [SerializeField] private TextMeshProUGUI secondarySelectionL;
    [SerializeField] private TextMeshProUGUI secondarySelectionR;

    [SerializeField] private GameObject[] secondarySelectionContents;

    private readonly string[,] secondarySelectionOptions = {
        { "", "Type I Civilisation", "Type II Civilisation" },
        { "Type I Civilisation", "Type II Civilisation", "Type III Civilisation" },
        { "Type II Civilisation", "Type III Civilisation", "" },
    };

    private int currentSecondarySelection = 0;


    void Start()
    {
        UpdateMainSelectionMenus();
        UpdateSecondarySelectionMenus();
    }

    private void UpdateMainSelectionMenus()
    {
        mainSelectionL.text = mainSelectionOptions[currentMainSelection, 0];
        mainSelectionCurrent.text = mainSelectionOptions[currentMainSelection, 1];
        mainSelectionR.text = mainSelectionOptions[currentMainSelection, 2];

        for (int i = 0; i < mainSelectionContents.Length; i++)
        {
            mainSelectionContents[i].SetActive(false);
        }
        mainSelectionContents[currentMainSelection].SetActive(true);
    }

    public void OnMainR()
    {
        if (currentMainSelection == mainSelectionOptions.Length) return;
        currentMainSelection++;
        UpdateMainSelectionMenus();
    }

    public void OnMainL()
    {
        if (currentMainSelection == 0) return;
        currentMainSelection--;
        UpdateMainSelectionMenus();
    }

    private void UpdateSecondarySelectionMenus()
    {
        secondarySelectionL.text = secondarySelectionOptions[currentSecondarySelection, 0];
        secondarySelectionCurrent.text = secondarySelectionOptions[currentSecondarySelection, 1];
        secondarySelectionR.text = secondarySelectionOptions[currentSecondarySelection, 2];

        for (int i = 0; i < secondarySelectionContents.Length; i++)
        {
            secondarySelectionContents[i].SetActive(false);
        }
        secondarySelectionContents[currentSecondarySelection].SetActive(true);
    }

    public void OnSecondaryR()
    {
        if (currentSecondarySelection == secondarySelectionContents.Length) return;
        currentSecondarySelection++;
        UpdateSecondarySelectionMenus();
    }

    public void OnSecondaryL()
    {
        if (currentSecondarySelection == 0) return;
        currentSecondarySelection--;
        UpdateSecondarySelectionMenus();
    }

}
