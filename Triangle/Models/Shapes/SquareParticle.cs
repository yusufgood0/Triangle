using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Models.Shapes
{
    internal struct SquareParticle
    {
        public DateTime CreationTime => _creationTime;
        public Square Square => new Square(P1, P2, P3, P4, Color);
        public Color Color => _color;

        static Random rnd = new Random();

        DateTime _creationTime;
        Vector3 _generalVelocity;
        Vector3 P1;
        Vector3 P2;
        Vector3 P3;
        Vector3 P4;
        Square _square;
        Color _color;
        public SquareParticle(Vector3 P, Color color, Vector3 Velocity)
        {
            P1 = P; P2 = P; P3 = P; P4 = P;
            _color = color;
            _generalVelocity = Velocity;
            _creationTime = DateTime.Now;
        }
        public SquareParticle(Vector3 P1, Vector3 P2, Vector3 P3, Vector3 P4, Color color, Vector3 Velocity)
        {
            this.P1 = P1; this.P2 = P2; this.P3 = P3; this.P4 = P4;
            _color = color;
            _generalVelocity = Velocity;
            _creationTime = DateTime.Now;
        }
        public SquareParticle Float(int drift)
        {
            P1 += _generalVelocity + new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
            P2 += _generalVelocity + new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
            P3 += _generalVelocity + new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
            P4 += _generalVelocity + new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
            return this;
        }
    }
}
