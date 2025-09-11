using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using random_generation_in_a_pixel_grid;

namespace Triangle
{
    internal interface Projectile
    {

        public Model Model { get; }
        public Vector3 Velocity { get; }
        public Vector3 Position { get; }
        public Color Color { get; }
        public bool Move(SeedMapper seedMapper, int MapCellSize)
        {
            return true; // returns true when you should get rid of projectile
        }
        public SquareParticle? GetParticles(Random rnd)
        {
            return null;
        }
        public SquareParticle[] HitGround(Random rnd)
        {
            return null;
        }
    }
    internal class FireBallProjectile(Vector3 Position, Vector3 DirVector, float SpeedMultiplier, float damageMultiplier) : Projectile
    {
        Model Projectile.Model => _model.Move(_position);
        Vector3 Projectile.Velocity => _velocity;
        Vector3 Projectile.Position => _position;
        Color Projectile.Color => _colorState;

        static Model _model = new Sphere(Vector3.Zero, 32, 5);
        static Color[] _colors = new Color[] { Color.DarkRed, Color.Orange};
        Vector3 _velocity = DirVector * SpeedMultiplier;
        Vector3 _position = Position;
        Color _colorState = Color.Red;
        static int _explosionSize = 100;
        public bool Move(SeedMapper seedMapper, int MapCellSize)
        {
            _velocity *= 1.05f;
            _position += _velocity;
            if (seedMapper.HeightAtPosition(_position, MapCellSize) < _position.Y)
            {
                return true;
            }
            return false;
        }
        public SquareParticle? GetParticles(Random rnd)
        {
            _colorState = new Color(rnd.Next(100, 255), rnd.Next(0, 60), 0);

            Color color = _colorState;
            var particle = new SquareParticle(_position, color, _velocity);
            particle.Float(rnd.Next(5, 50));
            return particle;
        }
        public SquareParticle[] HitGround(Random rnd)
        {
            var returnValue = new SquareParticle[rnd.Next(20, 50)];
            for (int i = 0; i < returnValue.Count(); i++)
            {
                Vector3 particlePos = _position + new Vector3(rnd.Next(-_explosionSize, _explosionSize), rnd.Next(-_explosionSize, _explosionSize), rnd.Next(-_explosionSize, _explosionSize));
                returnValue[i] = new SquareParticle(particlePos, _colorState, (particlePos - _position) / 10);
                returnValue[i].Float(200);
            }
            return returnValue;

        }
    }
}
