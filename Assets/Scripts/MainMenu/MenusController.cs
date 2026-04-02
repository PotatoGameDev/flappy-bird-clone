using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenusController : MonoBehaviour
{
    void Start()
    {
        currentMainSelection = 1; // We start in level selection

        UpdateAllLabels();
    }

    void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += UpdateGameStateRelatedLabels;
    }

    void OnDisable()
    {
        GameManager.Instance.OnGameStateChanged -= UpdateGameStateRelatedLabels;
    }

    private void UpdateGameStateRelatedLabels(GameState state)
    {
        UpdateAllLabels();
    }

    private void UpdateAllLabels()
    {
        UpdateMainSelectionMenus();
    }

    // Main Selection
    [SerializeField] private TextMeshProUGUI mainSelectionCurrent;
    [SerializeField] private TextMeshProUGUI mainSelectionL;
    [SerializeField] private TextMeshProUGUI mainSelectionR;

    [SerializeField] private GameObject[] mainSelectionContents;


    private readonly string[,] mainSelectionOptions = {
        { "", "System", "Level Select" },
        { "System", "Level Select", "Upgrades" },
        { "Level Select", "Upgrades", "" },
    };

    private int currentMainSelection = 0;

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
        if (currentMainSelection >= mainSelectionOptions.GetLength(0) - 1) return;
        currentMainSelection++;
        UpdateMainSelectionMenus();
    }

    public void OnMainL()
    {
        if (currentMainSelection == 0) return;
        currentMainSelection--;
        UpdateMainSelectionMenus();
    }

    // Level Selection

    // TODO Prune
    public void OnLevelSelectStartClicked()
    {
        GameManager.Instance.levelSettings.levelType = LevelType.Normal;
        SceneManager.LoadScene("Loading");
    }

    public void OnLevelSelectAdvanceClicked()
    {
        // TODO This will happen when Player defeats the boss.
        // GameManager.Instance.UnlockNextPhase();
        //UpdateLevelSelectionMenus();

        GameManager.Instance.levelSettings.levelType = LevelType.BossFight;
        SceneManager.LoadScene("Loading");
    }
}
