using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenusController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainSelectionCurrent;
    [SerializeField] private Button mainSelectionL;
    private TextMeshProUGUI mainSelectionLabelL;
    [SerializeField] private Button mainSelectionR;
    private TextMeshProUGUI mainSelectionLabelR;

    [SerializeField] private GameObject[] mainSelectionContents;

    [SerializeField] private string[] mainSelectionOptions;

    private int currentMainSelection = 0;
    void Awake()
    {
        mainSelectionLabelL = mainSelectionL.GetComponentInChildren<TextMeshProUGUI>(true);
        mainSelectionLabelR = mainSelectionR.GetComponentInChildren<TextMeshProUGUI>(true);
    }

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


    private void UpdateMainSelectionMenus()
    {
        // L1
        if (currentMainSelection > 0)
        {
            mainSelectionL.gameObject.SetActive(true);
            mainSelectionLabelL.SetText(mainSelectionOptions[currentMainSelection - 1]);
        }
        else
        {
            mainSelectionL.gameObject.SetActive(false);
        }

        // Selected
        mainSelectionCurrent.SetText(mainSelectionOptions[currentMainSelection]);

        // R1
        if (currentMainSelection < mainSelectionOptions.Length - 1)
        {
            mainSelectionR.gameObject.SetActive(true);
            mainSelectionLabelR.SetText(mainSelectionOptions[currentMainSelection + 1]);
        }
        else
        {
            mainSelectionR.gameObject.SetActive(false);
        }

        // Enabling/disabling the contents when selected/unselected
        for (int i = 0; i < mainSelectionContents.Length; i++)
        {
            mainSelectionContents[i].SetActive(i == currentMainSelection);
        }
    }

    public void OnMainR()
    {
        if (currentMainSelection >= mainSelectionOptions.Length - 1) return;
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
