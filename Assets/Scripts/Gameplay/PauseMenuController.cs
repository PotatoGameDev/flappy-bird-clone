using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject bossModeContent;
    [SerializeField] private GameObject normalModeContent;

    [SerializeField] private BossManager bossManager;

    [SerializeField] private InputActionReference pauseMenuInputAction;

    public bool IsPaused { get; set; } = false;

    private bool canPause = true;

    void OnEnable()
    {
        pauseMenuInputAction.action.performed += PauseTriggered;
        pauseMenuInputAction.action.canceled += PauseTriggered;
    }

    void OnDisable()
    {
        pauseMenuInputAction.action.performed -= PauseTriggered;
        pauseMenuInputAction.action.canceled -= PauseTriggered;
    }

    public void TogglePause()
    {
        if (canPause)
        {
            IsPaused = !IsPaused;

            if (IsPaused)
            {
                Time.timeScale = 0;
                content.SetActive(true);

                bossModeContent.SetActive(bossManager.IsBossActive());
                normalModeContent.SetActive(!bossManager.IsBossActive());
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
