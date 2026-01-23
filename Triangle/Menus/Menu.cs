using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Input;

namespace SlimeGame.Menus
{
    internal class Menu
    {
        RenderTarget2D _renderTarget;
        Rectangle _menuParematers;
        Rectangle _virtualScreen;
        MenuButton[] _menuButtons;
        public int GetButtonCount => _menuButtons.Length;
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
        public virtual void Draw(SpriteBatch spriteBatch, Texture2D rectTexture, SpriteFont font, Rectangle drawRect, MouseState mouseState, Color buttonColor, Color textColor, int darkenedBox = -1, string[] additionalText = null, Vector2[] additionalTextPos = null)
        {

            int width = spriteBatch.GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = spriteBatch.GraphicsDevice.PresentationParameters.BackBufferHeight;

            Color[] screenSnapshot = new Color[width * height];
            spriteBatch.GraphicsDevice.GetBackBufferData<Color>(screenSnapshot);
            Texture2D screenTexture = new Texture2D(spriteBatch.GraphicsDevice, width, height);
            screenTexture.SetData<Color>(screenSnapshot);


            spriteBatch.GraphicsDevice.SetRenderTarget(_renderTarget);
            _renderTarget.GraphicsDevice.Clear(Color.RoyalBlue);

            spriteBatch.Begin(depthStencilState: spriteBatch.GraphicsDevice.DepthStencilState);
            spriteBatch.Draw(screenTexture, _virtualScreen, drawRect, Color.White * 0.5f);
            spriteBatch.End();

            if (additionalText != null)
            {
                spriteBatch.Begin(depthStencilState: spriteBatch.GraphicsDevice.DepthStencilState);
                for (int i = 0; i < additionalText.Length; i++)
                {
                    spriteBatch.DrawString(font, additionalText[i], additionalTextPos[i], Color.White);
                }
                spriteBatch.End();
            }

            foreach (var button in _menuButtons)
            {
                button.Draw(spriteBatch, rectTexture, font, textColor, GetVirtualPosition(mouseState).ToPoint());
            }
            spriteBatch.GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin(depthStencilState: spriteBatch.GraphicsDevice.DepthStencilState);
            spriteBatch.Draw(_renderTarget, drawRect, Color.White);
            spriteBatch.End();

            screenTexture.Dispose();
        }
        public int GetBehaviorValueOnClick(MasterInput input)
        {
            if (input.OnLeftPress)
            {
                return GetBehaviorValue(input);
            }
            return -1; // No click detected
        }
        public int GetBehaviorValue(MasterInput input)
        {
            Vector2 virtualPosition = GetVirtualPosition(input.MouseState);
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
            _menuButtons[index].SetText(text);
        }
    }
}
