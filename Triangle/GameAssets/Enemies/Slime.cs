using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.Drawing.Models;
using SlimeGame.GameAsset;
using SlimeGame.GameAsset.IFrames;
using SlimeGame.GameAsset.Projectiles;
using SlimeGame.Generation;
using static SlimeGame.GameAsset.SpellBook;

namespace SlimeGame.GameAssets.Enemies
{
    internal class Slime : Enemy
    {
        public static readonly string EnemyNormal = "Slime_1";
        public static readonly string EnemyInAir = "Slime_3";
        public static void LoadContent(ContentManager content)
        {
            _enemyNormalModel = new ModelHolder(content.Load<Model>(EnemyNormal));
            _enemyAirModel = new ModelHolder(content.Load<Model>(EnemyInAir));
        }
        static ModelHolder _enemyNormalModel;
        static ModelHolder _enemyAirModel;
        ModelHolder GetCurrentModel
        {
            get
            {
                Vector3 position = _position;
                ModelHolder currentModel = _speed.Y > 25 ? _enemyAirModel : _enemyNormalModel;

                position.Y -= currentModel.BoundingBox.Min.Y;
                position.Y -= currentModel.Dimensions.Y / 2;

                currentModel.Move(_yaw, _pitch / 2f, MathF.PI, Scale, position);
                return currentModel;
            }
        }
        public override void Update(AssetManager assetManager, Random rnd)
        {
            CheckHeal(in rnd);
            double TimeSinceLastJump = TimeSinceLastMove;

            //_position.Y = assetManager.Player.EyePos.Y;
            _speed.Y += Gravity;
            if (!Attacking)
            {
                _speed.X *= 0.975f;
                _speed.Z *= 0.975f;
                _speed.Y *= 0.99f;
            }
            _position += _speed * SpeedMultiplier;

            float heightAtPos = assetManager.ChunkManager.HeightAtPosition(_position);
            if (heightAtPos < _position.Y)
            {
                _position.Y = heightAtPos;
                if (_speed.Y > 0)
                {
                    Attacking = false;
                    _speed.Y = 0;
                }
                onGround = true;
            }
            float JumpRate = this.JumpRate;
            if (onGround && JumpCooldown - TimeSinceLastJump < 0.5)
            {
                _squish = Math.Max(_squish - MathF.Pow(MathF.Abs(SquishFactorDown - _squish), JumpRate) * 0.5f, SquishFactorDown);
                if (_squish <= SquishFactorDown)
                {
                    JumpAtPlayer(assetManager.Player.Position);
                    onGround = false;
                    MovementTimer = Game1.PlayingGameTime;
                }
            }
            else if (!onGround)
            {
                _squish = Math.Max(_squish - MathF.Pow(MathF.Abs(SquishFactorStretch - _squish), JumpRate) * 0.5f, SquishFactorStretch);
            }
            else if (onGround)
            {
                _squish = Math.Max(_squish - MathF.Pow(MathF.Abs(_squish), JumpRate), 0);
            }
            if (Math.Abs(_squish) < 0.01f)
            {
                _squish = 0;
            }


            FormModel();
            base.Update(assetManager, rnd);
        }
        public override void SetPosition(Vector3 vector)
        {
            _position = vector;
            _speed = Vector3.Zero;
            FormModel();
        }
        public override BoundingBox BoundingBox { get => _hitbox; }
        public override BoundingBox Hitbox { get => _hitbox; }
        public override BoundingBox Hurtbox { get => _hurtbox; }
        public override GenericModel[] models { get => _model; }
        public override Vector3 Position { get => _position; }
        public override float Health { get => _health; }
        public override int Height { get => Math.Abs((int)((ModelHolder)_model[0]).Dimensions.Y); }
        public override IFrameType iFrameType { get => IFrameType.Universal; }
        protected override int MaxHealth { get => 300; }
        protected override float KnockbackMultiplier { get => 0.5f; }
        protected override float Damage { get => 1.5f; }
        protected override float IFrameDuration { get => 1f; }
        private const int minHeal = 3;
        private const int maxHeal = 4;
        private const float SpeedMultiplier = 2f;
        private const float Gravity = 1.5f * SpeedMultiplier;
        private const float Scale = 10f;
        private const float JumpCooldown = 4;
        private const float SquishFactorDown = -100;
        private const float SquishFactorStretch = 150;
        private const float _paddingPercent = 0.2f;

        private float JumpRate => 1;
        private bool onGround = false;
        private float _squish = SquishFactorStretch;
        private DateTime HealTimer = Game1.PlayingGameTime;
        private bool Attacking = false;

        private void CheckHeal(in Random rnd)
        {
            if ((Game1.PlayingGameTime - HealTimer).TotalSeconds > 2)
            {
                HealTimer = Game1.PlayingGameTime;
                _health = Math.Min(_health + rnd.Next(minHeal, maxHeal), MaxHealth);
            }
        }
        public static Vector3 GetMinimumVelocity(
            Vector3 start,
            Vector3 target,
            float gravity)
        {
            Vector3 displacement = target - start;

            // Horizontal displacement (XZ plane)
            Vector3 displacementXZ = new Vector3(displacement.X, 0f, displacement.Z);
            float x = displacementXZ.Length();

            // Vertical displacement
            float y = displacement.Y;

            float g = MathF.Abs(gravity);

            // Minimum-speed solution (discriminant = 0)
            float speed = MathF.Sqrt(g * (y + MathF.Sqrt(x * x + y * y)));

            // Launch angle
            float angle = MathF.Atan((y + MathF.Sqrt(x * x + y * y)) / x);

            Vector3 directionXZ = Vector3.Normalize(displacementXZ);

            // Velocity vector
            Vector3 velocity =
                directionXZ * MathF.Cos(angle) * speed +
                Vector3.Up * MathF.Sin(angle) * speed;

            return velocity;
        }

        private void FormModel()
        {
            /* Cubic slime with squish effect
            float height = Size + _squish;
            Vector3 Padding;

            Cube newCube = new Cube(_position - new Vector3(Size / 2, height, Size / 2), Size, height, Size, Color.Purple);
            newCube.SetRotation(_position, Rotation);
            Padding = (newCube.Opposite - newCube.Position) * _paddingPercent;

            _model[0] = newCube;
            _hitbox = new BoundingBox(newCube.Position, newCube.Opposite);
            _hurtbox = new BoundingBox(newCube.Position + Padding, newCube.Opposite - Padding);
            */
            Vector3 Padding;
            Padding = GetCurrentModel.Dimensions * _paddingPercent;
            _model = new GenericModel[] { GetCurrentModel };
            _hitbox = GetCurrentModel.BoundingBox;
            _hurtbox = new BoundingBox(GetCurrentModel.Min + Padding, GetCurrentModel.Max - Padding);
        }
        private void JumpAtPlayer(Vector3 playerPos)
        {
            //Vector3 dir = playerPos - _position;
            //_speed.Z += Math.Clamp(dir.Z * 0.06f, -100, 100);
            //_speed.X += Math.Clamp(dir.X * 0.06f, -100, 100);
            //_speed.Y += Math.Max(dir.Y * 0.3f - JumpMin, -100);

            Attacking = true;
            _speed = GetMinimumVelocity(Position, playerPos, Gravity / SpeedMultiplier) * 1f;
            _speed.Y *= -1;
        }
        public Slime(Vector3 position)
        {
            _position = position;
            _speed = Vector3.Down * 25;
            _health = MaxHealth;
            FormModel();
        }

    }
}
