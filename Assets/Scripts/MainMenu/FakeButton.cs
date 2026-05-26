using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;
using PotatoGameDev.InputGlyph;

public class FakeButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private InputActionReference actionSubmit;
    [SerializeField] private UnityEvent onClicked;
    [SerializeField] private MenuSoundManager soundManager;

    [SerializeField] private Color interactableColor = Color.white;
    [SerializeField] private Color nonInteractableColor = Color.gray;

    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GlyphIcon glyphIcon;

    private bool interactable = true;
    public bool Interactable
    {
        get { return interactable; }
        set
        {
            interactable = value;

            label.color = interactable
                ? interactableColor
                : nonInteractableColor;

            if (glyphIcon != null)
            {
                glyphIcon.gameObject.SetActive(interactable);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (interactable)
        {
            if (soundManager != null)
            {
                soundManager.PlayClick();
            }
            onClicked?.Invoke();
        }
    }

    public void OnInputActionPerformed(InputAction.CallbackContext ctx)
    {
        if (interactable)
        {
            if (soundManager != null)
            {
                soundManager.PlayClick();
            }
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
