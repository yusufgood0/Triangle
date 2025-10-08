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

            if (TimeSinceLastJump > JumpCooldown &&
                _speed.Y == 0 &&
                Enemy.CheckLineOfSight(_position, player.Position, ref collisionObject)
                )
            {
                JumpTimer = DateTime.Now;
                JumpAtPlayer(player.Position, rnd.Next(15, 50));
            }

            _speed.Y += 0.3f;
            _position += _speed;

            int heightAtPos = seedMap.HeightAtPosition(_position, MapCellSize);
            if (heightAtPos < _position.Y)
            {
                _position.Y = heightAtPos; 
                _speed.Y = Math.Max(0, _speed.Y);
            }

            FormModel(TimeSinceLastJump);

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


        

        private const int Size = 25;
        private const int MaxHealth = 125;
        private const int minHeal = 15;
        private const int maxHeal = 20;
        private const float JumpCooldown = 8;

        private Vector3 _position;
        private Vector3 _speed;

        private float TargetSquish;
        private double TimeSinceLastJump => (JumpTimer - DateTime.Now).TotalSeconds;
        private int _health;
        private Cube _model;
        private BoundingBox _hitbox;
        private DateTime HealTimer = DateTime.Now;
        private DateTime JumpTimer = DateTime.Now;

        private void CheckHeal(in Random rnd)
        {
            if ((HealTimer - DateTime.Now).TotalSeconds > 2)
            {
                HealTimer = DateTime.Now;
                _health = Math.Min(_health + rnd.Next(minHeal, maxHeal), MaxHealth);
            }
        }
        private void FormModel(double TimeSinceLastJump)
        {
            float height = Size / 2 + (float)Math.Sin((TimeSinceLastJump) * 3) * 5;
            var newCube = new Cube(_position - new Vector3 (Size / 2, height, Size/2), Size,  height, Size);
            _model = newCube;
            _hitbox = new BoundingBox(newCube.Position, newCube.Opposite);
        }
        private void JumpAtPlayer(Vector3 playerPos, int jumpheight)
        {
            Vector3 dir = playerPos - _position;
            dir.Y = 0;
            dir.Normalize();
            dir *= 10;
            dir.Y = -jumpheight;
            _speed += dir;
        }

        public Slime(Vector3 position)
        {
            _position = position;
            _speed = Vector3.Zero;
            _health = MaxHealth;
        }

    }
}
