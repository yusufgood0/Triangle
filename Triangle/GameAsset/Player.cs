using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.GameAsset.IFrames;
using SlimeGame.Generation;
using SlimeGame.Input;
using SlimeGame.Models.Shapes;

namespace SlimeGame.GameAsset
{
    internal class Player
    {
        static Texture2D _texture;
        const int HitboxPadding = -8;
        private static Vector3 HitboxPaddingVector3 = new Vector3(HitboxPadding);
        private static readonly string _saveDirectory = Path.Combine(Environment.CurrentDirectory, "PlayerInfo", "PlayerPos.txt");
        public static int sizeX = 30;
        public static int sizeY = 140;
        public static int sizeZ = 30;
        static float _maxHealth = 5;

        Random _rnd;
        float _health = _maxHealth;
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

        public static float MaxHealth => _maxHealth;
        public float Health => _health;
        public Vector2 Angle => new Vector2(_angle.X + _shake.X, MathHelper.Clamp(_angle.Y + _shake.Y, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f));
        public Camera PlayerCamera { get => _playerCamera; }
        public float SpeedEffectMultiplier => Math.Clamp(1 - _speed.Length()/100, 0, 1);
        public float SpeedEffectMultiplierReverse => Math.Clamp(_speed.Length()/100, 0, 1);
        public Player(Vector3 position, in Camera camera, Random rnd, Point screenSize, float xPaddingPercent, float yPaddingPercent, Texture2D texture)
        {
            _position = position;
            _playerCamera = camera;
            TryPullPositionFromArchive();
            _rnd = rnd;
            _ExpManager = new ExpManager(screenSize, xPaddingPercent, yPaddingPercent, texture);
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
        public Vector3 EyePos => _position + new Vector3(sizeX / 2, 0, sizeZ / 2);
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
            _health += 0.5f;
            _health = Math.Min(_maxHealth, _health);
            ResetHealTimer();
            return true;
        }
        public void ResetHealTimer()
        {
            HealTimer = Game1.PlayingGameTime.AddSeconds(5);
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
                    _speed = _speed  * 0.95F- Math.Max(dot, 0) * normal;
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
            _playerCamera.Position = EyePos;
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
        public void SavePosition()
        {
            if (!File.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }

            using StreamWriter writer = new(_saveDirectory);
            writer.WriteLine(_position.X.ToString());
            writer.WriteLine(_position.Y.ToString());
            writer.WriteLine(_position.Z.ToString());
        }
        void TryPullPositionFromArchive()
        {
            if (!File.Exists(_saveDirectory))
            {
                // debug.writeline($"Player position file {_saveDirectory} does not exist.");
                return; // Chunk does not exist in archive. Pull failed
            }
            string[] PlayerPosition = File.ReadAllLines(_saveDirectory);
            if (PlayerPosition.Length < 3)
            {
                // debug.writeline("Could not read PlayerFile");
                return; // Not enough data to set position
            }
            if (float.TryParse(PlayerPosition[0], out float xPos) && float.TryParse(PlayerPosition[1], out float yPos) && float.TryParse(PlayerPosition[2], out float zPos))
            {
                _position = new Vector3(xPos, yPos, zPos);
                // debug.writeline("Loaded Player position Successfully");
            }
        }
        public void SafeControlAngleWithMouse(bool PreviousIsMouseVisible, bool IsMouseVisible, Point screenSize, float sensitivity)
        {
            if (!PreviousIsMouseVisible)
            {
                UpdateRotation(screenSize, sensitivity * SpeedEffectMultiplier);
            }
            if (!IsMouseVisible)
            {
                Mouse.SetPosition(screenSize.X / 2, screenSize.Y / 2);
            }
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
