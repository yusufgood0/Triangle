using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SlimeGame.Drawing.Models;
using SlimeGame.Generation;

namespace SlimeGame.GameAsset.Projectiles
{
    internal class MeteorProjectile : Projectile
    {
        public override TargetType TargetType => TargetType.Enemy;
        public override GenericModel Model => _model.Move(_position);
        public override BoundingBox HitBox => _hitBox;
        public override float HitDamage => Damage;
        public override Vector3 Velocity { get => _velocity; set => _velocity = value; }
        public override Vector3 Position => _position;
        public override Color Color => _model.Color;
        public override float IFrameDuration => -1;
        public override float Volume => 0.15f;

        const int MaxDamage = 25;
        const int ModelDetail = 5;
        const int MaxRadius = 800;
        const int ExplosionSize = 100;
        const float gravity = 3f;
        const float HitboxSizeMultiplier = 1.5f;
        int HitBoxSize => (int)(Radius * HitboxSizeMultiplier);
        float Radius => MaxRadius * _size;
        float Damage => MaxDamage * _size;

        static BoundingBox _hitBox = new BoundingBox(Vector3.Zero, Vector3.Zero);

        GenericModel _model;
        Vector3 _velocity;
        Vector3 _position;
        float _size;
        public MeteorProjectile(Vector3 Position, Vector3 DirVector, float SpeedMultiplier, float Size)
        {
            _velocity = DirVector * SpeedMultiplier;
            _position = Position;
            _size = Size;
            _model = new Sphere(Vector3.Zero, Radius, ModelDetail, SpellBook.GetElementColor(Element.Earth));
        }
        public override bool Move(ChunkManager seedMapper)
        {
            _velocity *= 0.99f;
            _velocity.Y += gravity;
            _position += _velocity;
            _model.Color = Color.Lerp(_model.Color, Color.DarkRed, 0.05f);
            _model.Move(_position);
            ReformHitbox();
            _model.ChangeRotation(0.4f, 0.1f);
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
            Vector3 position = General.RandomVector3(rnd) * MaxRadius;

            var particle = new Particle(position, _model.Color, _velocity, rnd);

            particle.Float(rnd.Next(5, 50), rnd);

            Particles.Add(particle);
        }
        public override Particle[] GetPixelDeathParticles(Random rnd, AssetManager assetManager)
        {
            assetManager.ChunkManager.CreateCrater(Position, (int)(Math.Clamp(_velocity.Length() / 60, 3, 30) * 1.25f), -1200, _model.Color);  
            assetManager.ChunkManager.CreateCrater(Position, (int)Math.Clamp(_velocity.Length() / 60, 3, 30), 1500, _model.Color);  

            //spawns more projectiles
            if (Radius > MaxRadius)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 angle;
                    angle.X = (float)rnd.NextDouble() * MathHelper.TwoPi;
                    angle.Y = (float)rnd.NextDouble() * 0.3f - MathHelper.Pi / 2;
                    Vector3 dir = General.angleToVector3(angle);
                    Projectile projectile = new MeteorProjectile(
                        _position,
                        dir,
                        _velocity.Length() / 3f,
                        _size * (float)rnd.NextDouble() /2
                        );
                    assetManager.Add(projectile);
                }
            }
            // Pixels
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
