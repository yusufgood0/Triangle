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
        public void Knockback(Vector3 position);
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
        public Square[] GetHealthBar(Player player)
        {
            Vector3 BarPosition = this.Position;
            BarPosition.Y -= (this.Height + 20);
            Vector3 dir = player.Position - this.Position;
            Vector3 crossRight = Vector3.Cross(dir, Vector3.Up);
            Vector3 crossRightTimesSize = crossRight * HealthBarSize;

            Vector3 BarBottomLeft = BarPosition - crossRight;
            Vector3 BarTopLeft = BarBottomLeft;
            BarTopLeft.Y -= Enemy.HealthBarHeight;

            Vector3 BarBottomRight = BarPosition + crossRight;
            Vector3 BarTopRight = BarBottomLeft;
            BarTopRight.Y -= Enemy.HealthBarHeight;

            Square[] healthBar = new Square[2];
            healthBar[0] = new Square(
                BarTopLeft,
                BarTopRight,
                BarBottomRight,
                BarBottomLeft,
                Color.Black
                );

            float healthPercentage = Math.Clamp((float)Health / MaxHealth, 0, 1);
            crossRightTimesSize *= healthPercentage;

            Vector3 GreenBarBottomRight = BarPosition + crossRight;
            Vector3 GreenBarTopRight = BarBottomLeft;
            GreenBarTopRight.Y -= Enemy.HealthBarHeight;
            healthBar[0] = new Square(
                BarTopLeft,
                BarTopRight,
                GreenBarBottomRight,
                GreenBarTopRight,
                Color.Green
                );

            return healthBar;
        }
        const int HealthBarSize = 100;
        const int HealthBarHeight = 40;
        public BoundingBox BoundingBox { get; }
        public BoundingBox Hitbox { get; }
        public Model[] models { get; }
        public Vector3 Position { get; }
        public int Health { get; }
        public int MaxHealth { get; }
        public int Height { get; }
    }
}
