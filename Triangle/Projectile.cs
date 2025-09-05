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

        public Model Model { get; }
        public Vector3 Velocity { get; }
        public Vector3 Position { get; }
        public Color Color { get; }
        public void Move()
        {

        }
    }
    internal class FireBolt(Vector3 Position, Vector3 DirVector) : Projectile
    {
        Model Projectile.Model => _model.Move(_position);
        Vector3 Projectile.Velocity => _velocity;
        Vector3 Projectile.Position => _position;
        Color Projectile.Color => _colors[_colorState];

        static Model _model = new Sphere(Vector3.Zero, 32, 5);
        Vector3 _velocity = DirVector;
        Vector3 _position = Position;
        static Color[] _colors = new Color[] { Color.Red, Color.OrangeRed};
        int _colorState = 0;

        public void Move()
        {
            _colorState = (_colorState + 1) % _colors.Length;
            _velocity *= 1.05f;
            _position += _velocity;
        }
        
    }
}
