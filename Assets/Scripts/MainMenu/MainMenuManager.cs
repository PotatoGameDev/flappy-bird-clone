using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI startButtonLabel;
    [SerializeField] private Button resetButton;
    [SerializeField] private GameObject resetProgressPanel;

    void Start()
    {
        SetStartButtonLabel();
    }

    void OnEnable()
    {
        SetStartButtonLabel();
    }

    private void SetStartButtonLabel()
    {
        if (SaveSystem.StateExists())
        {
            startButtonLabel.SetText(Loc.Get("main_continue"));
            resetButton.interactable = true;
        }
        else
        {
            startButtonLabel.SetText(Loc.Get("main_new_game"));
            resetButton.interactable = false;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("NewMenu", LoadSceneMode.Single);
    }

    public void ShowResetModal()
    {
        resetProgressPanel.SetActive(true);
    }

    public void ResetGame()
    {
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene("NewMenu", LoadSceneMode.Single);
    }

    public void CancelReset()
    {
        resetProgressPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
