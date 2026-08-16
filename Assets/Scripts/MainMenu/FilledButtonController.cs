using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class FilledButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{

    [SerializeField]
    private Image filler;

    [SerializeField]
    private float fillTimeSeconds = 1.0f;

    private bool pressed;
    private bool selected;

    [SerializeField] private InputActionReference actionSubmit;

    [SerializeField]
    private UnityEvent onFilled;

    [SerializeField]
    private MenuSoundManager soundManager;

    void OnEnable()
    {
        actionSubmit.action.started += OnJoypadDown;
        actionSubmit.action.canceled += OnJoypadUp;
    }

    void OnDisable()
    {
        actionSubmit.action.started -= OnJoypadDown;
        actionSubmit.action.canceled -= OnJoypadUp;
    }

    void Start()
    {
        filler.fillAmount = 0.0f;
    }

    void Update()
    {
        if (pressed)
        {
            if (filler.fillAmount < 1.0f)
            {
                // fillTimeSeconds <=> 1
                // delta <=> x
                // ----------------
                //
                // x = delta/fillTimeSeconds
                filler.fillAmount += Time.unscaledDeltaTime / fillTimeSeconds;

                // Get ready to rumble:
                float intensity = filler.fillAmount * 0.3f;

                RumbleManager.Instance.StartSustainedRumble(
                        RumbleSource.MenuSystem,
                        intensity,
                        intensity);
            }
            else
            {
                RumbleManager.Instance.StopSustainedRumble(
                        RumbleSource.MenuSystem
                        );
                filler.fillAmount = 0.0f;
                soundManager.PlayClick();
                onFilled?.Invoke();
            }
        }
        else
        {
            if (filler.fillAmount > 0.0f)
            {
                filler.fillAmount -= Time.unscaledDeltaTime / fillTimeSeconds;
            }
            RumbleManager.Instance.StopSustainedRumble(
                    RumbleSource.MenuSystem
                    );
        }
    }

    private void OnJoypadDown(InputAction.CallbackContext ctx)
    {
        if (selected)
        {
            pressed = true;
        }
    }

    private void OnJoypadUp(InputAction.CallbackContext ctx)
    {
        if (selected)
        {
            pressed = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
    }

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        selected = true;
    }
}
