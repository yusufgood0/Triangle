using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.GameAsset;
using SlimeGame.Input;

namespace SlimeGame.Menus
{
    internal class SettingsMenu : Menu
    {
        public int SelectedMenuButton = -1;

        static Rectangle VirtualRect = new Rectangle(0, 0, 1000, 1000);

        
        public SettingsMenu(GraphicsDevice graphicsDevice, SpriteFont font, MasterInput masterInput, Rectangle menuParameters, Color defaultButtonColor, MenuButton[] menuButtons) :
            base(graphicsDevice, menuParameters, VirtualRect, menuButtons)
        {
        }
        private void AddKeyboardAndGamePadButton(ref List<MenuButton> buttons, Rectangle keyboardRect, Rectangle gamepadRect, SpriteFont font, string titleKeyboard, string titleGamepad, Color color)
        {
            buttons.Add(new MenuButton(keyboardRect, font, buttons.Count, titleKeyboard, color));
            buttons.Add(new MenuButton(gamepadRect, font, buttons.Count, titleGamepad, color));
        }

        public void Update(MasterInput input, MouseState mouseState, MouseState previousMouseState)
        {
            KeybindActions actionToRebind;
            Buttons[] pressedButtons;
            Buttons pressedButton;
            Keys[] pressedKeys;
            Keys pressedKey;

            if (SelectedMenuButton != -1 && !input.IsPressed(KeybindActions.GamePadClick))
            {
                if (input.IsPressed(KeybindActions.BackButton))
                {
                    SelectedMenuButton = -1;
                    return;
                }


                if (SelectedMenuButton % 2 == 1)
                {
                    actionToRebind = (KeybindActions)(SelectedMenuButton % GetButtonCount() / 2);
                    pressedButtons = input.PressedButtons();

                    if (pressedButtons.Length > 0)
                    {
                        pressedButton = pressedButtons[0];
                        input.ChangeKeybind(actionToRebind, pressedButton);
                        ChangeString(SelectedMenuButton, pressedButton.ToString());
                        SelectedMenuButton = -1;
                        return;
                    }
                    return;
                }
                else
                {
                    actionToRebind = (KeybindActions)(SelectedMenuButton % GetButtonCount() / 2);
                    pressedKeys = input.PressedKeys();

                    if (pressedKeys.Length > 0)
                    {
                        pressedKey = pressedKeys[0];
                        input.ChangeKeybind(actionToRebind, pressedKey);
                        ChangeString(SelectedMenuButton, pressedKey.ToString());
                        SelectedMenuButton = -1;
                        return;
                    }
                }
                return;
            }
            else if (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                SelectedMenuButton = GetBehaviorValue(previousMouseState, mouseState);
                Debug.WriteLine(SelectedMenuButton);
                return;
            }
        }
    }
}
