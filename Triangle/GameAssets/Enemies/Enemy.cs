using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SlimeGame.Drawing.Models;
using SlimeGame.Drawing.Models.Shapes;
using SlimeGame.GameAsset;
using SlimeGame.GameAsset.IFrames;
using SlimeGame.GameAsset.Projectiles;
using SlimeGame.Generation;
using static System.Net.Mime.MediaTypeNames;

namespace SlimeGame.GameAssets.Enemies
{
    internal abstract class Enemy
    {
        static Random rnd = new Random();
        public abstract void SetPosition(Vector3 vector);
        public virtual void Update(AssetManager assetManager, Random rnd)
        {
            if (StunnedUntill > Game1.PlayingGameTime)
            {
                _speed = Vector3.Down * 10;
            }
            LerpRotation(assetManager.Player.Position);
        }
        protected void LerpRotation(Vector3 target)
        {
            Vector2 difference = -new Vector2(_position.X - target.X, _position.Z - target.Z);
            float newYaw = -MathF.Atan2(difference.Y, difference.X) + MathF.PI / 2;
            float delta = newYaw - _yaw;
            float maxStep = MathF.Abs(delta) * 0.03f;
            _yaw += Math.Clamp(delta, -maxStep, maxStep);
            _pitch = MathF.Atan2(_position.Y - target.Y, difference.Length());
        }
        public void Knockback(Vector3 dir, float amount)
        {
            if (dir == Vector3.Zero)
                return;
            _speed += Vector3.Normalize(dir) * KnockbackMultiplier * amount;
        }
        public void ImmuneFor(float duration)
        {
            ImmuneUntill = Game1.PlayingGameTime.AddSeconds(duration);
        }
        public void StunnedFor(float duration)
        {
            StunnedUntill = Game1.PlayingGameTime.AddSeconds(duration);
        }
        public virtual bool EnemyIsHit(float amount, AssetManager assetManager)
            => EnemyIsHit(Position - assetManager.Player.Position, amount, assetManager);
        public virtual bool EnemyIsHit(Vector3 knockbackDir, float amount, AssetManager assetManager)
        {
            if (IsImmune) return false;

            Player player = assetManager.Player;
            Knockback(knockbackDir, amount);
            _health -= amount;
            if (Health <= 0)
            {
                player.LevelManager.AddExp(MaxHealth);
                assetManager.Enemies.Remove(this);

                for (int i = 0; i < rnd.Next(15, 20); i++)
                {
                    Vector3 randomVector = new Vector3(rnd.Next(-20, 20), rnd.Next(-20, 20), rnd.Next(-20, 20));
                    assetManager.Add(new Particle(Position, models[0].Color, player.Speed + randomVector, rnd));
                }
                return true;
            }
            return false;
        }
        public void EnemyHitPlayer(Player player)
        {
            Vector3 knockback = Vector3.Normalize(player.Position - Position) * KnockbackMultiplier + Vector3.Up * 5;
            IFrameInstance iFrameInstance = new IFrameInstance(this, IFrameDuration, iFrameType);
            player.TakeDamage(iFrameInstance, knockback, Damage);
        }
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
            Vector3 BarPosition = Position;
            BarPosition.Y -= Height + 20;
            Vector3 dir = player.Position - Position;
            dir.Normalize();
            Vector3 crossRight = Vector3.Cross(dir, Vector3.Up);
            Vector3 crossRightTimesSize = crossRight * HealthBarSize;

            Vector3 BarBottomLeft = BarPosition - crossRightTimesSize;
            Vector3 BarTopLeft = BarBottomLeft;
            BarTopLeft.Y -= HealthBarHeight;

            Vector3 BarBottomRight = BarPosition + crossRightTimesSize;
            Vector3 BarTopRight = BarBottomRight;
            BarTopRight.Y -= HealthBarHeight;

            float healthPercentage = Math.Clamp((float)Health / MaxHealth, 0, 1);
            crossRightTimesSize = crossRight * HealthBarSize * healthPercentage * 2;

            Vector3 GreenBarBottomRight = BarBottomLeft + crossRightTimesSize;
            Vector3 GreenBarTopRight = GreenBarBottomRight;
            GreenBarTopRight.Y -= HealthBarHeight;

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
        protected const int HealthBarSize = 400;
        protected const int HealthBarHeight = 100;
        public abstract BoundingBox BoundingBox { get; }
        public abstract BoundingBox Hitbox { get; }
        public abstract BoundingBox Hurtbox { get; }
        public abstract GenericModel[] models { get; }
        public abstract float Health { get; }
        public abstract int Height { get; }
        public abstract Vector3 Position { get; }
        public abstract IFrameType iFrameType { get; }
        protected abstract int MaxHealth { get; }
        protected abstract float KnockbackMultiplier { get; }
        protected abstract float Damage { get; }
        protected abstract float IFrameDuration { get; }
        protected double TimeSinceLastMove => (Game1.PlayingGameTime - MovementTimer).TotalSeconds;
        public bool IsImmune => Game1.PlayingGameTime < ImmuneUntill;
        protected Vector2 Rotation => new Vector2(_yaw, _pitch);
        protected Vector3 _speed;
        protected Vector3 _position;
        protected float _health;
        protected private GenericModel[] _model = new GenericModel[1];
        protected private BoundingBox _hitbox;
        protected private BoundingBox _hurtbox;
        protected DateTime MovementTimer = Game1.PlayingGameTime;
        private DateTime ImmuneUntill;
        private DateTime StunnedUntill;
        protected float _yaw = 0;
        protected float _pitch = 0;
    }
}
