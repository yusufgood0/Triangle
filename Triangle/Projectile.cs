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
    internal interface Projectile
    {
        public Vector3 Velocity { get; set; }
        public Vector3 Position { get; set; }
        public Color Color { get; set; }
    }
    internal class FireBolt : Projectile
    {
        Vector3 Projectile.Velocity { get; set; }
        Vector3 Projectile.Position { get; set; }
        Color Projectile.Color { get; set; }
        Vector3 _velocity;
        Vector3 _position;
        Color _color;

        public FireBolt(Vector3 Position, Vector3 DirVector)
        {
            _velocity = DirVector;
            _position = Position;
            _color = Color.Red;
        } 
    }
}
