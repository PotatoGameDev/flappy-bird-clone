using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenusController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainSelectionCurrent;
    [SerializeField] private FakeButton mainSelectionL;
    private TextMeshProUGUI mainSelectionLabelL;
    [SerializeField] private FakeButton mainSelectionR;
    private TextMeshProUGUI mainSelectionLabelR;

    [SerializeField] private SecondaryMenuController[] mainSelectionContents;

    [SerializeField] private InputActionReference backAction;

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

        backAction.action.performed += BackPressed;
    }

    void OnDisable()
    {
        GameManager.Instance.OnGameStateChanged -= UpdateGameStateRelatedLabels;

        backAction.action.performed -= BackPressed;
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

        // Selecte
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
            mainSelectionContents[i].gameObject.SetActive(i == currentMainSelection);
        }
    }

    public void OnSecondaryRight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }
        mainSelectionContents[currentMainSelection].OnGoRight(ctx);
    }

    public void OnSecondaryRight()
    {
        mainSelectionContents[currentMainSelection].OnGoRight();
    }

    public void OnSecondaryLeft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }
        mainSelectionContents[currentMainSelection].OnGoLeft(ctx);
    }

    public void OnSecondaryLeft()
    {
        mainSelectionContents[currentMainSelection].OnGoLeft();
    }

    public void OnGoRight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }
        OnMainR();
    }

    public void OnGoLeft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }
        OnMainL();
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

    public void OnStartClicked(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            OnStartClicked();
        }
    }

    public void OnStartClicked()
    {
        if (!GameManager.Instance.CanPlayLevel())
        {
            return;
        }
        GameManager.Instance.levelSettings.levelType = LevelType.Normal;
        SceneManager.LoadScene("Loading");
    }

    private void BackPressed(InputAction.CallbackContext ctx)
    {
        if (currentMainSelection == 0)
        {
            Application.Quit();
            return;
        }

        currentMainSelection = 0;

        UpdateMainSelectionMenus();

        mainSelectionContents[currentMainSelection].GoToMain();
    }
}
