using UnityEngine;
using UnityEngine.InputSystem;

public class ShowMouseInControlScheme : MonoBehaviour
{
    [SerializeField] private string[] schemes;
    [SerializeField] private PlayerInput playerInput;

    void OnEnable()
    {
        playerInput.onControlsChanged += OnControlsChanged;

        // Initial check, so we don't need actual change
        // to hide/show the item.
        OnControlsChanged(playerInput);
    }

    void OnDisable()
    {
        playerInput.onControlsChanged -= OnControlsChanged;
    }

    private void OnControlsChanged(PlayerInput input)
    {
        foreach (string scheme in schemes)
        {
            if (scheme == input.currentControlScheme)
            {
                Cursor.visible = true;
                return;
            }
        }
        Cursor.visible = false;
    }

}
