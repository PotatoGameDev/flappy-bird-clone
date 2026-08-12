using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject bossModeContent;
    [SerializeField] private GameObject normalModeContent;

    [SerializeField] private GameObject defaultSelectedButton;

    [SerializeField] private BossManager bossManager;

    [SerializeField] private InputActionReference pauseMenuInputAction;

#if UNITY_ANDROID
    private InputAction backAction;
#endif

    public bool IsPaused { get; set; } = false;

    private bool canPause = true;

    void OnEnable()
    {
        pauseMenuInputAction.action.performed += PauseTriggered;
        pauseMenuInputAction.action.canceled += PauseTriggered;

#if UNITY_ANDROID
        // Workaround for android:
        backAction = new InputAction("Back", binding: "<Keyboard>/escape");
        backAction.performed += PauseTriggered;
        backAction.canceled += PauseTriggered;
        backAction.Enable();
#endif
    }

    void OnDisable()
    {
        pauseMenuInputAction.action.performed -= PauseTriggered;
        pauseMenuInputAction.action.canceled -= PauseTriggered;

#if UNITY_ANDROID
        // Workaround for android:
        backAction.performed -= PauseTriggered;
        backAction.canceled -= PauseTriggered;
        backAction.Disable();
        backAction.Dispose();
#endif
    }

    public void TogglePause()
    {
        Debug.Log("Clicked!");
        if (canPause)
        {
            IsPaused = !IsPaused;

            if (IsPaused)
            {
                Time.timeScale = 0;
                content.SetActive(true);

                bossModeContent.SetActive(bossManager.IsBossActive());
                normalModeContent.SetActive(!bossManager.IsBossActive());

                if (defaultSelectedButton != null && EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
                }
            }
            else
            {
                Time.timeScale = 1;
                content.SetActive(false);
            }
        }
    }

    public void Back()
    {
        // We are in pause so we unpause first:
        TogglePause();

        // then we kill the player:
        GameplayManager.Instance.Player.Death();
    }

    private void PauseTriggered(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            TogglePause();
            canPause = false;
        }
        else if (ctx.canceled)
        {
            canPause = true;
        }
    }
}
