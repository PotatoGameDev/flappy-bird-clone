using UnityEngine;
using UnityEngine.EventSystems;

public class SoundOnSelect : MonoBehaviour, ISelectHandler
{
    [SerializeField] private MenuSoundManager soundManager;

    public void OnSelect(BaseEventData eventData)
    {
        soundManager.PlaySelect();
    }
}
