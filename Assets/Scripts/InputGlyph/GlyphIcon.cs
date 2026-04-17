using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace PotatoGameDev.InputGlyph
{

    public class GlyphIcon : MonoBehaviour
    {
        [SerializeField] private InputActionReference action;
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GlyphManager glyphManager;
        [SerializeField] private bool showLabel;

        void Start()
        {
            glyphManager = FindAnyObjectByType<GlyphManager>();
            Debug.Assert(glyphManager != null, "No GlyphManager on scene!");

            glyphManager.InputSchemeChanged += InputSchemeChanged;
            InputSchemeChanged(glyphManager.CurrentScheme, glyphManager.CurrentMapping);


            if (!showLabel)
            {
                label.gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            if (glyphManager != null)
            {
                glyphManager.InputSchemeChanged += InputSchemeChanged;
                InputSchemeChanged(glyphManager.CurrentScheme, glyphManager.CurrentMapping);
            }
        }

        void OnDisable()
        {
            if (glyphManager != null)
            {
                glyphManager.InputSchemeChanged -= InputSchemeChanged;
            }
        }

        void OnDestroy()
        {
            if (glyphManager != null)
            {
                glyphManager.InputSchemeChanged -= InputSchemeChanged;
            }
        }

        void InputSchemeChanged(string scheme, GlyphMapping currentMapping)
        {
            string bindingPath = GetBindingPath2(action.action, scheme);

            Sprite sprite = null;
            if (bindingPath != null && currentMapping != null)
            {
                sprite = currentMapping.GetGlyph(bindingPath);
            }
            if (sprite == null)
            {
                // No binding in that scheme, hiding image
                image.enabled = false;
                if (showLabel)
                {
                    label.gameObject.SetActive(false);
                }
            }
            else
            {
                image.enabled = true;
                image.sprite = sprite;
                if (showLabel)
                {
                    label.gameObject.SetActive(true);
                }
            }
        }

        static string GetBindingPath(InputAction action, string scheme)
        {

            foreach (var binding in action.bindings)
            {
                if (binding.groups.Contains(scheme))
                {
                    return binding.effectivePath;
                }
            }
            return null;
        }

        static string GetBindingPath2(InputAction action, string scheme)
        {
            if (scheme == null || action == null)
            {
                return null;
            }

            foreach (var binding in action.bindings)
            {
                if (binding.isComposite || binding.isPartOfComposite)
                    continue;

                if (binding.groups.Contains(scheme))
                {
                    if (binding.effectivePath.Contains("{"))
                        continue;

                    return binding.effectivePath;
                }
            }

            return null;
        }
    }
}
