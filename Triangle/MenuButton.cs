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
    internal struct MenuButton
    {
        public string Text;
        public Rectangle ButtonRect;
        public Rectangle TextRect;
        public int BehaviorValue;
        public bool IsHovered(Vector2 point) => ButtonRect.Contains(point);
        public bool IsHovered(Point point) => ButtonRect.Contains(point);
        public bool IsHovered(MouseState mouseState) => ButtonRect.Contains(new Point(mouseState.X, mouseState.Y));
        public bool IsClicked(MouseState previousMouse, MouseState currentMouse)
        {
            return ButtonRect.Contains(new Vector2(currentMouse.X, currentMouse.Y)) &&
                   previousMouse.LeftButton == ButtonState.Released &&
                   currentMouse.LeftButton == ButtonState.Pressed;
        }
        public MenuButton(string text, Rectangle buttonRect, Rectangle textRect, int behaviorValue)
        {
            Text = text;
            ButtonRect = buttonRect;
            TextRect = textRect;
            BehaviorValue = behaviorValue;
        }
        public MenuButton(string text, Rectangle buttonRect, SpriteFont spriteFont, int behaviorValue)
        {
            Text = text;
            ButtonRect = buttonRect;

            Vector2 stringSize = spriteFont.MeasureString(text);
            TextRect = new Rectangle(
                (int)(buttonRect.Width - stringSize.X / 2),
                (int)(buttonRect.Height - stringSize.Y / 2),
                (int)(stringSize.X),
                (int)(stringSize.Y)
                );

            BehaviorValue = behaviorValue;
        }

        public void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget2D, Texture2D rectTexture, SpriteFont font, Color buttonColor, Color textColor, Point mousePos)
        {
            spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget2D);

            // Draw button background 
            spriteBatch.Draw(rectTexture, ButtonRect, buttonColor);

            if (IsHovered(mousePos))
            {
                buttonColor = Color.Lerp(buttonColor, Color.White, 0.3f);
            }

            // Draw button text
            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPosition = new Vector2(
                TextRect.X + (TextRect.Width - textSize.X) / 2,
                TextRect.Y + (TextRect.Height - textSize.Y) / 2);
            spriteBatch.DrawString(font, Text, textPosition, textColor);

            spriteBatch.GraphicsDevice.SetRenderTarget(null);

        }
    }
}
