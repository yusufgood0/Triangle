using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeGame
{
    internal ref struct RefSlider
    {
        private Slider _slider;
        public float Percent
        {
            get => _slider.Percent;
            set => _slider.Percent = value;
        }
        public RefSlider(ref Slider slider)
        {
            _slider = slider;
        }
        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Microsoft.Xna.Framework.Graphics.Texture2D pixel)
        {
            _slider.Draw(spriteBatch, pixel);
        }
    }
}
