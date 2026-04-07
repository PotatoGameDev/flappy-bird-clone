using UnityEngine;
using UnityEngine.EventSystems;


public class UpgradeButtonController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private UpgradePanelController upgradePanel;

    public void OnDeselect(BaseEventData eventData)
    {
        upgradePanel.SetSelected(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        upgradePanel.SetSelected(true);
    }

}
