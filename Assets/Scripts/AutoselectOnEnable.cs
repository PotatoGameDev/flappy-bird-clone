using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class AutoselectOnEnable : MonoBehaviour
{
    [SerializeField] private bool autoselect;

    void OnEnable()
    {
        if (autoselect)
        {
            //EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
