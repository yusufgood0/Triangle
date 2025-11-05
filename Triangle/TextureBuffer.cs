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
    internal class TextureBuffer
    {
        Color[] _pixels;
        int[] _distance;
        public ref Color[] Pixels { get => ref _pixels; }
        public ref int[] Distance { get => ref _distance; }
        public int width { get; set; }
        public int height { get; set; }
        public TextureBuffer(int width, int height, Color BackgroundColor)
        {
            this.width = width;
            this.height = height;

            _pixels = new Color[width * height];
            _distance = new int[width * height];

            Clear(BackgroundColor);
        }
        public void Clear(Color color)
        {
            Array.Fill(_pixels, color);
            Array.Fill(_distance, int.MaxValue);
        }
        public unsafe void applyDepth(int strength)
        {
            fixed (Color* screenBufferColorPtr = _pixels)
            fixed (int* screenBufferDistancePtr = _distance)
            {
                for (int i = 0; i < _pixels.Length; i++)
                {
                    if (screenBufferDistancePtr[i] == int.MaxValue) { continue; }
                    screenBufferColorPtr[i] = Color.Lerp(Color.Black, screenBufferColorPtr[i], 1f / (screenBufferDistancePtr[i] / strength));
                }
            }
        }
        public Texture2D ToTexture2D(GraphicsDevice graphicsDevice, out Texture2D texture)
        {
            texture = new Texture2D(graphicsDevice, width, height);
            texture.SetData(_pixels);
            return texture;
        }

    }
}
