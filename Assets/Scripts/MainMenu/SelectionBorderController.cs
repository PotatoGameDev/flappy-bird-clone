using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionBorderController : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    [SerializeField] private GameObject border;

    void OnEnable()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            border.SetActive(true);
        }
        else
        {
            border.SetActive(false);
        }
    }

    void OnDisable()
    {
        border.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        border.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        border.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
