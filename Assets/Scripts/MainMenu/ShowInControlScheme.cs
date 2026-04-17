using UnityEngine;
using UnityEngine.InputSystem;

public class ShowInControlScheme : MonoBehaviour
{
    [SerializeField] private string[] schemes;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject[] objects;
    [SerializeField] private MonoBehaviour[] components;

    void OnEnable()
    {
        playerInput.onControlsChanged += OnControlsChanged;
    }

    void OnDisable()
    {
        playerInput.onControlsChanged -= OnControlsChanged;
    }

    private void OnControlsChanged(PlayerInput input)
    {
        bool show = false;
        foreach (string scheme in schemes)
        {
            if (scheme == input.currentControlScheme)
            {
                show = true;
                break;
            }
        }
        foreach (GameObject obj in objects)
        {
            obj.SetActive(show);
        }
        foreach (MonoBehaviour cmp in components)

        {
            cmp.enabled = show;
        }
    }

}
