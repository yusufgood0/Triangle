using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimeGame.Generation;
using SlimeGame.Models;
using SlimeGame.Models.Shapes;
using SlimeGame.GameAsset;

namespace SlimeGame.Enemies
{
    internal interface Enemy
    {
        public void Update(in Player player, in List<Projectile> projectiles, in Random rnd, SeedMapper seedMap, int MapCellSize);
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
        public Shape[] GetHealthBar(Player player)
        {
            Vector3 BarPosition = this.Position;
            BarPosition.Y -= (this.Height + 20);
            Vector3 dir = player.Position - this.Position;
            dir.Normalize();
            Vector3 crossRight = Vector3.Cross(dir, Vector3.Up);
            Vector3 crossRightTimesSize = crossRight * HealthBarSize;

            Vector3 BarBottomLeft = BarPosition - crossRightTimesSize;
            Vector3 BarTopLeft = BarBottomLeft;
            BarTopLeft.Y -= Enemy.HealthBarHeight;

            Vector3 BarBottomRight = BarPosition + crossRightTimesSize;
            Vector3 BarTopRight = BarBottomRight;
            BarTopRight.Y -= Enemy.HealthBarHeight;

            float healthPercentage = Math.Clamp((float)Health / (float)MaxHealth, 0, 1);
            crossRightTimesSize = crossRight * HealthBarSize * healthPercentage * 2;

            Vector3 GreenBarBottomRight = BarBottomLeft + crossRightTimesSize;
            Vector3 GreenBarTopRight = GreenBarBottomRight;
            GreenBarTopRight.Y -= Enemy.HealthBarHeight;

            Shape[] healthBar = new Shape[2];
            healthBar[0] = new Square(
                GreenBarTopRight,
                BarTopRight,
                BarBottomRight,
                GreenBarBottomRight,
                Color.Red
                );

            healthBar[1] = new Square(
                BarTopLeft,
                GreenBarTopRight,
                GreenBarBottomRight,
                BarBottomLeft,
                Color.Green
                );

            return healthBar;
        }
        const int HealthBarSize = 400;
        const int HealthBarHeight = 100;
        public BoundingBox BoundingBox { get; }
        public BoundingBox Hitbox { get; }
        public GenericModel[] models { get; }
        public Vector3 Position { get; }
        public int Health { get; }
        public int MaxHealth { get; }
        public int Height { get; }
    }
}
