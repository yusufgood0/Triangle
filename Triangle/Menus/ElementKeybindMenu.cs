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
        static Point ForwardOffset = new(ButtonSize.X, 0);
        static Point BackwardOffset = new(ButtonSize.X, ButtonSize.Y * 2);
        static Point LeftOffset = new(0, ButtonSize.Y);
        static Point RightOffset = new(ButtonSize.X * 2, ButtonSize.Y);
        static MenuButton[] buttons(SpriteFont font, MasterInput masterInput, Color defaultButtonColor)
        {
            List<MenuButton> buttons = new();
            Rectangle[] rectangles = new Rectangle[12];

            for (int i = 0; i < 12; i++)
            {
                rectangles[i] = new Rectangle(KeyRebindPos + new Point((i % 2) * ButtonSize.X, (i / 2) * ButtonSize.Y), ButtonSize);
            }

            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.AddElementFire].KeyboardKey.ToString(), SpellBook.GetElementColor(Element.Fire)));
            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.AddElementFire].GamepadButton.ToString(), SpellBook.GetElementColor(Element.Fire)));

            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.AddElementWater].KeyboardKey.ToString(), SpellBook.GetElementColor(Element.Water)));
            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.AddElementWater].GamepadButton.ToString(), SpellBook.GetElementColor(Element.Water)));

            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.AddElementEarth].KeyboardKey.ToString(), SpellBook.GetElementColor(Element.Earth)));
            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.AddElementEarth].GamepadButton.ToString(), SpellBook.GetElementColor(Element.Earth)));

            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.AddElementAir].KeyboardKey.ToString(), SpellBook.GetElementColor(Element.Air)));
            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.AddElementAir].GamepadButton.ToString(), SpellBook.GetElementColor(Element.Air)));

            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.CastSpell].KeyboardKey.ToString(), defaultButtonColor));
            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.CastSpell].GamepadButton.ToString(), defaultButtonColor));

            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.Jump].KeyboardKey.ToString(), defaultButtonColor));
            buttons.Add(new MenuButton(rectangles[buttons.Count], font, buttons.Count, masterInput.Keybinds[KeybindActions.Jump].GamepadButton.ToString(), defaultButtonColor));

            return buttons.ToArray();
        }
        public ElementKeybindMenu(GraphicsDevice graphicsDevice, SpriteFont font, MasterInput masterInput, Rectangle menuParameters, Color defaultButtonColor)
            : base(graphicsDevice, font, masterInput, menuParameters, defaultButtonColor, buttons(font, masterInput, defaultButtonColor))
        {
        }
    }
}
