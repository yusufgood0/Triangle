using Microsoft.Xna.Framework;
using random_generation_in_a_pixel_grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triangle.Enemies
{
    internal interface Enemy
    {
        public void Update(in Player player, in Random rnd, ref BoundingBox[] collisionObject, SeedMapper seedMap, int MapCellSize);
        public void Bounce(Vector3 position);
        public void EnemyHitPlayer(ref Player player);
        public void EnemyIsHit(ref Player player, Vector3 source, int amount);
        public static bool CheckLineOfSight(Vector3 Pos1, Vector3 Pos2, ref BoundingBox[] collisionObject)
        {
            float distance = Vector3.Distance(Pos1, Pos2);
            foreach (var box in collisionObject)
            {
                Ray ray = new Ray(Pos2, Pos2 - Pos1);
                if (box.Intersects(ray) < distance)
                {
                    return false;
                }
            }
            return true;
        }
        public BoundingBox BoundingBox { get; }
        public BoundingBox Hitbox { get; }
        public Model[] models { get; }
        public Vector3 Position { get; }
        public int Health { get; }
    }
}
