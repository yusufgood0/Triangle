using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.GameAsset;
using SlimeGame.GameAsset;
using SlimeGame.Input;
using SlimeGame.Input;
using static SlimeGame.Menus.TitleScreenMenu;

namespace SlimeGame.Menus
{
    internal class ElementKeybindMenu : SettingsMenu
    {
        static Point ButtonSize = new Point(400, 100);

        static Point KeyRebindPos = new(100, 350);
        static float TextHeight = KeyRebindPos.Y - ButtonSize.Y;

        public static int ReturnToTitle = int.MaxValue;
        static MenuButton[] buttons(SpriteFont font, MasterInput masterInput, Color defaultButtonColor)
        {
            List<MenuButton> buttons = new();
            (KeybindActions keybind, Color color)[] buttonsToAdd = new (KeybindActions, Color)[]
            {
                (KeybindActions.AddElementFire, SpellBook.GetElementColor(Element.Fire)),
                //(KeybindActions.AddElementWater, SpellBook.GetElementColor(Element.Water)),
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
        public override void Draw(SpriteBatch spriteBatch, Texture2D rectTexture, SpriteFont font, Rectangle drawRect, MouseState mouseState, Color buttonColor, Color textColor, int darkenedBox = -1, string[] additionalText = null, Vector2[] additionalTextPos = null)
        {
            Vector2 keyboardText = new(KeyRebindPos.X + ButtonSize.X * 0f, TextHeight);
            Vector2 controllerText = new(KeyRebindPos.X + ButtonSize.X * 1f, TextHeight);
            Vector2 titlePos = new(500 - font.MeasureString("Rebind Menu").X / 2, KeyRebindPos.Y-200);
            base.Draw(spriteBatch, rectTexture, font, drawRect, mouseState, buttonColor, textColor, darkenedBox,
                new string[] { "Keyboard", "Controller", "Rebind Menu" },
                new Vector2[] { keyboardText, controllerText, titlePos }
                );
        }
        public ElementKeybindMenu(GraphicsDevice graphicsDevice, SpriteFont font, MasterInput masterInput, Rectangle menuParameters, Color defaultButtonColor)
            : base(graphicsDevice, font, menuParameters, defaultButtonColor, buttons(font, masterInput, defaultButtonColor))
        {
        }
    }
}
