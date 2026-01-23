using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.GameAsset;
using SlimeGame.GameAsset.Exp;
using SlimeGame.Input;

namespace SlimeGame.Menus
{
    internal class GameOverMenu : Menu
    {
        public enum GameOverMenuStates
        {
            GoToTitle = 0,
            Quit = 1,
        }
        ExpManager ExpManager;
        static Rectangle VirtualRect = new Rectangle(0, 0, 1000, 1000);
        static MenuButton[] buttons(SpriteFont font, Color defaultButtonColor)
        {
            List<MenuButton> buttons = new();
            buttons.Add(new MenuButton("Title", font, new Rectangle(250, 500, 500, 100), new Rectangle(250, 700, 500, 100), (int)GameOverMenuStates.GoToTitle, Color.DeepSkyBlue));
            buttons.Add(new MenuButton("Quit", font, new Rectangle(250, 700, 500, 100), new Rectangle(250, 700, 500, 100), (int)GameOverMenuStates.Quit, Color.Red));
            return buttons.ToArray();
        }
        public override void Draw(SpriteBatch spriteBatch, Texture2D rectTexture, SpriteFont font, Rectangle drawRect, MouseState mouseState, Color buttonColor, Color textColor, int darkenedBox = -1, string[] additionalText = null, Vector2[] additionalTextPos = null)
        {
            string score = "Final Score: " + ExpManager.LifeTimeExperience.ToString();

            Vector2 titlePos = new(500 - font.MeasureString("Game Over").X / 2, 100);
            Vector2 scorePos = new(500 - font.MeasureString(score).X / 2, titlePos.Y + font.LineSpacing);
            additionalText = new string[] { "Game Over", score };
            additionalTextPos = new Vector2[] { titlePos, scorePos };
            base.Draw(spriteBatch, rectTexture, font, drawRect, mouseState, buttonColor, textColor, darkenedBox, additionalText, additionalTextPos);
        }
        public GameOverMenu(GraphicsDevice graphicsDevice, SpriteFont font, Rectangle menuParameters, Color defaultButtonColor, ExpManager expManager)
            : base(graphicsDevice, menuParameters, VirtualRect, buttons(font, defaultButtonColor))
        {
            ExpManager = expManager;
        }
    }
}
