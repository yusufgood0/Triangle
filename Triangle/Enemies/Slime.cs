using Microsoft.Xna.Framework;
using random_generation_in_a_pixel_grid;
using System;
using System.Collections.Generic;
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

            _speed.Y += 0.7f;
            _speed *= 0.975f;
            _position += _speed;

            int heightAtPos = seedMap.HeightAtPosition(_position, MapCellSize);
            if (heightAtPos < _position.Y)
            {
                _position.Y = heightAtPos;
                _speed.Y = Math.Min(0, _speed.Y);
                TimeSinceGroundTimer = DateTime.Now;

                if (
                    _squish <= -SquishFactorDown
                   )
                {
                    JumpAtPlayer(player.Position);
                    JumpTimer = DateTime.Now;
                    TimeSinceGroundTimer = DateTime.MinValue;
                }
                if (JumpCooldown - TimeSinceLastJump < 1)
                {
                    _squish = Math.Max(_squish - 5, -SquishFactorDown);
                }
            }
            else if (_speed.Y != 0 && (DateTime.Now - TimeSinceGroundTimer).TotalSeconds > 1)
            {
                if (_speed.Y > 0)
                {
                    _squish = Math.Min(_squish + 25, SquishFactorUp);
                }
                else
                {
                    _squish = Math.Max(_squish - 35, 0);
                }
            }

            FormModel(TimeSinceLastJump);

        }
        void Enemy.EnemyHitPlayer(ref Player player)
        {

        }
        void Enemy.EnemyIsHit(ref Player player)
        {

        }
        BoundingBox Enemy.BoundingBox { get => _hitbox; }
        BoundingBox Enemy.Hitbox { get => _hitbox; }
        Model[] Enemy.models { get => _model; }
        Vector3 Enemy.Position { get => _position; }



        private const int JumpStrength = 30;
        private const int JumpMin = 15;
        private const int JumpMax = 40;
        private (int, int, int) jumpInfo => (JumpMin, JumpMax, JumpStrength);
        private const int Size = 250;
        private const int MaxHealth = 125;
        private const int minHeal = 15;
        private const int maxHeal = 20;
        private const float JumpCooldown = 3;
        private const float SquishFactorUp = 150;
        private const float SquishFactorDown = 100;

        private Vector3 _position;
        private Vector3 _speed;
        private Model[] _model = new Model[1];
        private float _squish;
        private double TimeSinceLastJump => (DateTime.Now - JumpTimer).TotalSeconds;
        private int _health;
        private BoundingBox _hitbox;
        private DateTime HealTimer = DateTime.Now;
        private DateTime JumpTimer = DateTime.Now;
        private DateTime TimeSinceGroundTimer = DateTime.Now;



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

            _speed.Z += Math.Clamp(dir.Z * 0.025f, -800, 800);
            _speed.X += Math.Clamp(dir.X * 0.025f, -800, 800);
            _speed.Y += dir.Y * 0.075f;
        }
        public Slime(Vector3 position)
        {
            _position = position;
            _speed = Vector3.Down * 50;
            _health = MaxHealth;
            FormModel(0);
        }

    }
}
