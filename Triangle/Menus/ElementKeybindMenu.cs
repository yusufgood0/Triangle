using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.GameAsset;
using SlimeGame.Input;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.GameAsset;
using SlimeGame.Input;

namespace SlimeGame.Menus
{
    internal class ElementKeybindMenu : SettingsMenu
    {
        static Point ButtonSize = new Point(400, 100);

        static Point KeyRebindPos = new(100, 0);
        static MenuButton[] buttons(SpriteFont font, MasterInput masterInput, Color defaultButtonColor)
        {
            List<MenuButton> buttons = new();
            (KeybindActions keybind, Color color)[] buttonsToAdd = new (KeybindActions, Color)[]
            {
                (KeybindActions.AddElementFire, SpellBook.GetElementColor(Element.Fire)),
                (KeybindActions.AddElementWater, SpellBook.GetElementColor(Element.Water)),
                (KeybindActions.AddElementEarth, SpellBook.GetElementColor(Element.Earth)),
                (KeybindActions.AddElementAir, SpellBook.GetElementColor(Element.Air)),
                (KeybindActions.CastSpell, defaultButtonColor),
                (KeybindActions.Jump, defaultButtonColor),
            };
            int buttonCount = buttonsToAdd.Length * 2; // Each action has a keyboard and gamepad button
            Rectangle[] rectangles = new Rectangle[buttonCount];
            for (int i = 0; i < buttonCount; i++)
            {
                rectangles[i] = new Rectangle(KeyRebindPos + new Point((i % 2) * ButtonSize.X, (i / 2) * ButtonSize.Y), ButtonSize);
            }
            foreach ((KeybindActions keybind, Color color) in buttonsToAdd)
            {
                buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[keybind].KeyboardKey.ToString(), color));
                buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[keybind].GamepadButton.ToString(), color));
            }

            return buttons.ToArray();
        }
        public ElementKeybindMenu(GraphicsDevice graphicsDevice, SpriteFont font, MasterInput masterInput, Rectangle menuParameters, Color defaultButtonColor)
            : base(graphicsDevice, font, masterInput, menuParameters, defaultButtonColor, buttons(font, masterInput, defaultButtonColor))
        {
        }
    }
}
