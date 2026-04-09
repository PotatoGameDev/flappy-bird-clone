using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace PotatoGameDev.InputGlyph
{

    public class GlyphIcon : MonoBehaviour
    {
        [SerializeField] private InputActionReference action;
        [SerializeField] private Image image;
        [SerializeField] private GlyphManager glyphManager;

        void Awake()
        {
            glyphManager.InputSchemeChanged += InputSchemeChanged;
            InputSchemeChanged(glyphManager.CurrentScheme, glyphManager.CurrentMapping);
        }

        void OnDestroy()
        {
            glyphManager.InputSchemeChanged -= InputSchemeChanged;
        }

        void InputSchemeChanged(string scheme, GlyphMapping currentMapping)
        {
            string bindingPath = GetBindingPath2(action.action, scheme);
            //Debug.Log("Binding: " + bindingPath + " " + scheme);

            Sprite sprite = currentMapping.GetGlyph(bindingPath);
            if (sprite == null)
            {
                image.enabled = false;
            }
            else
            {
                image.enabled = true;
                image.sprite = sprite;
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
            foreach (var binding in action.bindings)
            {
                //Debug.Log("BBB: " + binding.path + " " + binding.groups + " " + binding.effectivePath);
                //Debug.Log("Comp: " + binding.isComposite + " " + binding.isPartOfComposite);

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
