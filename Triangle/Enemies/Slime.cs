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
            if ((JumpTimer - DateTime.Now).TotalSeconds > 8 &&
                _speed.Y == 0 &&
                Enemy.CheckLineOfSight(_position, player.Position, ref collisionObject)
                )
            {
                JumpTimer = DateTime.Now;

                Vector3 dir = player.Position - _position;
                dir.Y = 0;
                dir.Normalize();
                _speed += dir * 10;
                _speed.Y = -15;
            }
            _position += _speed;
            int heightAtPos = seedMap.HeightAtPosition(_position, MapCellSize);
            if (heightAtPos < _position.Y)
            {
                _position.Y = heightAtPos; 
                _speed.Y = 0;
            }

        }
        void Enemy.EnemyHitPlayer(ref Player player)
        {

        }
        void Enemy.EnemyIsHit(ref Player player)
        {

        }
        BoundingBox Enemy.BoundingBox { get; }
        BoundingBox Enemy.Hitbox { get; }
        Model[] Enemy.models { get; }
        Vector3 Enemy.Position { get => _position; }

        private void CheckHeal(in Random rnd)
        {
            if ((HealTimer - DateTime.Now).TotalSeconds > 2)
            {
                HealTimer = DateTime.Now;
                _health = Math.Min(_health + rnd.Next(minHeal, maxHeal), MaxHealth);
            }
        }

        private const int Size = 25;
        private const int MaxHealth = 125;
        private const int minHeal = 15;
        private const int maxHeal = 20;

        private Vector3 _position;
        private Vector3 _speed;
        private int _health;
        private DateTime HealTimer = DateTime.Now;
        private DateTime JumpTimer = DateTime.Now;

        public Slime(Vector3 position)
        {
            _position = position;
            _speed = Vector3.Zero;
            _health = MaxHealth;
        }

    }
}
