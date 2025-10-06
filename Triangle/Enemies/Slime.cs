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

            int heightAtPos = seedMap.HeightAtPosition(_position, MapCellSize) - Size;
            if (heightAtPos < _position.Y)
            {
                _position.Y = heightAtPos;
                _speed.Y = Math.Min(0, _speed.Y);
            }

            if ((DateTime.Now - JumpTimer).TotalSeconds > 1 &&
                _speed.Y == 0
                //Enemy.CheckLineOfSight(_position, player.Position, ref collisionObject)
                )
            {
                JumpTimer = DateTime.Now;
                JumpAtPlayer(player.Position, rnd.Next(15, 20));

            }

            _speed.Y += 0.3f;
            _speed *= 0.98f;
            _position += _speed;

            

            FormModel();

        }
        void Enemy.EnemyHitPlayer(ref Player player)
        {

        }
        void Enemy.EnemyIsHit(ref Player player)
        {

        }
        BoundingBox Enemy.BoundingBox { get; }
        BoundingBox Enemy.Hitbox { get; }
        Model[] Enemy.models { get => _model; }
        Vector3 Enemy.Position { get => _position; }


        

        private const int Size = 250;
        private const int MaxHealth = 125;
        private const int minHeal = 15;
        private const int maxHeal = 20;

        private Vector3 _position;
        private Vector3 _speed;
        private int _health;
        private Model[] _model = new Model[1];
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
        private void FormModel()
        {
            _model[0] = new Cube(_position - new Vector3(Size / 2, Size, Size / 2), Size, Size, Size);
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
            _speed = Vector3.Down * 5;
            _health = MaxHealth;
            FormModel();
        }

    }
}
