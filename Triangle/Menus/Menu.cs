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
    internal class Menu
    {
        RenderTarget2D _renderTarget;
        Rectangle _menuParematers;
        Rectangle _virtualScreen;
        MenuButton[] _menuButtons;
        public Vector2 GetVirtualPosition(MouseState mouseState)
        {
            var mousePos = mouseState.Position;
            return new Vector2(
                (mousePos.X - _menuParematers.X) * (float)(_virtualScreen.Width / (float)_menuParematers.Width) + _virtualScreen.X,
                (mousePos.Y - _menuParematers.Y) * (float)(_virtualScreen.Height / (float)_menuParematers.Height) + _virtualScreen.Y
                );
        }
        public Menu(GraphicsDevice graphicsDevice, Rectangle menuParematers, Rectangle virtualScreen, MenuButton[] menuButtons)
        {
            _renderTarget = new RenderTarget2D(graphicsDevice, virtualScreen.Width, virtualScreen.Height);
            _menuParematers = menuParematers;
            _virtualScreen = virtualScreen;
            _menuButtons = menuButtons;
        }
        public void Draw(SpriteBatch spriteBatch, Texture2D rectTexture, SpriteFont font, Rectangle drawRect, MouseState mouseState, Color buttonColor, Color textColor)
        {
            spriteBatch.GraphicsDevice.SetRenderTarget(_renderTarget);
            _renderTarget.GraphicsDevice.Clear(Color.RoyalBlue);
            foreach (var button in _menuButtons)
            {
                button.Draw(spriteBatch, _renderTarget, rectTexture, font, textColor, GetVirtualPosition(mouseState).ToPoint());
            }
            spriteBatch.GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin();
            spriteBatch.Draw(_renderTarget, drawRect, Color.White);
            spriteBatch.End();
        }
        public int GetBehaviorValueOnClick(MouseState previousMouse, MouseState currentMouse)
        {
            if (previousMouse.LeftButton != ButtonState.Released || currentMouse.LeftButton != ButtonState.Pressed)
            {
                return -1; // No click detected
            }
            return GetBehaviorValue(previousMouse, currentMouse);
        }
        public int GetBehaviorValue(MouseState previousMouse, MouseState currentMouse)
        {
            Vector2 virtualPosition = GetVirtualPosition(currentMouse);
            foreach (var button in _menuButtons)
            {
                if (button.IsHovered(virtualPosition))
                {
                    return button.BehaviorValue;
                }
            }

            return -1; // No button clicked
        }
        public void ChangeString(int index, string text)
        {
            _menuButtons[index].Text = text;
        }
    }
}
