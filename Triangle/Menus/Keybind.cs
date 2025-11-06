using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Menus
{
    
    internal readonly struct Keybind
    {
        public readonly Keys KeyboardKey;
        public readonly Buttons GamepadButton;

        public Keybind(Keys key, Buttons button)
        {
            KeyboardKey = key;
            GamepadButton = button;
        }
        public bool IsPressed(MasterInput masterInput)
        {
            return masterInput.KeyboardState.IsKeyDown(KeyboardKey) || masterInput.GamePadState.IsButtonDown(GamepadButton);
        }
        public bool IsPressedPrevious(MasterInput masterInput)
        {
            return masterInput.PreviousKeyboardState.IsKeyDown(KeyboardKey) || masterInput.PreviousGamePadState.IsButtonDown(GamepadButton);
        }
        public bool OnReleased(MasterInput masterInput)
        {
            return IsPressedPrevious(masterInput) && !IsPressed(masterInput);
        }
        public bool OnPressed(MasterInput masterInput)
        {
            return !IsPressedPrevious(masterInput) && IsPressed(masterInput);
        }
    }
}
