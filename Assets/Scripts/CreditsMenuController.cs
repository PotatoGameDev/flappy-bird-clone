using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CreditsMenuController : MonoBehaviour
{
    [SerializeField]
    private InputActionReference backAction;
    void OnEnable()
    {
        backAction.action.performed += GoToMainMenu;
    }

    void OnDisable()
    {
        backAction.action.performed -= GoToMainMenu;
    }

    private void GoToMainMenu(InputAction.CallbackContext ctx)
    {
        GoToMainMenu();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("NewMenu");
    }
}
