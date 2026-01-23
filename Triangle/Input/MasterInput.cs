using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using SlimeGame.GameAsset;

namespace SlimeGame.Input
{
    [Flags]
    public enum InputType : byte
    {
        None = 0,
        Keyboard = 1 << 0,
        Gamepad = 1 << 1,
        Both = Keyboard | Gamepad
    }
    enum KeybindActions
    {
        AddElementFire = 0,
        //AddElementWater = 1,
        AddElementEarth = 1,
        AddElementAir = 2,
        CastSpell = 3,
        Jump = 4,
        BackButton = 5,
        GamePadClick = 6,
    }
    internal class MasterInput
    {
        public KeyboardState KeyboardState;
        public KeyboardState PreviousKeyboardState;
        public GamePadState GamePadState;
        public GamePadState PreviousGamePadState;
        public MouseState MouseState;
        public MouseState PreviousMouseState;
        public Dictionary<KeybindActions, Keybind> Keybinds = new();
        public bool ControllerActive = false;
        
        static readonly Buttons[] AllButtons = (Buttons[])Enum.GetValues(typeof(Buttons));
        

        public string GetKeyBindName(KeybindActions action)
        {
            Keybind keybind = Keybinds[action];
            return ControllerActive ? keybind.GamepadButton.ToString() : keybind.KeyboardKey.ToString();
        }
        public MasterInput()
        {
            //Initialize states to avoid null errors
            KeyboardState = Keyboard.GetState();
            GamePadState = GamePad.GetState(0);
            PreviousKeyboardState = Keyboard.GetState();
            PreviousGamePadState = GamePad.GetState(0);

            //Insert default keybinds
            Keybinds.Add(KeybindActions.AddElementFire, new Keybind(Keys.E, Buttons.B));
            //Keybinds.Add(KeybindActions.AddElementWater, new Keybind(Keys.D2, Buttons.X));
            Keybinds.Add(KeybindActions.AddElementEarth, new Keybind(Keys.Q, Buttons.A));
            Keybinds.Add(KeybindActions.AddElementAir, new Keybind(Keys.C, Buttons.Y));
            Keybinds.Add(KeybindActions.CastSpell, new Keybind(Keys.R, Buttons.RightTrigger));
            Keybinds.Add(KeybindActions.BackButton, new Keybind(Keys.Escape, Buttons.Start));
            Keybinds.Add(KeybindActions.Jump, new Keybind(Keys.Space, Buttons.X));
            Keybinds.Add(KeybindActions.GamePadClick, new Keybind(Keys.None, Buttons.A));
        }
        public void AddKeybind(KeybindActions Action, Keybind keybind)
        {
            Keybinds.Add(Action, keybind);
        }
        public void UpdateStates()
        {
            PreviousKeyboardState = KeyboardState;
            PreviousGamePadState = GamePadState;
            KeyboardState = Keyboard.GetState();
            GamePadState = GamePad.GetState(0);

            if (PressedButtons().Length > 0)
            {
                ControllerActive = true;
            }
            else if (PressedKeys().Length > 0)
            {
                ControllerActive = false;
            }
        }
        public InputType ActiveInputType()
        {
            InputType keyboardActive = KeyboardState.GetPressedKeyCount() > 0 ? InputType.Keyboard : InputType.None;
            InputType gamepadActive = InputType.None;
            foreach (Buttons button in AllButtons)
            {
                if (GamePadState.IsButtonDown(button))
                {
                    gamepadActive = InputType.Gamepad;
                    break;
                }
            }
            return keyboardActive | gamepadActive;
        }
        public Keys[] PressedKeys()
            => KeyboardState.GetPressedKeys();
        public Buttons[] PressedButtons()
        {
            List<Buttons> pressedButtons = new List<Buttons>();
            foreach (Buttons button in AllButtons)
            {
                if (GamePadState.IsButtonDown(button))
                {
                    pressedButtons.Add(button);
                }
            }
            return pressedButtons.ToArray();
        }
        public bool OnRightReleased
            => (MouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed);
        
        public bool OnRightPress
            => (MouseState.RightButton == ButtonState.Pressed && PreviousMouseState.RightButton == ButtonState.Released);
        
        public bool OnLeftReleased
            => (MouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed);
        
        public bool OnLeftPress
            => (MouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released);
        

        public bool OnPress(KeybindActions Action) =>
            Keybinds[Action].OnPressed(this);
        public bool OnRelease(KeybindActions Action) =>
            Keybinds[Action].OnReleased(this);
        public bool IsPressed(KeybindActions Action) =>
            Keybinds[Action].IsPressed(this);

        public bool OnPress(Keys key) =>
            KeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);
        public bool OnRelease(Keys key) =>
            KeyboardState.IsKeyUp(key) && PreviousKeyboardState.IsKeyDown(key);
        public bool IsPressed(Keys key) =>
            KeyboardState.IsKeyDown(key);

        public bool OnPress(Buttons button) =>
            GamePadState.IsButtonDown(button) && GamePadState.IsButtonUp(button);
        public bool OnRelease(Buttons button) =>
            GamePadState.IsButtonUp(button) && GamePadState.IsButtonDown(button);
        public bool IsPressed(Buttons button) =>
            GamePadState.IsButtonDown(button);


        public void ChangeKeybind(KeybindActions Action, Buttons newButton)
        {
            Keybinds[Action] = new Keybind(Keybinds[Action].KeyboardKey, newButton);
        }
        public void ChangeKeybind(KeybindActions Action, Keys newKey)
        {
            Keybinds[Action] = new Keybind(newKey, Keybinds[Action].GamepadButton);
        }
    }
}
