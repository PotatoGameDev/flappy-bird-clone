using UnityEngine;
using System;
using System.Collections.Generic;

namespace PotatoGameDev.InputGlyph
{
    [CreateAssetMenu(fileName = "GlyphMapping", menuName = "Scriptable Objects/GlyphMapping")]
    public class GlyphMapping : ScriptableObject
    {
        public string inputScheme;

        [Serializable]
        public class ActionGlyph
        {
            public string bindingPath;
            public Sprite glyph;
        }

        public List<ActionGlyph> mappings;

        public Sprite GetGlyph(string bindingPath)
        {
            Debug.Log($"Checking mappings: {mappings} for bindingPath: {bindingPath}");
            foreach (ActionGlyph glyph in mappings)
            {
                if (bindingPath.Contains(glyph.bindingPath))
                {
                    Debug.Log($"bindingPath: {bindingPath}, contains {glyph.bindingPath}");
                    return glyph.glyph;
                }
                else
                {
                    Debug.Log($"bindingPath: {bindingPath}, DOESNT contain {glyph.bindingPath}");
                }
            }
            return null;
        }
    }
}
