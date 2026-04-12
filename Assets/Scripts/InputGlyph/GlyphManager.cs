using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace PotatoGameDev.InputGlyph
{
    public class GlyphManager : MonoBehaviour
    {
        [SerializeField] private GlyphMapping genericMapping;
        [SerializeField] private GlyphMapping xboxMapping;
        [SerializeField] private GlyphMapping mouseAndKeyboardMapping;

        [SerializeField] private PlayerInput playerInput;

        public string CurrentScheme { get; private set; }
        public GlyphMapping CurrentMapping { get; private set; }

        public event Action<string, GlyphMapping> InputSchemeChanged;


        void Start()
        {
            playerInput.onControlsChanged += OnControlsChanged;

            OnControlsChanged(playerInput);
            Debug.Log("Start " + playerInput + "AAA");
        }

        void OnDestroy()
        {
            Debug.Log("OnDestroy");
            playerInput.onControlsChanged -= OnControlsChanged;
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
            Debug.Log("OnControlsChanged: " + CurrentScheme + " = " + CurrentMapping);

            InputSchemeChanged?.Invoke(CurrentScheme, CurrentMapping);
            // TODO
        }
    }
}
