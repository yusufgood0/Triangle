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
        Color[] _pixelBuffer = new Color[Array.MaxLength];
        List<(Rectangle ScreenPos, Color Tint, int BufferPlacement)> _drawInfos = new List<(Rectangle screenPos, Color Tint, int BufferPlacement)>();
        int _CurrentBufferPlacement = 0;
        Texture2D _texture;

        public TriangleBulkDraw(SpriteBatch spriteBatch, Point screenSize)
        {
            GraphicsDevice GraphicsDevice = spriteBatch.GraphicsDevice;
            this._texture = new Texture2D(GraphicsDevice, screenSize.X, screenSize.Y);
            
        }
        public void AddTriangle(Rectangle ScreenPos, Color Tint, Color[] pixels)
        {
            _drawInfos.Add((ScreenPos, Tint, _CurrentBufferPlacement));
            Array.Copy(pixels, 0, _pixelBuffer, _CurrentBufferPlacement, pixels.Length);
            _CurrentBufferPlacement += pixels.Length;
        }
        public void ProcessAll(ref SpriteBatch spriteBatch)
        {
            if (_drawInfos.Count == 0) return;

            spriteBatch.Begin();
            foreach (var (ScreenPos, Tint, BufferPlacement) in _drawInfos)
            {
                //_texture.SetData(0, ScreenPos, _pixelBuffer, BufferPlacement, ScreenPos.Width * ScreenPos.Height);
                //_texture?.Dispose();
                _texture = new Texture2D(spriteBatch.GraphicsDevice, ScreenPos.Width, ScreenPos.Height);
                _texture.SetData(_pixelBuffer, BufferPlacement, ScreenPos.Width * ScreenPos.Height);
                spriteBatch.Draw(_texture, ScreenPos, null, Tint, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);

            }
            spriteBatch.End();

            _drawInfos.Clear();
            _pixelBuffer = new Color[Array.MaxLength];
            _CurrentBufferPlacement = 0;
        }
    }
}
