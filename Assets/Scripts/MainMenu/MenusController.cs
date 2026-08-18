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

    private InputAction backAction;

    [SerializeField] private string[] mainSelectionOptions;

    private int currentMainSelection = -1;

    private const int LEVEL_SELECTION_MENU = 0;
    private const int UPGRADES_MENU = 1;

    void Awake()
    {
        mainSelectionLabelL = mainSelectionL.GetComponentInChildren<TextMeshProUGUI>(true);
        mainSelectionLabelR = mainSelectionR.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void Start()
    {
        UpdateAllLabels();
    }

    void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += UpdateGameStateRelatedLabels;

        // TODO: The normal action reference was ignored on mobile.
        //       Nothing else worked :(
        backAction = new InputAction("Back", binding: "<Keyboard>/escape");
        backAction.performed += BackPressed;
        backAction.Enable();

        // This is for when the player returns from the gameplay.
        // If just turned on the game - go to level select.
        // If new level unlocked - level select.
        // Any other case - just go to the upgrades, makes most sense.
        if (GameManager.Instance.newLevelUnlocked ||
                !GameManager.Instance.backFromGameplay)
        {
            GameManager.Instance.newLevelUnlocked = false;

            currentMainSelection = LEVEL_SELECTION_MENU;
        }
        else
        {
            currentMainSelection = UPGRADES_MENU;
        }

        Loc.OnLanguageChanged += UpdateMainSelectionMenus;
    }

    void OnDisable()
    {
        GameManager.Instance.OnGameStateChanged -= UpdateGameStateRelatedLabels;

        backAction.performed -= BackPressed;
        backAction.Disable();
        backAction.Dispose();

        Loc.OnLanguageChanged -= UpdateMainSelectionMenus;
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
            mainSelectionLabelL.SetText(Loc.Get(mainSelectionOptions[currentMainSelection - 1]));
        }
        else
        {
            mainSelectionL.gameObject.SetActive(false);
        }

        // Selecte
        mainSelectionCurrent.SetText(Loc.Get(mainSelectionOptions[currentMainSelection]));

        // R1
        if (currentMainSelection < mainSelectionOptions.Length - 1)
        {
            mainSelectionR.gameObject.SetActive(true);
            mainSelectionLabelR.SetText(Loc.Get(mainSelectionOptions[currentMainSelection + 1]));
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
