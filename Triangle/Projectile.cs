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
        public TargetType TargetType { get; }
        public Model Model { get; }
        public BoundingBox HitBox { get; }
        public int HitDamage { get; }
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
    public enum TargetType
    {
        Enemy = 0,
        Player = 1,
    }
    internal class FireBallProjectile(Vector3 Position, Vector3 DirVector, float SpeedMultiplier, float damageMultiplier) : Projectile
    {
        TargetType Projectile.TargetType => TargetType.Enemy;
        Model Projectile.Model => _model.Move(_position);
        BoundingBox Projectile.HitBox => _hitBox;
        int Projectile.HitDamage => (int)(Damage * damageMultiplier);
        Vector3 Projectile.Velocity => _velocity;
        Vector3 Projectile.Position => _position;
        Color Projectile.Color => _colorState;
        
        const int Damage = 20;
        const int ModelDetail = 5;
        const int Radius = 32;
        const int ExplosionSize = 100;
        const float HitboxSizeMultiplier = 1.5f;
        const int HitBoxSize = (int)(Radius*HitboxSizeMultiplier);
        static Model _model = new Sphere(Vector3.Zero, Radius, ModelDetail);
        static BoundingBox _hitBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
        static Color[] _colors = new Color[] { Color.DarkRed, Color.Orange};
        Vector3 _velocity = DirVector * SpeedMultiplier;
        Vector3 _position = Position;
        Color _colorState = Color.Red;
        public bool Move(SeedMapper seedMapper, int MapCellSize)
        {
            _velocity *= 1.05f;
            _position += _velocity;
            ReformHitbox();
            if (seedMapper.HeightAtPosition(_position, MapCellSize) < _position.Y)
            {
                return true;
            }
            return false;
            
        }
        public void ReformHitbox()
        {
            _hitBox = new BoundingBox(_position - new Vector3(HitBoxSize), _position + new Vector3(HitBoxSize));
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
                Vector3 particlePos = _position + new Vector3(rnd.Next(-ExplosionSize, ExplosionSize), rnd.Next(-ExplosionSize, ExplosionSize), rnd.Next(-ExplosionSize, ExplosionSize));
                returnValue[i] = new SquareParticle(particlePos, _colorState, (particlePos - _position) / 10);
                returnValue[i].Float(200);
            }
            return returnValue;

        }
    }
}
