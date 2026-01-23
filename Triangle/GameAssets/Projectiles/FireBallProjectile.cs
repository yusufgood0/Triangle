using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SlimeGame.Drawing.Models;
using SlimeGame.Generation;

namespace SlimeGame.GameAsset.Projectiles
{
    internal class FireBallProjectile : Projectile
    {
        public override TargetType TargetType => TargetType.Enemy;
        public override GenericModel Model => _model.Move(_position);
        public override BoundingBox HitBox => _hitBox;
        public override float HitDamage => _damage;
        public override Vector3 Velocity { get => _velocity; set => _velocity = value; }
        public override Vector3 Position => _position;
        public override Color Color => _model.Color;
        public override float IFrameDuration => -1f;
        public override float Volume => 0.2f;

        const int Damage = 100;
        const int ModelDetail = 3;
        const int Radius = 400;
        const int ExplosionSize = 100;
        const float HitboxSizeMultiplier = 1.5f;
        const int HitBoxSize = (int)(Radius * HitboxSizeMultiplier);

        static GenericModel _model = new Sphere(Vector3.Zero, Radius, ModelDetail, Color.Red);
        static BoundingBox _hitBox = new BoundingBox(Vector3.Zero, Vector3.Zero);

        Vector3 _velocity;
        Vector3 _position;
        int _damage;
        public FireBallProjectile(Vector3 Position, Vector3 DirVector, float SpeedMultiplier, float damageMultiplier)
        {
            _velocity = DirVector * SpeedMultiplier;
            _position = Position;
            _damage = (int)(Damage * damageMultiplier);
        }
        public override bool Move(ChunkManager seedMapper)
        {
            _velocity *= 1.025f;
            _position += _velocity;
            ReformHitbox();
            _model.ChangeRotation(0.1f, 0.2f);
            if (seedMapper.HeightAtPosition(_position) < _position.Y)
            {
                return true;
            }
            return false;
        }
        public void ReformHitbox()
        {
            _hitBox = new BoundingBox(_position - new Vector3(HitBoxSize), _position + new Vector3(HitBoxSize));
        }
        public override void AddParticles(List<Particle> Particles, Random rnd)
        {
            _model.Color = new Color(rnd.Next(100, 255), rnd.Next(0, 60), 0);

            var particle = new Particle(_position, _model.Color, _velocity, rnd);
            particle.Float(rnd.Next(5, 50), rnd);

            Particles.Add(particle);
        }
        public override Particle[] GetPixelDeathParticles(Random rnd, AssetManager manager)
        {
            if (Position.Y < manager.ChunkManager.HeightAtPosition(_position) - 30)
            {
                manager.ChunkManager.CreateCrater(Position, (int)(Math.Clamp(_velocity.Length() / 60, 3, 30) * 1.25f), -1200, _model.Color);
                manager.ChunkManager.CreateCrater(Position, (int)Math.Clamp(_velocity.Length() / 60, 3, 30), 1500, _model.Color);
            }

            var returnValue = new Particle[rnd.Next(20, 50)];
            for (int i = 0; i < returnValue.Count(); i++)
            {
                Vector3 particlePos = _position + new Vector3(rnd.Next(-ExplosionSize, ExplosionSize), rnd.Next(-ExplosionSize, ExplosionSize), rnd.Next(-ExplosionSize, ExplosionSize));
                returnValue[i] = new Particle(particlePos, _model.Color, (particlePos - _position) / 10, rnd);
                returnValue[i].Float(200, rnd);
            }
            return returnValue;
        }
    }
}
