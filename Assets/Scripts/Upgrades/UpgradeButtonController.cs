using UnityEngine;
using UnityEngine.EventSystems;


public class UpgradeButtonController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private UpgradePanelController upgradePanel;
    [SerializeField] private bool defaultOnEnable;
    [SerializeField] private MenuSoundManager soundManager;

    void OnEnable()
    {
        if (defaultOnEnable && EventSystem.current.currentSelectedGameObject != gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    void OnDisable()
    {
        if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            // Manually, because if the button gets disabled, OnDeselect does not run.
            EventSystem.current.SetSelectedGameObject(null);

            upgradePanel.SetSelected(false);
        }
    }

    // This does not get called when the button gets disabled!
    public void OnDeselect(BaseEventData eventData)
    {
        upgradePanel.SetSelected(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        upgradePanel.SetSelected(true);

        soundManager.PlaySelect();
    }

}
