using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triangle.Enemies
{
    internal class Slime : Enemy
    {
        void Enemy.Update(in Player player, in Random rnd, ref BoundingBox[] collisionObject)
        {
            CheckHeal(in rnd);
            foreach (var box in collisionObject)
            {
                Ray ray = new Ray(_position, _position - player.Position);
                if (box.Intersects(ray) < 500)
                {

                }
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

        private void CheckHeal(in Random rnd)
        {
            if ((HealTimer - DateTime.Now).TotalSeconds > 2)
            {
                HealTimer = DateTime.Now;
                _health = Math.Min(_health + rnd.Next(minHeal, maxHeal), MaxHealth);
            }
        }

        private static int Size = 25;
        private static int MaxHealth = 125;
        private static int minHeal = 15;
        private static int maxHeal = 20;

        private Vector3 _position;
        private Vector3 _speed;
        private int _health;
        private DateTime HealTimer = DateTime.Now;

        public Slime(Vector3 position)
        {
            _position = position;
            _speed = Vector3.Zero;
            _health = MaxHealth;
        }

    }
}
