using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimeGame.Generation;
using SlimeGame.Models;
using SlimeGame.GameAsset;

namespace SlimeGame.Enemies
{
    internal class Archer : Enemy
    {
        enum state
        {
            Idle,
            Attacking,
            Recovering
        }
        void Enemy.Update(in Player player, in List<Projectile> projectiles, in Random rnd, SeedMapper seedMap, int MapCellSize)
        {
            double TimeSinceLastJump = this.TimeSinceLastJump;

            if (_currentState == state.Idle)
            {
                _speed.Y += 0.4f;

                _speed.X *= 0.975f;
                _speed.Z *= 0.975f;
            }
            else if (_currentState == state.Attacking)
            {
                _speed.Y += 5f;

                _speed.X *= 0.98f;
                _speed.Z *= 0.98f;
            }
            else if (_currentState == state.Recovering)
            {
                _speed.Y += 0.8f;

                _speed.X *= 0.975f;
                _speed.Z *= 0.975f;
            }

            _speed.Y *= 0.99f;
            _position += _speed;

            int heightAtPos = seedMap.HeightAtPosition(_position, MapCellSize);
            if (heightAtPos < _position.Y)
            {
                _position.Y = heightAtPos;
                _speed.Y = Math.Min(0, _speed.Y);
            }
            if (TimeSinceLastJump > FlapCooldown)
            {
                if (FlapPatternPos == FlapPatternLength)
                {
                    AttackPlayer(player.Position, rnd);
                    JumpTimer = DateTime.Now.AddSeconds(rnd.NextDouble());
                    FlapPatternPos = 0;
                    _currentState = state.Attacking;
                }
                else
                {
                    FlapAtPlayer(player.Position, rnd);
                    JumpTimer = DateTime.Now.AddSeconds(-rnd.NextDouble());
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

            FormModel(General.AngleBetween(player.Position, _position));

        }
        void Enemy.Knockback(Vector3 position)
        {
            var diff = _position - position;
            if (diff == Vector3.Zero) diff = Vector3.Up;
            _position += Vector3.Normalize(diff) * 1f;
        }
        void Enemy.EnemyHitPlayer(ref Player player)
        {

        }
        void Enemy.EnemyIsHit(ref Player player, Vector3 source, int amount)
        {
            _speed += Vector3.Normalize(_position - source) * knockBackMultiplier * amount;
            _health -= amount;
            Debug.WriteLine($"{this.ToString()} hit! Health now {_health}");
        }
        BoundingBox Enemy.BoundingBox { get => _hitbox; }
        BoundingBox Enemy.Hitbox { get => _hitbox; }
        GenericModel[] Enemy.models { get => _model; }
        Vector3 Enemy.Position { get => _position; }
        int Enemy.Health { get => _health; }
        int Enemy.MaxHealth { get => MaxHealth; }
        int Enemy.Height { get => Size; }


        private const int FlapPatternLength = 5;
        private const int Size = 250;
        private const int Damage = 20;
        private const int MaxHealth = 125;
        private const float FlapCooldown = 1;
        private const float knockBackMultiplier = 1f;

        private Vector3 _position;
        private Vector3 _speed;
        private int _health;

        private GenericModel[] _model = new GenericModel[1];
        private BoundingBox _hitbox;

        private double TimeSinceLastJump => (DateTime.Now - JumpTimer).TotalSeconds;
        private DateTime JumpTimer = DateTime.Now;
        private int FlapPatternPos = 0;
        private state _currentState = state.Idle;


        private void FormModel(Vector2 rotation)
        {
            float height = Size;
            var newCube = new Cube(_position - new Vector3(Size / 2, height, Size / 2), Size, height, Size, Color.LightYellow);
            _model[0] = newCube;
            _model[0].SetRotation(rotation);

            _hitbox = new BoundingBox(newCube.Position, newCube.Opposite);
        }
        private void AttackPlayer(Vector3 playerPos, in Random rnd)
        {
            Vector3 dir = playerPos - _position;
            _speed.Z += Math.Clamp(dir.Z * 0.1f, -40, 40);
            _speed.X += Math.Clamp(dir.X * 0.1f, -40, 40);
            _speed.Y += 15 + dir.Y * 0.01f;
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
        public Archer(Vector3 position)
        {
            _position = position;
            _speed = Vector3.Down * 25;
            _health = MaxHealth;
            FormModel(Vector2.Zero);
        }

    }
}
