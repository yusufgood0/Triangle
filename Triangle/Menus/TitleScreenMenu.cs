using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Menus
{
    internal class TitleScreenMenu : Menu
    {
        public enum TitleScreenMenuStates
        {
            Play = 0,
            Settings = 1,
            Quit = 2,
        }

        static Rectangle VirtualRect = new Rectangle(0, 0, 1000, 1000);
        static string Title = "Mixcast";
        static MenuButton[] buttons(SpriteFont font, Color defaultButtonColor)
        {
            List<MenuButton> buttons = new();
            buttons.Add(new MenuButton("New Game", font, new Rectangle(250, 300, 500, 100), new Rectangle(250, 300, 500, 100), (int)TitleScreenMenuStates.Play, defaultButtonColor));
            buttons.Add(new MenuButton("Settings", font, new Rectangle(250, 500, 500, 100), new Rectangle(250, 500, 500, 100), (int)TitleScreenMenuStates.Settings, defaultButtonColor));
            buttons.Add(new MenuButton("Quit", font, new Rectangle(250, 700, 500, 100), new Rectangle(250, 700, 500, 100), (int)TitleScreenMenuStates.Quit, defaultButtonColor));
            return buttons.ToArray();
        }
        public override void Draw(SpriteBatch spriteBatch, Texture2D rectTexture, SpriteFont font, Rectangle drawRect, MouseState mouseState, Color buttonColor, Color textColor, int darkenedBox = -1, string[] additionalText = null, Vector2[] additionalTextPos = null)
        {
            Vector2 titlePos = new(500 - font.MeasureString(Title).X / 2, 100);
            additionalText = new string[] { Title };
            additionalTextPos = new Vector2[] { titlePos };
            base.Draw(spriteBatch, rectTexture, font, drawRect, mouseState, buttonColor, textColor, darkenedBox, additionalText, additionalTextPos);
        }
        public TitleScreenMenu(GraphicsDevice graphicsDevice, SpriteFont font, Rectangle menuParameters, Color defaultButtonColor)
            : base(graphicsDevice, menuParameters, VirtualRect, buttons(font, defaultButtonColor))
        {
        }
    }
}
