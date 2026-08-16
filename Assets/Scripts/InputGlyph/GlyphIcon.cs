using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

namespace PotatoGameDev.InputGlyph
{

    public enum GlyphShowMode
    {
        WhenSelected,
        Always,
    }

    public class GlyphIcon : MonoBehaviour
    {
        [SerializeField] private InputActionReference action;
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private bool showLabel;
        [SerializeField] private GlyphShowMode showMode;

        private bool shown;
        private bool subscribed;

        public bool Shown => shown;

        void OnEnable()
        {
            shown = showMode == GlyphShowMode.Always || IsCurrentSelection();

            Subscribe();
            Refresh();
            Apply();
        }

        void Start()
        {
            if (!subscribed)
            {
                Subscribe();
            }

            Refresh();
            Apply();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (subscribed || GlyphManager.Instance == null)
            {
                return;
            }

            GlyphManager.Instance.InputSchemeChanged += OnInputSchemeChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (GlyphManager.Instance != null)
            {
                GlyphManager.Instance.InputSchemeChanged -= OnInputSchemeChanged;
            }
            subscribed = false;
        }

        public void SetShown(bool visible)
        {
            shown = visible;
            Apply();
        }

        private void OnInputSchemeChanged(string scheme, GlyphMapping currentMapping)
        {
            Refresh();
            Apply();
        }

        private void Refresh()
        {
            string bindingPath = GetBindingPath(
                    action.action,
                    GlyphManager.Instance.CurrentScheme
                    );

            Sprite sprite = null;
            if (bindingPath != null && GlyphManager.Instance != null)
            {
                sprite = GlyphManager.Instance.CurrentMapping.GetGlyph(bindingPath);
            }


            if (image != null)
            {
                image.sprite = sprite;
            }
        }

        private void Apply()
        {
            bool visible = shown && image != null && image.sprite != null;

            if (image != null)
            {
                image.enabled = visible;
            }

            if (showLabel && label != null)
            {
                label.gameObject.SetActive(visible);
            }
        }

        private bool IsCurrentSelection()
        {
            return EventSystem.current != null
                    && EventSystem.current.currentSelectedGameObject == gameObject;
        }

        static string GetBindingPath(InputAction action, string scheme)
        {
            if (scheme == null || action == null)
            {
                return null;
            }

            foreach (var binding in action.bindings)
            {
                if (binding.isComposite || binding.isPartOfComposite)
                {
                    continue;
                }

                if (binding.groups.Contains(scheme))
                {
                    if (binding.effectivePath.Contains("{"))
                    {
                        continue;
                    }

                    return binding.effectivePath;
                }
            }

            return null;
        }
    }
}
