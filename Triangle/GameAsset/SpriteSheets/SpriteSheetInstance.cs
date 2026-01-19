using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.GameAsset.SpriteSheets
{
    internal class SpriteSheetInstance
    {
        public (int X, int Y) CurrentCell;
        public (int X, int Y) DrawPosition;
        public bool IsVisible = true;
        public float Brightness = 1.0f;
        public Color ColorMask = Color.White;
        public SpriteSheetInstance((int X, int Y) drawPosition, (int, int)? currentCell = null)
        {
            DrawPosition = drawPosition;
            CurrentCell = currentCell ?? (0, 0);
        }

    }
}
