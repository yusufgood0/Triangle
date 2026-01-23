using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Menus
{
    internal class PauseMenu : Menu
    {
        public enum Options
        {
            None = -1,
            Resume = 0,
            Settings = 1,
            Title = 2
        }

        public Menu Menu;
        static SpriteFont font;
        static Rectangle VirtualRect = new Rectangle(0, 0, 1000, 1000);
        static Point ButtonSize = new Point(400, 100);
        static MenuButton[] buttons(SpriteFont font, Color defaultButtonColor) => new MenuButton[]
            {
                new MenuButton(new Rectangle(new (300, 200*2), ButtonSize), font, (int) Options.Resume, Options.Resume.ToString(), defaultButtonColor),
                new MenuButton(new Rectangle(new (300, 200*3), ButtonSize), font, (int) Options.Settings, Options.Settings.ToString(), defaultButtonColor),
                new MenuButton(new Rectangle(new (300, 200*4), ButtonSize), font, (int) Options.Title, Options.Title.ToString(), defaultButtonColor),
            };
        public PauseMenu(GraphicsDevice graphicsDevice, SpriteFont font, Rectangle menuParameters, Color defaultButtonColor) :
                base(graphicsDevice, menuParameters, VirtualRect, buttons(font, defaultButtonColor))
        {

        }
        public override void Draw(SpriteBatch spriteBatch, Texture2D rectTexture, SpriteFont font, Rectangle drawRect, MouseState mouseState, Color buttonColor, Color textColor, int darkenedBox = -1, string[] additionalText = null, Vector2[] additionalTextPos = null)
        {
            Vector2 titlePos = new(500 - font.MeasureString("Paused").X / 2, 100);
            additionalText = new string[] { "Paused" };
            additionalTextPos = new Vector2[] { titlePos };
            base.Draw(spriteBatch, rectTexture, font, drawRect, mouseState, buttonColor, textColor, darkenedBox, additionalText, additionalTextPos);
        }
        
    }
}
