using UnityEngine;
using UnityEngine.InputSystem;

public class ShowMouseInControlScheme : MonoBehaviour
{
    [SerializeField] private string[] schemes;
    [SerializeField] private PlayerInput playerInput;

    private static string lastScheme;

    void OnEnable()
    {
        playerInput.onControlsChanged += OnControlsChanged;

        // Initial check, so we don't need actual change
        // to hide/show the item.

        if (lastScheme == null)
        {
            lastScheme = playerInput.currentControlScheme;
        }

        ApplyCursorState(lastScheme);
    }

    void OnDisable()
    {
        playerInput.onControlsChanged -= OnControlsChanged;
    }

    private void ApplyCursorState(string scheme)
    {
        foreach (string s in schemes)
        {
            if (scheme == s)
            {
                Cursor.visible = true;
                return;
            }
        }
        Cursor.visible = false;
    }

    private void OnControlsChanged(PlayerInput input)
    {
        lastScheme = input.currentControlScheme;
        ApplyCursorState(lastScheme);
    }
}
