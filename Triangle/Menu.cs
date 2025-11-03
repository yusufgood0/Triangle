using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Triangle
{
    internal class Menu
    {
        RenderTarget2D _renderTarget;
        Rectangle _menuParematers;
        MenuButton[] _menuButtons;

        public Menu(GraphicsDevice graphicsDevice, Rectangle menuParematers, MenuButton[] menuButtons)
        {
            _renderTarget = new RenderTarget2D(graphicsDevice, menuParematers.Width, menuParematers.Height);
            _menuParematers = menuParematers;
            _menuButtons = menuButtons;
        }
        public void Draw(SpriteBatch spriteBatch, Texture2D rectTexture, SpriteFont font, Rectangle drawRect, MouseState mouseState, Color buttonColor, Color textColor)
        {
            _renderTarget.GraphicsDevice.Clear(Color.White);
            foreach (var button in _menuButtons)
            {
                button.Draw(spriteBatch, _renderTarget, rectTexture, font, buttonColor, textColor, mouseState.Position);
            }
            spriteBatch.Draw(_renderTarget, drawRect, Color.White);
        }
        public int GetClickedButtonBehaviorValue(MouseState previousMouse, MouseState currentMouse)
        {
            foreach (var button in _menuButtons)
            {
                if (button.IsClicked(previousMouse, currentMouse))
                {
                    return button.BehaviorValue;
                }
            }
            return -1; // No button clicked
        }
    }
}
