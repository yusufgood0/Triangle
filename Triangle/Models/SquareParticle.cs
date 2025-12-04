using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Models.Shapes;

namespace SlimeGame.Models
{
    internal struct SquareParticle
    {
        public DateTime CreationTime => _creationTime;
        public Triangle Triangle => new Triangle(_p1, _p2, _p3, Color);
        public Vector3 P1 => _p1;
        public Vector3 P2 => _p2;
        public Vector3 P3 => _p3;
        public Color Color => _color;

        //static Random rnd = new Random();

        DateTime _creationTime;
        Vector3 _generalVelocity;
        Vector3 _p1;
        Vector3 _p2;
        Vector3 _p3;
        Color _color;
        public SquareParticle(Vector3 P, Color color, Vector3 Velocity, Random rnd)
        {
            _p1 = P; _p2 = P; _p3 = P;
            _color = color;
            _generalVelocity = Velocity;
            _creationTime = DateTime.Now.AddSeconds((rnd.NextDouble()-0.5));
        }
        public SquareParticle(Vector3 P1, Vector3 P2, Vector3 P3, Color color, Vector3 Velocity)
        {
            this._p1 = P1; this._p2 = P2; this._p3 = P3;
            _color = color;
            _generalVelocity = Velocity;
            _creationTime = DateTime.Now;
        }
        public SquareParticle Float(int drift, Random rnd)
        {
            _p1 += _generalVelocity + new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
            _p2 += _generalVelocity + new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
            _p3 += _generalVelocity + new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
            return this;
        }
    }
}
