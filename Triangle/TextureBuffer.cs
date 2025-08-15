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
    internal class TextureBuffer
    {
        Color[] _pixels;
        int[] _layer;
        public ref Color[] Pixels { get => ref _pixels; }
        public ref int[] Layer { get => ref _layer; }
        int width { get; set; }
        int height { get; set; }
        public TextureBuffer(int width, int height, Color BackgroundColor)
        {
            this.width = width;
            this.height = height;

            _pixels = new Color[width * height];
            _layer = new int[width * height];

            Clear(BackgroundColor);
        }
        public void Clear(Color color)
        {
            Array.Fill(_pixels, color);
            Array.Fill(_layer, int.MaxValue);
        }

        public Texture2D ToTexture2D(GraphicsDevice graphicsDevice, out Texture2D texture)
        {
            texture = new Texture2D(graphicsDevice, width, height);
            texture.SetData(_pixels);
            return texture;
        }

    }
}
