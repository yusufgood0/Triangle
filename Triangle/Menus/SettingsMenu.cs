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
        static Point ButtonSize = new Point(400, 100);

        static Point KeyRebindPos = new(100, 0);
        static Point ForwardOffset = new(ButtonSize.X, 0);
        static Point BackwardOffset = new(ButtonSize.X, ButtonSize.Y * 2);
        static Point LeftOffset = new(0, ButtonSize.Y);
        static Point RightOffset = new(ButtonSize.X * 2, ButtonSize.Y);
        static MenuButton[] buttons(SpriteFont font, MasterInput masterInput) => new MenuButton[]
            {
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*0, ButtonSize.Y*0), ButtonSize), font, 0, masterInput.Keybinds[KeybindActions.AddElementFire].KeyboardKey.ToString(), SpellBook.GetElementColor(Element.Fire)),
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*0, ButtonSize.Y*1), ButtonSize), font, 1, masterInput.Keybinds[KeybindActions.AddElementWater].KeyboardKey.ToString(), SpellBook.GetElementColor(Element.Water)),
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*0, ButtonSize.Y*2), ButtonSize), font, 2, masterInput.Keybinds[KeybindActions.AddElementEarth].KeyboardKey.ToString(), SpellBook.GetElementColor(Element.Earth)),
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*0, ButtonSize.Y*3), ButtonSize), font, 3, masterInput.Keybinds[KeybindActions.AddElementAir].KeyboardKey.ToString(), SpellBook.GetElementColor(Element.Air)),
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*0, ButtonSize.Y*4), ButtonSize), font, 4, masterInput.Keybinds[KeybindActions.CastSpell].KeyboardKey.ToString(), Color.Plum),
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*0, ButtonSize.Y*5), ButtonSize), font, 5, masterInput.Keybinds[KeybindActions.Jump].KeyboardKey.ToString(), Color.Plum),

                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*1, ButtonSize.Y*0), ButtonSize), font, 6, masterInput.Keybinds[KeybindActions.AddElementFire].GamepadButton.ToString(), SpellBook.GetElementColor(Element.Fire)),
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*1, ButtonSize.Y*1), ButtonSize), font, 7, masterInput.Keybinds[KeybindActions.AddElementWater].GamepadButton.ToString(), SpellBook.GetElementColor(Element.Water)),
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*1, ButtonSize.Y*2), ButtonSize), font, 8, masterInput.Keybinds[KeybindActions.AddElementEarth].GamepadButton.ToString(), SpellBook.GetElementColor(Element.Earth)),
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*1, ButtonSize.Y*3), ButtonSize), font, 9, masterInput.Keybinds[KeybindActions.AddElementAir].GamepadButton.ToString(), SpellBook.GetElementColor(Element.Air)),
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*1, ButtonSize.Y*4), ButtonSize), font, 10, masterInput.Keybinds[KeybindActions.CastSpell].GamepadButton.ToString(), Color.Plum),
                new MenuButton(new Rectangle(KeyRebindPos + new Point(ButtonSize.X*1, ButtonSize.Y*5), ButtonSize), font, 11, masterInput.Keybinds[KeybindActions.Jump].KeyboardKey.ToString(), Color.Plum),
            };
        int KeybindsCount;
        public SettingsMenu(GraphicsDevice graphicsDevice, SpriteFont font, MasterInput masterInput, Rectangle menuParameters) :
            base(graphicsDevice, menuParameters, VirtualRect, buttons(font, masterInput))
        {
            KeybindsCount = buttons(font, masterInput).Length / 2;
        }

        public void Update(MasterInput input, MouseState mouseState, MouseState previousMouseState)
        {
            if (SelectedMenuButton != -1 && !input.IsPressed(KeybindActions.GamePadClick))
            {
                if (input.IsPressed(KeybindActions.BackButton))
                {
                    SelectedMenuButton = -1;
                }

                KeybindActions actionToRebind;
                if (SelectedMenuButton > 3)
                {
                    actionToRebind = (KeybindActions)(SelectedMenuButton % KeybindsCount);
                    Buttons[] pressedButtons = input.PressedButtons();
                    Buttons pressedButton;
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
                    actionToRebind = (KeybindActions)SelectedMenuButton;

                    Keys[] pressedKeys = input.PressedKeys();
                    Keys pressedKey;
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
