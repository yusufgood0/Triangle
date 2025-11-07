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
        void Enemy.Update(in Player player, in Random rnd, ref BoundingBox[] collisionObject, SeedMapper seedMap, int MapCellSize)
        {
            double TimeSinceLastJump = this.TimeSinceLastJump;

            _speed.Y += 1.5f;
            _speed.X *= 0.975f;
            _speed.Z *= 0.975f;
            _speed.Y *= 0.99f;
            _position += _speed;

            int heightAtPos = seedMap.HeightAtPosition(_position, MapCellSize);
            if (heightAtPos < _position.Y)
            {
                _position.Y = heightAtPos;
                _speed.Y = Math.Min(0, _speed.Y);
                onGround = true;
            }
            if (FlapCooldown - TimeSinceLastJump < 3)
            {
                FlapAtPlayer(player.Position, rnd);
            }

            FormModel(TimeSinceLastJump);

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
            Debug.WriteLine($"Slime hit! Health now {_health}");
        }
        BoundingBox Enemy.BoundingBox { get => _hitbox; }
        BoundingBox Enemy.Hitbox { get => _hitbox; }
        GenericModel[] Enemy.models { get => _model; }
        Vector3 Enemy.Position { get => _position; }
        int Enemy.Health { get => _health; }
        int Enemy.MaxHealth { get => MaxHealth; }
        int Enemy.Height { get => Size; }


        private const int JumpStrength = 35;
        private const int JumpMin = 50;
        private const int JumpMax = 150;
        private const int JumpPatternLength = 2;
        private (int, int, int) jumpInfo => (JumpMin, JumpMax, JumpStrength);
        private const int Size = 250;
        private const int Damage = 20;
        private const int MaxHealth = 125;
        private const int minHeal = 1;
        private const int maxHeal = 5;
        private const float FlapCooldown = 2;
        private const float SquishFactorUp = 200;
        private const float SquishFactorDown = -100;
        private const float SquishFactorNormal = 50;
        private const float knockBackMultiplier = 1f;

        private Vector3 _position;
        private Vector3 _speed;
        private bool onGround = false;
        private GenericModel[] _model = new GenericModel[1];
        private float _squish = SquishFactorNormal;
        private double TimeSinceLastJump => (DateTime.Now - JumpTimer).TotalSeconds;
        private int _health;
        private int _jumpPattern = 0;
        private BoundingBox _hitbox;
        private DateTime HealTimer = DateTime.Now;
        private DateTime JumpTimer = DateTime.Now;


        private void FormModel(double TimeSinceLastJump)
        {
            float height = Size + _squish;
            var newCube = new Cube(_position - new Vector3(Size / 2, height, Size / 2), Size, height, Size, Color.LightYellow);
            _model[0] = newCube;
            _hitbox = new BoundingBox(newCube.Position, newCube.Opposite);
        }
        private void FlapAtPlayer(Vector3 playerPos, in Random rnd)
        {
            Vector3 dir = playerPos - _position;

            _speed.Z += Math.Clamp(dir.Z * 0.05f, -10, 10);
            _speed.X += Math.Clamp(dir.X * 0.05f, -10, 10);
            _speed.Y += rnd.Next(-10, -30);
        }
        public Archer(Vector3 position)
        {
            _position = position;
            _speed = Vector3.Down * 25;
            _health = MaxHealth;
            FormModel(0);
        }

    }
}
