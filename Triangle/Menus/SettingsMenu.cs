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

namespace SlimeGame.Menus
{
    internal class SettingsMenu : Menu
    {
        public int SelectedOption = -1;

        static Rectangle VirtualRect = new Rectangle(0, 0, 1000, 1000);
        static Point ButtonSize = new Point(400, 100);
        static Vector2 Decrease = new (0, 100);

        static Point KeyRebindPos = new(100, 0);
        static Point ForwardOffset = new(ButtonSize.X, 0);
        static Point BackwardOffset = new(ButtonSize.X, ButtonSize.Y * 2);
        static Point LeftOffset = new(0, ButtonSize.Y);
        static Point RightOffset = new(ButtonSize.X * 2, ButtonSize.Y);
        static MenuButton[] buttons(SpriteFont font) => new MenuButton[]
            {
                new MenuButton(new Rectangle(KeyRebindPos + (Decrease*0).ToPoint(), ButtonSize), font, (int)Game1.Actions.AddFire, "F", SpellBook.GetElementColor(Element.Fire)),
                new MenuButton(new Rectangle(KeyRebindPos + (Decrease*1).ToPoint(), ButtonSize), font, (int)Game1.Actions.AddWater, "W", SpellBook.GetElementColor(Element.Water)),
                new MenuButton(new Rectangle(KeyRebindPos + (Decrease*2).ToPoint(), ButtonSize), font, (int)Game1.Actions.AddAir, "E", SpellBook.GetElementColor(Element.Earth)),
                new MenuButton(new Rectangle(KeyRebindPos + (Decrease*3).ToPoint(), ButtonSize), font, (int)Game1.Actions.AddEarth, "A", SpellBook.GetElementColor(Element.Air)),
            };
        public SettingsMenu(GraphicsDevice graphicsDevice, SpriteFont font, Rectangle menuParameters) :            
            base(graphicsDevice, menuParameters, VirtualRect, buttons(font))
        {
        }

        public Keys GetPressedKey(KeyboardState keyboardState)
        {
            if (keyboardState.GetPressedKeyCount() > 0)
            {
                Debug.WriteLine(keyboardState.GetPressedKeys()[0]);
                return keyboardState.GetPressedKeys()[0];

            }
            return Keys.None;
        }
        public (int, Keys)? getChanges(MouseState previousMouseState, MouseState mouseState, KeyboardState keyboard)
        {
            if (SelectedOption != -1)
            {
                Keys PressedKey = GetPressedKey(keyboard);
                if (PressedKey != Keys.None)
                {
                    var returnValue = (SelectedOption, PressedKey);
                    ChangeString(SelectedOption, PressedKey.ToString());
                    SelectedOption = -1;
                    return returnValue;
                }
            }
            if (SelectedOption == -1)
            {
                SelectedOption = GetClickedButtonBehaviorValue(previousMouseState, mouseState);
            }
            else
            {
                Debug.WriteLine(SelectedOption);
            }
            return null;
        }
    }
}
