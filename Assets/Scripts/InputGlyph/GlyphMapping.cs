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
            foreach (ActionGlyph glyph in mappings)
            {
                if (bindingPath.Contains(glyph.bindingPath))
                {
                    return glyph.glyph;
                }
            }
            return null;
        }
    }
}
