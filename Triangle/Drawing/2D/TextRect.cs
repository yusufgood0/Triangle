using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpriteFont_inside_a_rect
{
    internal class TextRect
    {
        public Rectangle Rect;
        public SpriteFont Font;
        public string Text;

        public float _scale = 1f;
        private List<string> _lines = new();

        private Texture2D _texture;
        public TextRect(Rectangle rect, string text, SpriteFont font, float? scale = null)
        {
            Rect = rect;
            Text = text;
            Font = font;
            if (scale != null) 
            {
                _scale = (float)scale;
                _lines = BuildLines(Text, Rect.Width);
            }
            else
            {
                Recalculate();
            }
        }

        private void Recalculate()
        {
            _scale = 1f;

            // Build best possible horizontal layout first
            _lines = BuildLines(Text, Rect.Width);

            // Only scale DOWN if vertical overflow
            while ((GetTotalHeight() > Rect.Height) && _scale > 0.01f)
            {
                _lines = BuildLines(Text, Rect.Width);
                _scale -= 0.05f;
            }


            //float lineWidth = _lines.Max(i => Font.MeasureString(i).X) * _scale;
            //if (lineWidth >= Rect.Width)
            //{
            //    _scale = Rect.Width / lineWidth;
            //}
        }
        public void DrawRect(SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                _texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _texture.SetData(new Color[] { Color.White });
            }
            spriteBatch.Draw(_texture, Rect, Color.Red * 0.5f);
        }
        public void Draw(SpriteBatch spriteBatch, Color color)
        {


            Vector2 cursor = new(Rect.X, Rect.Y);

            foreach (string line in _lines)
            {
                spriteBatch.DrawString(
                    Font,
                    line,
                    cursor,
                    color,
                    0f,
                    Vector2.Zero,
                    _scale,
                    SpriteEffects.None,
                    0f
                );
                cursor.Y += Font.LineSpacing * _scale;
            }
        }

        private float GetTotalHeight()
        {
            return _lines.Count * Font.LineSpacing * _scale;
        }

        private List<string> BuildLines(string text, float maxWidth)
        {
            List<string> lines = new();
            string currentLine = "";
            float currentWidth = 0f;

            float spaceWidth = Font.MeasureString(" ").X;
            string[] words = text.Split(' ', '_');

            float widestLineWidth = 0f;

            foreach (string word in words)
            {
                float wordWidth = Font.MeasureString(word).X * _scale;

                // First word on line
                if (currentLine.Length == 0)
                {
                    currentLine = word;
                    currentWidth = wordWidth;
                    continue;
                }

                // Try to append horizontally
                float lineWidth = currentWidth + spaceWidth + wordWidth;
                if (lineWidth <= maxWidth)
                {
                    currentLine += " " + word;
                    currentWidth += spaceWidth + wordWidth;
                }
                else
                {
                    // Width exceeded → wrap
                    lines.Add(currentLine);
                    currentLine = word;
                    currentWidth = wordWidth;
                    widestLineWidth = Math.Max(widestLineWidth, lineWidth);
                }
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine);

            return lines;
        }



    }
}
