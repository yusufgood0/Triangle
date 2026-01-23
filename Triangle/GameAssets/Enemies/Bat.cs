using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.Drawing.Models;
using SlimeGame.GameAsset;
using SlimeGame.GameAsset.IFrames;
using SlimeGame.GameAsset.Projectiles;
using SlimeGame.Generation;

namespace SlimeGame.GameAssets.Enemies
{
    internal class Bat : Enemy
    {
        enum state
        {
            Idle,
            Attacking,
            Recovering,
            WindingUpAttack
        }
        public override void Update(AssetManager objectManager, Random rnd)
        {

            switch (_currentState)
            {
                case state.Idle:
                    _speed.Y += 0.8f;
                    _speed.X *= 0.99f;
                    _speed.Z *= 0.99f;
                    break;
                case state.Attacking:
                    _speed *= 0.95f;
                    Vector3 playerDir = objectManager.Player.Position - _position;
                    _speed = Vector3.Lerp(_speed, playerDir, 0.0035f);
                    break;
                case state.Recovering:
                    _speed.Y += 0.8f;
                    _speed.X *= 0.975f;
                    _speed.Z *= 0.975f;
                    break;
                case state.WindingUpAttack:
                    _speed = Vector3.Zero;
                    break;
            }

            _position += _speed;

            float heightAtPos = objectManager.ChunkManager.HeightAtPosition(_position);
            if (heightAtPos < _position.Y)
            {
                _position.Y = heightAtPos;
                _speed.Y = Math.Min(0, _speed.Y);
                _speed.X *= 0.8f;
                _speed.Z *= 0.8f;
            }
            if (TimeSinceLastMove > FlapCooldown)
            {
                if (_currentState == state.WindingUpAttack)
                {
                    AttackPlayer(rnd);
                    MovementTimer = Game1.PlayingGameTime.AddSeconds(rnd.NextDouble());
                    _currentState = state.Attacking;
                }
                else if (FlapPatternPos == FlapPatternLength)
                {
                    _dashTarget = objectManager.Player.Position;
                    MovementTimer = Game1.PlayingGameTime.AddSeconds(rnd.NextDouble());

                    int Variance = Size * 2;
                    for (int i = 0; i < 15; i++)
                        objectManager.Particles.Add(new Particle(
                        _position + new Vector3(rnd.Next(-Variance, Variance), rnd.Next(-Variance, Variance), rnd.Next(-Variance, Variance)),
                        _position + new Vector3(rnd.Next(-Variance, Variance), rnd.Next(-Variance, Variance), rnd.Next(-Variance, Variance)),
                        _position + new Vector3(rnd.Next(-Variance, Variance), rnd.Next(-Variance, Variance), rnd.Next(-Variance, Variance)),
                        Color.LightYellow * (float)rnd.NextDouble(),
                        _position,
                        Particle.FloatType.BlackHole
                        ));
                    _currentState = state.WindingUpAttack;
                    FlapPatternPos = 0;
                }
                else
                {
                    FlapAtPlayer(objectManager.Player.Position, rnd);
                    MovementTimer = Game1.PlayingGameTime.AddSeconds(FlapCooldown * -rnd.NextDouble());
                    FlapPatternPos++;
                    if (_currentState == state.Attacking)
                    {
                        _currentState = state.Recovering;
                    }
                    else
                    {
                        _currentState = state.Idle;
                    }
                }

            }

            FormModel();
            base.Update(objectManager, rnd);
        }
        public override bool EnemyIsHit(float amount, AssetManager objectManager)
        {

            if (MaxHealth * 0.1f < amount)
            {
                if (_currentState == state.Attacking) {
                    _currentState = state.Recovering;
                }
                FlapPatternPos = 1;
            }
            return base.EnemyIsHit(amount, objectManager);
        }
        public override void SetPosition(Vector3 vector)
        {
            _position = vector;
            _speed = Vector3.Zero;
            FormModel();
        }
        public override BoundingBox BoundingBox { get => _hitbox; }
        public override BoundingBox Hitbox { get => _hitbox; }
        public override BoundingBox Hurtbox { get => _hurtbox; }
        public override GenericModel[] models { get => _model; }
        public override Vector3 Position { get => _position; }
        public override float Health { get => _health; }
        public override int Height { get => Size; }
        public override IFrameType iFrameType => IFrameType.Universal;
        protected override int MaxHealth => _maxHealth;
        protected override float KnockbackMultiplier => 2f;
        protected override float Damage => 0.75f;
        protected override float IFrameDuration => 1f;

        private const int FlapPatternLength = 2;
        private const int Size = 250;
        private const int _maxHealth = 200;
        private const float FlapCooldown = .7f;
        private const float paddingPercent = -0.25f;

        private Vector3 _dashTarget;
        private int FlapPatternPos = 0;
        private state _currentState = state.Idle;


        private void FormModel()
        {
            float height = Size;
            var newCube = new Cube(_position - new Vector3(Size / 2, height, Size / 2), Size, height, Size, Color.LightYellow);
            Vector3 Padding = new Vector3(paddingPercent * Size);

            _model[0] = newCube;
            _model[0].SetRotation(newCube.Center, Rotation);

            _hitbox = new BoundingBox(newCube.Position, newCube.Opposite);
            _hurtbox = new BoundingBox(newCube.Position + Padding, newCube.Opposite - Padding);
        }
        private void AttackPlayer(in Random rnd)
        {
            Vector3 dir = _dashTarget - _position;
            _speed += dir * 0.15f;
            _speed.Y += 15;
        }
        private void FlapAtPlayer(Vector3 playerPos, in Random rnd)
        {
            Vector3 dir = playerPos - _position;
            dir.Z += rnd.Next(-1000, 1000);
            dir.X += rnd.Next(-1000, 1000);
            _speed.Z += dir.Z * 0.01f;
            _speed.X += dir.X * 0.01f;
            _speed.Y -= rnd.Next(15, 40) - dir.Y * 0.01f;
        }
        public Bat(Vector3 position)
        {
            _position = position;
            _speed = Vector3.Down * 25;
            _health = MaxHealth;
            FormModel();
        }

    }
}
