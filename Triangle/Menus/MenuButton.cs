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
        public string Text;
        public Rectangle ButtonRect;
        public Rectangle TextRect;
        public int BehaviorValue;
        public Color ButtonColor;
        public bool IsHovered(Vector2 point) => ButtonRect.Contains(point);
        public bool IsHovered(Point point) => ButtonRect.Contains(point);
        public bool IsHovered(MouseState mouseState) => ButtonRect.Contains(new Point(mouseState.X, mouseState.Y));
        public bool IsClicked(Vector2 mousePos, MouseState previousMouse, MouseState currentMouse)
        {
            return ButtonRect.Contains(mousePos) &&
                   previousMouse.LeftButton == ButtonState.Released &&
                   currentMouse.LeftButton == ButtonState.Pressed;
        }
        public MenuButton(string text, Rectangle buttonRect, Rectangle textRect, int behaviorValue, Color buttonColor = new())
        {
            Text = text;
            ButtonRect = buttonRect;
            TextRect = textRect;
            BehaviorValue = behaviorValue;
            ButtonColor = buttonColor;
        }
        public MenuButton(Rectangle buttonRect, SpriteFont spriteFont, int behaviorValue, string text, Color buttonColor = new())
        {
            Text = text;
            ButtonRect = buttonRect;

            Vector2 stringSize = spriteFont.MeasureString(text);
            TextRect = new Rectangle(
                (int)(buttonRect.X + buttonRect.Width/2 - stringSize.X / 2),
                (int)(buttonRect.Y + buttonRect.Height/2 - stringSize.Y / 2),
                (int)stringSize.X,
                (int)stringSize.Y
                );

            BehaviorValue = behaviorValue;
            ButtonColor = buttonColor;
        }

        public void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget2D, Texture2D rectTexture, SpriteFont font, Color textColor, Point mousePos)
        {
            Color buttonColor = ButtonColor;

            if (IsHovered(mousePos))
            {
                buttonColor = Color.Lerp(ButtonColor, Color.White, 0.3f);
            }

            // Draw button text
            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPosition = new Vector2(
                TextRect.X + (TextRect.Width - textSize.X) / 2,
                TextRect.Y + (TextRect.Height - textSize.Y) / 2);

            // Draw button background 
            spriteBatch.Begin();
            spriteBatch.Draw(rectTexture, ButtonRect, buttonColor);
            spriteBatch.DrawString(font, Text, textPosition, textColor);
            spriteBatch.End();


        }
    }
}
