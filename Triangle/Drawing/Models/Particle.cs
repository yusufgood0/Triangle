using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Drawing.Models.Shapes;

namespace SlimeGame.Drawing.Models
{
    internal struct Particle
    {
        [Flags]
        public enum FloatType
        {
            DefaultFloat = 1 << 1,
            BlackHole = 1 << 2,
        }

        public DateTime CreationTime => _creationTime;
        public Triangle Triangle => new Triangle(_p1, _p2, _p3, Color);
        public Vector3 P1 => _p1;
        public Vector3 P2 => _p2;
        public Vector3 P3 => _p3;
        public Color Color => _color;

        DateTime _creationTime;
        Vector3 _movementVector;
        Vector3 _p1;
        Vector3 _p2;
        Vector3 _p3;
        Color _color;
        readonly FloatType _floatType;
        public Particle(Vector3 P, Color color, Vector3 MovementVector, Random rnd, FloatType Type = FloatType.DefaultFloat)
        {
            _p1 = P; _p2 = P; _p3 = P;
            _color = color;
            _movementVector = MovementVector;
            _creationTime = Game1.PlayingGameTime;
            _floatType = Type;
        }
        public Particle(Vector3 P1, Vector3 P2, Vector3 P3, Color color, Vector3 MovementVector, FloatType Type = FloatType.DefaultFloat)
        {
            _p1 = P1; _p2 = P2; _p3 = P3;
            _color = color;
            _movementVector = MovementVector;
            _creationTime = Game1.PlayingGameTime;
            _floatType = Type;
        }
        public Particle Float(int drift, Random rnd)
        {
            switch (_floatType)
            {
                case FloatType.BlackHole:
                    _p1 += 5 * Vector3.Normalize(_movementVector - _p1);
                    _p2 += 5 * Vector3.Normalize(_movementVector - _p2);
                    _p3 += 5 * Vector3.Normalize(_movementVector - _p3);
                    break;
                case FloatType.DefaultFloat:

                    _p1 += _movementVector + new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
                    _p2 += _movementVector + new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
                    _p3 += _movementVector + new Vector3(rnd.Next(-drift, drift), rnd.Next(-drift, drift), rnd.Next(-drift, drift));
                    break;
            }
            return this;
        }
    }
}
