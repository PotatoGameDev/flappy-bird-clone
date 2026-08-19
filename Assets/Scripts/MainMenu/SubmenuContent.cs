using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class SubmenuContent : MonoBehaviour
{
    [SerializeField]
    internal GameObject[] inputGlyphsActive;

    [SerializeField]
    internal GameObject[] buttonsActive;

    [SerializeField]
    internal Selectable defaultSelectable;

    [SerializeField]
    internal UnityEvent onEnable;


    internal void SelectDefalutControl()
    {
        if (defaultSelectable != null)
        {
            // make sure we don't try to select the same button again
            if (EventSystem.current.currentSelectedGameObject != defaultSelectable.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(defaultSelectable.gameObject);
            }
        }
    }

    void OnEnable()
    {
        if (buttonsActive != null)
        {
            foreach (GameObject button in buttonsActive)
            {
                button.SetActive(true);
            }
        }

        onEnable.Invoke();
    }

    void OnDisable()
    {
        if (buttonsActive != null)
        {
            foreach (GameObject button in buttonsActive)
            {
                button.SetActive(false);
            }
        }
    }
}
