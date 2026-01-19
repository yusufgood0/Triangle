using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SlimeGame.Generation;
using SlimeGame.Models;

namespace SlimeGame.GameAsset.Projectiles
{
    internal struct FlameThrowerProjectile : Projectile
    {
        TargetType Projectile.TargetType => TargetType.Enemy;
        GenericModel Projectile.Model => _model.Move(_position);
        BoundingBox Projectile.HitBox => _hitBox;
        float Projectile.HitDamage => Damage;
        Vector3 Projectile.Velocity { get => _velocity; set => _velocity = value; }
        Vector3 Projectile.Position => _position;
        Color Projectile.Color => _model.Color;
        float Projectile.IFrameDuration => 1.5f;

        const float Damage = 0.05f;
        const int ModelDetail = 3;
        const int Radius = 30;
        const float HitboxSizeMultiplier = 20;
        const int HitBoxSize = (int)(Radius * HitboxSizeMultiplier);
        static GenericModel _model = new Sphere(Vector3.Zero, Radius, ModelDetail, Color.Red);
        static BoundingBox _hitBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
        Random _rnd;
        
        Vector3 _velocity;
        Vector3 _position;
        public FlameThrowerProjectile(Vector3 Position, Vector3 DirVector, float SpeedMultiplier, Random random)
        {
            this._rnd = random;
            this._position = Position;
            this._velocity = DirVector * SpeedMultiplier;
            this._velocity = General.RotateVector(_velocity, (float)_rnd.NextDouble() - 0.5f, (float)_rnd.NextDouble() - 0.5f);
        }

        public bool Move(ChunkManager chunkManager)
        {
            _velocity *= 0.975f;
            _position += _velocity;
            
            ReformHitbox();

            if (_velocity.Length() < 0.5f)
            {
                return true;
            }
            if (chunkManager.HeightAtPosition(_position) < _position.Y)
            {
                return true;
            }
            return false;
        }
        public void ReformHitbox()
        {
            _hitBox = new BoundingBox(_position - new Vector3(HitBoxSize), _position + new Vector3(HitBoxSize));
        }
        public void AddParticles(List<Particle> Particles, Random rnd)
        {
            _model.Color = new Color(rnd.Next(100, 255), rnd.Next(0, 60), 0);

            var particle = new Particle(_position, Color.Red * 0.7f, _velocity, rnd);
            particle.Float(rnd.Next(5, 50), rnd);

            Particles.Add(particle);

        }
        public Particle[] GetPixelDeathParticles(Random rnd, ObjectManager manager)
        {
            return new Particle[0];
        }
    }
}
