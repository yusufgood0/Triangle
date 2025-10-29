using Microsoft.Xna.Framework;
using random_generation_in_a_pixel_grid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triangle.Enemies
{
    internal class Slime : Enemy
    {
        void Enemy.Update(in Player player, in Random rnd, ref BoundingBox[] collisionObject, SeedMapper seedMap, int MapCellSize)
        {
            CheckHeal(in rnd);
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
            
            if (onGround)
            {
                if (JumpCooldown - TimeSinceLastJump < 1)
                {
                    _squish = Math.Max(_squish - MathF.Pow(MathF.Abs(SquishFactorDown - _squish), 0.7f) * 0.5f, SquishFactorDown);
                    if (_squish <= SquishFactorDown)
                    {
                        if (Vector3.DistanceSquared(_position, player.Position) > 400)
                        {
                            JumpAtPlayer(player.Position);
                        }
                        else if (_jumpPattern == JumpPatternLength)
                        {
                            JumpAtPlayer(player.Position, JumpMax * 3, JumpStrength);
                            _jumpPattern = 0;
                        }
                        else
                        {
                            _jumpPattern++;
                            JumpAtPlayer(player.Position, rnd.Next(JumpMin, JumpMax), JumpStrength);
                        }
                        onGround = false;
                        JumpTimer = DateTime.Now;
                    }
                }
            }
            else
            {
                _squish = Math.Clamp(_speed.Y * 5, SquishFactorDown, SquishFactorUp);
            }

            FormModel(TimeSinceLastJump);

        }
        void Enemy.Bounce(Vector3 position)
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
        Model[] Enemy.models { get => _model; }
        Vector3 Enemy.Position { get => _position; }
        int Enemy.Health { get => _health; }



        private const int JumpStrength = 35;
        private const int JumpMin = 5;
        private const int JumpMax = 10;
        private const int JumpPatternLength = 2;
        private (int, int, int) jumpInfo => (JumpMin, JumpMax, JumpStrength);
        private const int Size = 250;
        private const int Damage = 20;
        private const int MaxHealth = 125;
        private const int minHeal = 15;
        private const int maxHeal = 20;
        private const float JumpCooldown = 2;
        private const float SquishFactorUp = 200;
        private const float SquishFactorDown = -100;
        private const float SquishFactorNormal = 50;
        private const float knockBackMultiplier = 1f;

        private Vector3 _position;
        private Vector3 _speed;
        private bool onGround = false;
        private Model[] _model = new Model[1];
        private float _squish = SquishFactorNormal;
        private double TimeSinceLastJump => (DateTime.Now - JumpTimer).TotalSeconds;
        private int _health;
        private int _jumpPattern = 0;
        private BoundingBox _hitbox;
        private DateTime HealTimer = DateTime.Now;
        private DateTime JumpTimer = DateTime.Now;


        private void CheckHeal(in Random rnd)
        {
            if ((DateTime.Now - HealTimer).TotalSeconds > 2)
            {
                HealTimer = DateTime.Now;
                _health = Math.Min(_health + rnd.Next(minHeal, maxHeal), MaxHealth);
            }
        }
        private void FormModel(double TimeSinceLastJump)
        {
            float height = Size + _squish;
            var newCube = new Cube(_position - new Vector3(Size / 2, height, Size / 2), Size, height, Size);
            _model[0] = newCube;
            _hitbox = new BoundingBox(newCube.Position, newCube.Opposite);
        }
        private void JumpAtPlayer(Vector3 playerPos, Random rnd, (int Min, int Max, int Strength) info)
            => JumpAtPlayer(playerPos, rnd.Next(info.Min, info.Max), info.Strength);
        private void JumpAtPlayer(Vector3 playerPos, int jumpheight, int jumpStrength)
        {
            Vector3 dir = playerPos - _position;
            dir.Y = 0;
            dir.Normalize();
            dir *= jumpStrength;
            dir.Y = -jumpheight;
            _speed += dir;
        }
        private void JumpAtPlayer(Vector3 playerPos)
        {
            Vector3 dir = playerPos - _position;

            _speed.Z += Math.Clamp(dir.Z * 0.05f, -50, 50);
            _speed.X += Math.Clamp(dir.X * 0.05f, -50, 50);
            _speed.Y += dir.Y * 0.2f - JumpMin;
        }
        public Slime(Vector3 position)
        {
            _position = position;
            _speed = Vector3.Down * 25;
            _health = MaxHealth;
            FormModel(0);
        }

    }
}
