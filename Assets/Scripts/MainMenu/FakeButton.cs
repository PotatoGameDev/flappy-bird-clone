using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class FakeButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private InputActionReference actionSubmit;
    [SerializeField] private UnityEvent onClicked;

    public bool interactable = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (interactable)
        {
            onClicked?.Invoke();
        }
    }

    public void OnInputActionPerformed(InputAction.CallbackContext ctx)
    {
        if (interactable)
        {
            onClicked?.Invoke();
        }
    }

    void OnEnable()
    {
        actionSubmit.action.performed += OnInputActionPerformed;
    }

    void OnDisable()
    {
        actionSubmit.action.performed -= OnInputActionPerformed;
    }
}
