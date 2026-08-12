using UnityEngine;
using UnityEngine.EventSystems;

namespace PotatoGameDev.InputGlyph
{
    public class GlyphOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private GlyphIcon glyph;

        private GlyphIcon cachedGlyph;

        void Start()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                GetGlyph()?.SetShown(true);
            }
            else
            {
                GetGlyph()?.SetShown(false);
            }
        }

        void OnEnable()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                GetGlyph()?.SetShown(true);
            }
            else
            {
                GetGlyph()?.SetShown(false);
            }
        }

        void OnDisable()
        {
            GetGlyph()?.SetShown(false);
        }

        public void OnSelect(BaseEventData eventData)
        {
            GetGlyph()?.SetShown(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            GetGlyph()?.SetShown(false);
        }

        private GlyphIcon GetGlyph()
        {
            if (glyph != null)
            {
                return glyph;
            }

            if (cachedGlyph == null)
            {
                cachedGlyph = GetComponentInChildren<GlyphIcon>();
            }

            return cachedGlyph;
        }
    }
}
