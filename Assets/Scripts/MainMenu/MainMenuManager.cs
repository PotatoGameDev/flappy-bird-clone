using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI startButtonLabel;
    [SerializeField] private Button resetButton;

    void Start()
    {
        if (SaveSystem.StateExists())
        {
            startButtonLabel.text = "continue";
            resetButton.interactable = true;
        }
        else
        {
            startButtonLabel.text = "start";
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("NewMenu", LoadSceneMode.Single);
    }

    public void ResetGame()
    {
        SaveSystem.Reset();
        SceneManager.LoadScene("NewMenu", LoadSceneMode.Single);
    }
}
