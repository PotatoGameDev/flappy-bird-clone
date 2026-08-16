using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace PotatoGameDev.InputGlyph
{
    [DefaultExecutionOrder(-1000)]
    public class GlyphManager : MonoBehaviour
    {
        [SerializeField] private GlyphMapping genericMapping;
        [SerializeField] private GlyphMapping xboxMapping;
        [SerializeField] private GlyphMapping mouseAndKeyboardMapping;

        [SerializeField] private PlayerInput playerInput;

        public static GlyphManager Instance { get; private set; }

        public string CurrentScheme { get; private set; }
        public GlyphMapping CurrentMapping { get; private set; }

        public event Action<string, GlyphMapping> InputSchemeChanged;

        void Awake()
        {
            Instance = this;

            if (playerInput != null)
            {
                playerInput.onControlsChanged += OnControlsChanged;

                OnControlsChanged(playerInput);
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (playerInput != null)
            {
                playerInput.onControlsChanged -= OnControlsChanged;
            }
        }

        private void OnControlsChanged(PlayerInput input)
        {
            CurrentScheme = input.currentControlScheme;

            CurrentMapping = CurrentScheme switch
            {
                "Gamepad" => xboxMapping,
                "Keyboard&Mouse" => mouseAndKeyboardMapping,
                _ => genericMapping,
            };

            InputSchemeChanged?.Invoke(CurrentScheme, CurrentMapping);
        }
    }
}
