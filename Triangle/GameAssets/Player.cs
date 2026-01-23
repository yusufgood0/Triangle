using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Drawing;
using SlimeGame.GameAsset.Exp;
using SlimeGame.GameAsset.IFrames;
using SlimeGame.GameAssets.Enemies;
using SlimeGame.Generation;
using SlimeGame.Input;

namespace SlimeGame.GameAsset
{
    internal class Player
    {
        static Texture2D _texture;
        const int HitboxPadding = -8;
        private static Vector3 HitboxPaddingVector3 = new Vector3(HitboxPadding);
        public static int sizeX = 30;
        public static int sizeY = 200;
        public static int sizeZ = 30;
        public const int StartingMaxHealth = 5;
        float _maxHealth;

        Random _rnd;
        float _health = StartingMaxHealth;
        ExpManager _ExpManager;
        Vector3 _position = new();
        Vector3 _speed = Vector3.Zero;
        float _movementMultiplier = 1;
        Vector2 _angle = Vector2.Zero;
        Vector2 _shake = Vector2.Zero;
        Vector2 _shakeDifference = Vector2.Zero;
        Camera _playerCamera;
        DateTime UnStunTime = Game1.PlayingGameTime;
        DateTime HealTimer = Game1.PlayingGameTime;
        ChunkManager _chunkManager;
        public float MaxHealth => _maxHealth;
        public float Health => _health;
        public Vector2 Angle => new Vector2(_angle.X + _shake.X, MathHelper.Clamp(_angle.Y + _shake.Y, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f));
        public Camera PlayerCamera { get => _playerCamera; }
        public float SpeedEffectMultiplier => Math.Clamp(1 - _speed.Length() / 100, 0, 1);
        public float SpeedEffectMultiplierReverse => Math.Clamp(_speed.Length() / 100, 0, 1);

        public AudioListener AudioListener
        {
            get
            {
                AudioListener soundListener = new AudioListener();
                soundListener.Forward = dirVector;
                soundListener.Up = _playerCamera.Up;
                soundListener.Position = Position;
                soundListener.Velocity = Speed;
                return soundListener;
            }
        }

        public Player(Vector3 position, in Camera camera, Random rnd, Point screenSize, float xPaddingPercent, float yPaddingPercent, Texture2D texture, SpriteFont spriteFont, ChunkManager chunkManager)
        {
            _position = position;
            _playerCamera = camera;
            _rnd = rnd;
            _ExpManager = new ExpManager(screenSize, xPaddingPercent, yPaddingPercent, texture, spriteFont);
            _maxHealth = StartingMaxHealth;
            _chunkManager = chunkManager;
        }
        public bool IsStunned { get => Game1.PlayingGameTime < UnStunTime; }
        public BoundingBox HitBox
        {
            get => new BoundingBox(
                _position - HitboxPaddingVector3,
                _position + new Vector3(sizeX, sizeY, sizeZ) + HitboxPaddingVector3
                );
        }
        public Vector3 Speed { get => _speed; }
        public float SpeedMultiplier { get => _movementMultiplier; set => _movementMultiplier = value; }
        public ExpManager LevelManager { get => _ExpManager; }
        public Vector3 Position
        {
            get => _position;
            set => Move(value - _position);
        }
        public Vector3 EyePos
        {
            get
            {
                Vector3 pos = _position + new Vector3(sizeX / 2, 0, sizeZ / 2);
                pos.Y = Math.Min(pos.Y, _chunkManager.HeightAtPosition(pos) - 100);
                return pos;
            }
        }
        public Rectangle Rectangle { get => new((int)_position.X, (int)_position.Y, sizeX, sizeY); }
        public Vector3 dirVector { get => General.angleToVector3(Angle); }

        public void Stun(float duration)
        {
            UnStunTime = Game1.PlayingGameTime.AddSeconds(duration);
        }
        public void IncreaseMaxHealth(float amount)
        {
            _maxHealth += amount;
        }
        public bool TryHeal()
        {
            if (HealTimer > Game1.PlayingGameTime) return false;
            _health += 0.25f;
            _health = Math.Min(_maxHealth, _health);
            ResetHealTimer();
            return true;
        }
        public void ResetHealTimer()
        {
            HealTimer = Game1.PlayingGameTime.AddSeconds(2.5f);
        }
        public bool TakeDamage(IFrameInstance iFrame, Vector3 force, float amount)
        {
            if (iFrame.IsImmune())
            {
                return false;
            }
            //ResetHealTimer();
            WorldDraw.BlinkColors(Color.Red.ToVector3(), Color.Orange.ToVector3(), Color.OrangeRed.ToVector3());
            IFrameInstance.AddIFrame(iFrame);
            _health -= amount;
            _health = Math.Max(0, _health);
            _speed += force;
            _shake += amount * 0.5f * new Vector2(
                (2 * (float)_rnd.NextDouble() - 1f),
                (2 * (float)_rnd.NextDouble() - 1f)
                );
            Stun(0.05f);
            return true;
        }
        public void SetSpeed(Vector3 vector)
        {
            _speed = vector;
        }
        public void ChangeSpeed(Vector3 vector)
        {
            _speed += vector;
        }
        //public bool ApplyTerrainCollision(Vector3 normal)
        //{
        //    var speed = _speed;
        //    speed.Normalize();
        //    float dot = Vector3.Dot(Vector3.Up, normal);
        //    float AbsDot = Math.Abs(dot);
        //    if (AbsDot < 0.07f)
        //    {
        //        //_speed.X *= Math.Min(AbsDot * 20, 1);
        //        //_speed.Z *= Math.Min(AbsDot * 20, 1);
        //        _speed += normal;
        //        _position.Y += 1;
        //        return true;
        //    }
        //    return false;
        //}
        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 OFFSET)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_texture,
                new Rectangle(Rectangle.X + (int)OFFSET.X, Rectangle.Y + (int)OFFSET.Y, Rectangle.Width, Rectangle.Height),
                null,
                Color.Red,
                0,
                new(),
                0,
                //_position.Z
                0
                );
            spriteBatch.End();
        }
        public bool Update(MasterInput masterInput, ChunkManager ChunkManager)
        {
            MoveKeyPressed(masterInput);
            Friction();
            UpdateShake();
            Move(Speed * _movementMultiplier);
            CheckIfHitGround(ChunkManager, masterInput.IsPressed(KeybindActions.Jump));
            _movementMultiplier = 1f + (masterInput.IsPressed(Keys.LeftShift) ? 1f : 0.8f);

            return TryHeal();
        }
        public void ApplyGravity(bool onGround, Vector3 groundNormal)
        {
            Vector3 gravity = Vector3.Up * 0.8f;
            if (onGround)
            {

                if (_speed.Y > 0)
                {
                    _speed *= 1.03f;
                }
                Vector3 slopeGravity = gravity - Vector3.Dot(gravity, groundNormal) * groundNormal;
                _speed += slopeGravity;
            }
            else
            {
                _speed += gravity;
            }
        }
        public void Friction()
        {
            _speed.X *= .97f;
            _speed.Y *= .995f;
            _speed.Z *= .97f;
        }
        public void Dash()
        {
            _speed = dirVector * 80;
        }
        public bool IsOnGround(ChunkManager chunkManager)
            => (int)(chunkManager.HeightAtPosition(_position) - Player.sizeY) <= this.Position.Y;
        public bool IsOnGround(ChunkManager chunkManager, float Leniency)
            => (int)(chunkManager.HeightAtPosition(_position) - Player.sizeY - Leniency) <= this.Position.Y;
        public void CheckIfHitGround(ChunkManager chunkManager, bool tryJump)
        {
            int terrainHeightAtPlayerPosition = (int)(chunkManager.HeightAtPosition(_position) - Player.sizeY);
            Vector3 normal = chunkManager.GetNormal(this.EyePos);
            bool lenientOnGround = IsOnGround(chunkManager, 10);
            float intoGround = Vector3.Dot(_speed, normal);
            float dot = Vector3.Dot(Vector3.Normalize(_speed), normal);
            if (normal.Y > 0) { normal *= -1; }


            ApplyGravity(lenientOnGround, normal);

            if (lenientOnGround && tryJump)
            {
                Jump(normal);
            }

            /* If player is below terrain, move them to terrain height */
            if (lenientOnGround && !tryJump && Math.Abs(dot) < 0.1f)
            {
                SetPosition(
                    new Vector3(
                        _position.X,
                        terrainHeightAtPlayerPosition,
                        _position.Z
                    ));
                if (intoGround < 0f && !tryJump)
                    _speed -= intoGround * normal;
            }
            else if (terrainHeightAtPlayerPosition < this.Position.Y)
            {
                HitGround(normal, tryJump);
                SetPosition(
                    new Vector3(
                        _position.X,
                        terrainHeightAtPlayerPosition,
                        _position.Z
                    ));
                if (intoGround < 0f && !tryJump)
                    _speed -= intoGround * normal;
            }
            //_speed.Y = -_speed.Y;
            //_speed.Y = -_speed.Y;
            return;
        }
        public void HitGround(Vector3 normal, bool jumping)
        {
            if (normal != Vector3.Zero && !jumping)
            {
                _speed.Y = -_speed.Y;
                float dot = Vector3.Dot(_speed, normal);
                _speed = _speed * 0.95F - Math.Max(dot, 0) * normal;
                _speed.Y = -_speed.Y;

            }
        }
        public void Jump(Vector3 normal)
        {
            //_speed.Y = -40;
            //_speed -= normal * 10f;
            SetPosition(_position + normal * 10f);
            //_speed.Y = -_speed.Y;
            _speed = Vector3.Reflect(_speed, normal);
            //_speed.Y = -_speed.Y;
            _speed.Y = Math.Max(_speed.Y - 15, -30);
        }
        void Move(Vector3 vector)
        {
            if (IsStunned) return;
            SetPosition(_position + vector);
            if (_speed.LengthSquared() > 20)
            {
                _shake += 0.004f * new Vector2(
                    (2 * (float)_rnd.NextDouble() - 1f),
                    (2 * (float)_rnd.NextDouble() - 1f)
                    );
                _shake *= 1f * new Vector2(
                    (2 * (float)_rnd.NextDouble() - 1f),
                    (2 * (float)_rnd.NextDouble() - 1f)
                    ) * (float)Math.Pow(_speed.LengthSquared(), 0.005f);
            }
        }
        void SetPosition(Vector3 vector)
        {
            _position = vector;
            _playerCamera.Position = EyePos - dirVector * 1000;
        }
        enum Direction
        {
            FORWARD = Keys.W,
            BACKWARDS = Keys.S,
            LEFT = Keys.A,
            RIGHT = Keys.D,
            UP = Keys.Space,
            DOWN = Keys.LeftShift
        }
        public void MoveKeyPressed(MasterInput masterInput)
        {
            Vector3 movement = Vector3.Zero;

            var dirVector = this.dirVector;
            dirVector.Y = 0;
            dirVector.Normalize();

            Vector3 RightDirection = Vector3.Cross(dirVector, _playerCamera.Up);

            if (masterInput.IsPressed(Keys.W))
                movement += dirVector;
            if (masterInput.IsPressed(Keys.S))
                movement -= dirVector;
            if (masterInput.IsPressed(Keys.A))
                movement -= RightDirection;
            if (masterInput.IsPressed(Keys.D))
                movement += RightDirection;

            // Normalize if moving diagonally
            if (movement == Vector3.Zero)
            {
                Vector2 rightJoystickDirection = masterInput.GamePadState.ThumbSticks.Left;
                if (rightJoystickDirection == Vector2.Zero) return;
                rightJoystickDirection.Normalize();

                float cosAngle = (float)Math.Cos(-_angle.X);
                float sinAngle = (float)Math.Sin(-_angle.X);

                float rotatedX = rightJoystickDirection.X * cosAngle - rightJoystickDirection.Y * sinAngle;
                float rotatedY = rightJoystickDirection.X * sinAngle + rightJoystickDirection.Y * cosAngle;

                movement += new Vector3(rotatedX, 0, rotatedY);
            }
            else
            {
                movement.Normalize();
            }

            _speed += movement;
        }
        public void SafeControlAngleWithMouse(bool PreviousIsMouseVisible, bool IsMouseVisible, Point screenSize, float sensitivity)
        {
            if (!PreviousIsMouseVisible)
            {
                UpdateRotation(screenSize, sensitivity * 0.75f + sensitivity * SpeedEffectMultiplierReverse * 0.25f);
            }
            if (!IsMouseVisible)
            {
                Mouse.SetPosition(screenSize.X / 2, screenSize.Y / 2);
            }
        }
        public void AimAssist(AssetManager manager)
        {
            if (manager.Enemies.Count() <= 0) return;
            //Enemy target = manager.Enemies[0];
            Vector3 enemyDir = Vector3.Zero;
            float greatestDot = default;
            foreach (Enemy enemy in manager.Enemies)
            {
                Vector3 target = (enemy.Hitbox.Min + enemy.Hitbox.Max) /2F;
                //Vector3 target = enemy.Position;
                Vector3 dir = Vector3.Normalize(target - Position);
                float dot = Vector3.Dot(dir, dirVector);
                dot = MathF.Max(dot, greatestDot);

                if (greatestDot == default)
                {
                    greatestDot = dot;
                    enemyDir = dir;
                }
            }

            //enemyDir = Vector3.Normalize(target.Position - Position);

            if (enemyDir == Vector3.Zero) return;
            if (greatestDot < 0f) return;

            float EnemyYaw = MathF.Atan2(enemyDir.X, enemyDir.Z);
            float EnemyPitch = MathF.Asin(MathHelper.Clamp(enemyDir.Y, -1f, 1f));

            float deltaYaw =  _angle.X - EnemyYaw;
            float deltaPitch = _angle.Y - EnemyPitch;

            while (deltaYaw > MathF.PI) deltaYaw -= MathF.Tau;
            while (deltaYaw < -MathF.PI) deltaYaw += MathF.Tau;

            while (deltaPitch > MathF.PI) deltaPitch -= MathF.Tau;
            while (deltaPitch < -MathF.PI) deltaPitch += MathF.Tau;

            _angle.X -= deltaYaw * 0.03f * greatestDot;
            _angle.Y -= deltaPitch * 0.03f * greatestDot;

            //_angle.X = EnemyYaw;
            //_angle.Y = EnemyPitch;

            _playerCamera.SetRotation(Angle.X, Angle.Y);
        }
        public void UpdateRotation(Point screenSize, float sensitivity)
        {
            _angle.X += (Mouse.GetState().X - screenSize.X / 2) * sensitivity;
            _angle.Y += (Mouse.GetState().Y - screenSize.Y / 2) * sensitivity;
            _angle.Y = MathHelper.Clamp(_angle.Y, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);
            _playerCamera.SetRotation(Angle.X, Angle.Y);

        }
        public void Recoil(float intensity)
        {
            _shakeDifference = new(
                (2 * (float)_rnd.NextDouble() - 1f) / (1f / intensity),
                ((float)_rnd.NextDouble() - 1f) / (1f / intensity)
                );
            _speed -= dirVector * intensity * 5f;
        }
        public void Shake(Vector2 shakeDifference)
        {
            _shakeDifference = shakeDifference;
        }
        public void Shake(float intensity, Random rnd)
        {
            _shakeDifference = new(
                (2 * (float)rnd.NextDouble() - 1f) / (1f / intensity),
                (2 * (float)rnd.NextDouble() - 1f) / (1f / intensity)
                );
        }
        public void ShakeFull(float intensity, Random rnd)
        {
            _shakeDifference = new(
                (2 * (float)rnd.Next(0, 2) - 1f) / (1f / intensity),
                (2 * (float)rnd.Next(0, 2) - 1f) / (1f / intensity)
                );
        }
        void UpdateShake()
        {
            _shake += (_shakeDifference - _shake) * 0.06f;
            _shakeDifference *= 0.93f;
            //if (_shake.Length() < 0.05f)
            //{
            //    _shake = Vector2.Zero;
            //    _shakeDifference = Vector2.Zero;
            //}
        }




    }
}
