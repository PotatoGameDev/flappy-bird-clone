using UnityEngine;
using UnityEngine.EventSystems;

public class AutoselectOnEnable : MonoBehaviour
{
    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
