using UnityEngine;
using UnityEngine.EventSystems;

namespace PotatoGameDev.InputGlyph
{
    public class GlyphOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private GameObject glyph;

        void Start()
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                glyph.SetActive(true);
            }
            else
            {
                glyph.SetActive(false);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            glyph.SetActive(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            glyph.SetActive(false);
        }
    }
}
