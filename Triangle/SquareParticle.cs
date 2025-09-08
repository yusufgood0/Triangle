using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Triangle
{
    internal struct SquareParticle
    {
        static Random rnd = new Random();
        public Square Square => _square;
        public DateTime CreationTime => _creationTime;
        public Color Color => _color;
        DateTime _creationTime = DateTime.Now;
        Vector3 _generalVelocity;
        Square _square;
        Color _color;
        public SquareParticle(Vector3 P, Color color, Vector3 Velocity)
        {
            _color = color;
            _square = new Square(P, P, P, P);
            _generalVelocity = Velocity;
        }
        public SquareParticle(Vector3 P1, Vector3 P2, Vector3 P3, Vector3 P4, Color color, Vector3 Velocity)
        {
            _color = color;
            _square = new Square(P1, P2, P3, P4);
            _generalVelocity = Velocity;
        }
        public void Float(int drift)
        {
            for (int i = 0; i < 4; i++)
            {
                Square.Vertices[i] += _generalVelocity;
                Square.Vertices[i] += new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
            }
        }
    }
}
