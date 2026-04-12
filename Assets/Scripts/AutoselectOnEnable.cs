using UnityEngine;
using UnityEngine.EventSystems;

public class AutoselectOnEnable : MonoBehaviour
{
    [SerializeField] private bool autoselect;

    void OnEnable()
    {
        if (autoselect)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
