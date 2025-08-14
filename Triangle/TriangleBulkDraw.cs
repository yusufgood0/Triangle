using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Triangle
{
    internal class TriangleBulkDraw
    {
        Byte[] _pixelBuffer = new Byte[3840*2160];
        List<(Rectangle ScreenPos, Color Tint, int BufferPlacement)> _drawInfos = new List<(Rectangle screenPos, Color Tint, int BufferPlacement)>();
        int _CurrentBufferPlacement = 0;
        Texture2D _texture;

        public TriangleBulkDraw(SpriteBatch spriteBatch, Point screenSize)
        {
            this._texture = new Texture2D(spriteBatch.GraphicsDevice, screenSize.X, screenSize.Y);
        }
        public void AddTriangle(Rectangle ScreenPos, Color Tint, Byte[] pixels)
        {
            _drawInfos.Add((ScreenPos, Tint, _CurrentBufferPlacement));
            Array.Copy(pixels, 0, _pixelBuffer, _CurrentBufferPlacement, pixels.Length);
        }
        public void ProcessAll(SpriteBatch spriteBatch)
        {
            Color[] _pixelBufferColors = new Color[_pixelBuffer.Length];
            int i = 0;
            foreach (var num in _pixelBuffer)
            {
                int Int = (int)num;
                _pixelBufferColors[i++] = new Color(num, num, num, num);
            }
            foreach (var (ScreenPos, Tint, BufferPlacement) in _drawInfos)
            {
                _texture.SetData(0, ScreenPos, _pixelBuffer, BufferPlacement, ScreenPos.Width * ScreenPos.Height);
                spriteBatch.Draw(_texture, ScreenPos, ScreenPos, Tint, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            }
        }
    }
}
