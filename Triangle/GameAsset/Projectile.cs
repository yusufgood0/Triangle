using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Generation;
using SlimeGame.Models;

namespace SlimeGame.GameAsset
{
    internal interface Projectile
    {
        public TargetType TargetType { get; }
        public GenericModel Model { get; }
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
        GenericModel Projectile.Model => _model.Move(_position);
        BoundingBox Projectile.HitBox => _hitBox;
        int Projectile.HitDamage => (int)(Damage * damageMultiplier);
        Vector3 Projectile.Velocity => _velocity;
        Vector3 Projectile.Position => _position;
        Color Projectile.Color => _model.Color;
        
        const int Damage = 25;
        const int ModelDetail = 3;
        const int Radius = 160;
        const int ExplosionSize = 100;
        const float HitboxSizeMultiplier = 1.5f;
        const int HitBoxSize = (int)(Radius*HitboxSizeMultiplier);
        static GenericModel _model = new Sphere(Vector3.Zero, Radius, ModelDetail, Color.Red);
        static BoundingBox _hitBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
        static Color[] _colors = new Color[] { Color.DarkRed, Color.Orange};
        Vector3 _velocity = DirVector * SpeedMultiplier;
        Vector3 _position = Position;
        public bool Move(SeedMapper seedMapper, int MapCellSize)
        {
            _velocity *= 1.05f;
            _position += _velocity;
            ReformHitbox();
            _model.ChangeRotation(0.1f, 0.2f);
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
            _model.Color = new Color(rnd.Next(100, 255), rnd.Next(0, 60), 0);

            var particle = new SquareParticle(_position, _model.Color, _velocity, rnd);
            particle.Float(rnd.Next(5, 50), rnd);
            return particle;
        }
        public SquareParticle[] HitGround(Random rnd)
        {
            var returnValue = new SquareParticle[rnd.Next(20, 50)];
            for (int i = 0; i < returnValue.Count(); i++)
            {
                Vector3 particlePos = _position + new Vector3(rnd.Next(-ExplosionSize, ExplosionSize), rnd.Next(-ExplosionSize, ExplosionSize), rnd.Next(-ExplosionSize, ExplosionSize));
                returnValue[i] = new SquareParticle(particlePos, _model.Color, (particlePos - _position) / 10, rnd);
                returnValue[i].Float(200, rnd);
            }
            return returnValue;
        }
    }
}
