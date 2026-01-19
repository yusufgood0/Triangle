using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.GameAsset;

namespace SlimeGame.Models.Shapes
{
    internal interface Shape
    {
        Vector3 Position { get; }
        Color Color { get; set; }
        public unsafe void Draw(
            ref TextureBuffer screenBuffer,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            int distance,
            Color color
            )
        {
            // debug.writeline("this is interface Code, It Should not run");
        }
        public Color ApplyShading(Vector3 lightDirection, Color triangleColor, Color lightColor)
        {
            return default;
        }
    }
}
