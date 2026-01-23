using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SlimeGame.Drawing.Models;
using SlimeGame.Generation;

namespace SlimeGame.GameAsset.Projectiles
{
    internal class FireBurstProjectile : Projectile
    {
        public override TargetType TargetType => TargetType.Enemy;
        public override GenericModel Model => _model.Move(_position);
        public override BoundingBox HitBox => _hitBox;
        public override float HitDamage => Damage;
        public override Vector3 Velocity { get => _velocity; set => _velocity = value; }
        public override Vector3 Position => _position;
        public override Color Color => _model.Color;
        public override float IFrameDuration => -1;
        public override float Volume => 0.075f;

        const float Damage = 5;
        const int ModelDetail = 3;
        const int Radius = 50;
        const float HitboxSizeMultiplier = 5;
        const int HitBoxSize = (int)(Radius * HitboxSizeMultiplier);
        static GenericModel _model = new Sphere(Vector3.Zero, Radius, ModelDetail, Color.Purple);
        static BoundingBox _hitBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
        static Random _rnd = new Random();

        Vector3 _velocity;
        Vector3 _position;
        public FireBurstProjectile(Vector3 Position, Vector3 DirVector, float SpeedMultiplier)
        {
            this._position = Position;
            this._velocity = DirVector * SpeedMultiplier;
            _velocity = General.RotateVector(_velocity, (float)_rnd.NextDouble() - 0.5f, (float)_rnd.NextDouble() - 0.5f);
        }

        public override bool Move(ChunkManager chunkManager)
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
        public override void AddParticles(List<Particle> Particles, Random rnd)
        {
            _model.Color = new Color(rnd.Next(80, 200), rnd.Next(30, 30), rnd.Next(0, 30), rnd.Next(0, 255));

            var particle = new Particle(_position, _model.Color * 0.7f, _velocity, rnd);
            particle.Float(rnd.Next(20, 80), rnd);

            Particles.Add(particle);
        }
        public override Particle[] GetPixelDeathParticles(Random rnd, AssetManager manager)
        {
            return new Particle[0];
        }
    }
}
