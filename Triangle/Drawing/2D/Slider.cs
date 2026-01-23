using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame
{
    internal struct Slider
    {
        Rectangle _drawRect;
        float _percent;
        Color _backColor;
        Color _fillColor;
        public float Percent
        {
            get => _percent;
            set
            {
                _percent = Math.Clamp(value, 0f, 1f);
            }
        }
        public Rectangle Rectangle
        {
            get => _drawRect;
        }
        public Color Color
        {
            get => _fillColor;
        }
        public Slider(Rectangle drawRect, float percent, Color backColor, Color fillColor)
        {
            this._drawRect = drawRect;
            this.Percent = percent;
            this._backColor = backColor;
            this._fillColor = fillColor;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            spriteBatch.Draw(pixel, _drawRect, _backColor);
            Rectangle fillRect = new Rectangle(_drawRect.X, _drawRect.Y, (int)(_drawRect.Width * Percent), _drawRect.Height);
            spriteBatch.Draw(pixel, fillRect, _fillColor);
        }
    }
}
