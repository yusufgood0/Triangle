using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static System.Net.Mime.MediaTypeNames;

namespace SlimeGame.Menus
{
    internal struct MenuButton
    {
        private string _text;
        public Rectangle ButtonRect;
        public int BehaviorValue;
        public Color ButtonColor;
        private SpriteFont _font;
        private float _scale = 1;
        private static float _padding = 0.1f;
        public void SetText(String text)
        {
            _text = text;
            PreventOverFlow();
        }
        Vector2 StringSize => _font.MeasureString(_text) * _scale;
        public void PreventOverFlow()
        {
            _scale = 1;
            Vector2 size = StringSize;
            float scale1 = _scale;
            float scale2 = _scale;
            if (size.X > ButtonRect.Width)
            {
                scale1 = (float)ButtonRect.Width / size.X * scale1;
                scale1 -= scale1 * _padding  * 2f;
            }
            if (size.X > ButtonRect.Width)
            {
                scale2 = scale2 = (float)ButtonRect.Width / size.X * scale2;
                scale2 -= scale2 * _padding * 2f;
            }
            _scale = Math.Min(scale1, scale2);

        }
        public bool IsHovered(Vector2 point) => ButtonRect.Contains(point);
        public bool IsHovered(Point point) => ButtonRect.Contains(point);
        public bool IsHovered(MouseState mouseState) => ButtonRect.Contains(new Point(mouseState.X, mouseState.Y));
        public bool IsClicked(Vector2 mousePos, MouseState previousMouse, MouseState currentMouse)
        {
            return ButtonRect.Contains(mousePos) &&
                   previousMouse.LeftButton == ButtonState.Released &&
                   currentMouse.LeftButton == ButtonState.Pressed;
        }
        public MenuButton(string text, SpriteFont font, Rectangle buttonRect, Rectangle textRect, int behaviorValue, Color buttonColor = new())
        {
            
            ButtonRect = buttonRect;
            BehaviorValue = behaviorValue;
            ButtonColor = buttonColor;
            _font = font;
            SetText(text);

        }
        public MenuButton(Rectangle buttonRect, SpriteFont spriteFont, int behaviorValue, string text, Color buttonColor = new())
        {
            _font = spriteFont;
            Vector2 stringSize = spriteFont.MeasureString(text);

            BehaviorValue = behaviorValue;
            ButtonColor = buttonColor;
            ButtonRect = buttonRect;
            SetText(text);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D rectTexture, SpriteFont font, Color textColor, Point mousePos)
        {
            Color buttonColor = ButtonColor;

            if (IsHovered(mousePos))
            {
                buttonColor = Color.Lerp(ButtonColor, Color.White, 0.3f);
            }

            // Draw button text
            Vector2 textSize = StringSize;
            Vector2 textPosition = new Vector2(
                ButtonRect.X + (ButtonRect.Width - textSize.X) / 2,
                ButtonRect.Y + (ButtonRect.Height - textSize.Y) / 2);

            // Draw button background 
            spriteBatch.Begin(depthStencilState: spriteBatch.GraphicsDevice.DepthStencilState);
            spriteBatch.Draw(rectTexture, ButtonRect, buttonColor);
            spriteBatch.DrawString(font, _text, textPosition, textColor, 0, new(), _scale, 0, 0);
            spriteBatch.End();


        }
    }
}
