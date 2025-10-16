using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Triangle
{
    internal class Player
    {
        static Texture2D _texture;
        public static int sizeX = 30;
        public static int sizeY = 140;
        public static int sizeZ = 30;
        Vector3 _position = new();
        Vector3 _speed = new();
        public Vector2 Angle => _angle + _shake;
        Vector2 _angle = Vector2.Zero;
        Vector2 _shake = Vector2.Zero;
        Vector2 _shakeDifference = Vector2.Zero;
        private static readonly string _saveDirectory = Path.Combine(Environment.CurrentDirectory, "PlayerInfo", "PlayerPos.txt");
        public Camera PlayerCamera { get; set; }
        public enum GameMode
        {
            Survival,
            Creative
        }

        GameMode gameMode = GameMode.Survival;

        public Player(Vector3 position, in Camera camera)
        {
            _position = position;
            PlayerCamera = camera;
            TryPullPositionFromArchive();
        }
        public bool ApplyTerrainCollision(Vector3 normal)
        {
            var speed = _speed;
            speed.Normalize();
            float dot = Vector3.Dot(Vector3.Up, normal);
            float AbsDot = Math.Abs(dot);
            if (AbsDot < 0.07f)
            {
                //_speed.X *= Math.Min(AbsDot * 20, 1);
                //_speed.Z *= Math.Min(AbsDot * 20, 1);
                _speed += normal;
                _position.Y += 1;
                return true;
            }
            return false;
        }
        //public void HitGround(Vector3 normal)
        //{

        //    if (_speed.Y > 15)
        //    {
        //        _speed = Vector3.Reflect(_speed, normal) * 1.5f;
        //    }
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
        /*
        public void CollisionX(Cube Cube)
        {
            if (_speed.X > 0)
            {
                _position.X = Cube.X - sizeX - .1f;
                _speed.X = 0;
            }
            else if (_speed.X < 0)
            {
                _position.X = Cube.X_OP + .1f;
                _speed.X = 0;
            }
        }
        public void CollisionY(Cube Cube)
        {
            if (_speed.Y > 0)
            {
                _speed.Y = 0;
                _position.Y = Cube.Y - sizeY - .1f;
            }
            else if (_speed.Y < 0)
            {
                _position.Y = Cube.Y_OP + .1f;
                _speed.Y = 0;
            }
        }
        public void CollisionZ(Cube Cube)
        {
            if (_speed.Z > 0)
            {
                _speed.Z = 0;
                _position.Z = Cube.Z - sizeZ - 1f;
            }
            else if (_speed.Z < 0)
            {
                _position.Z = Cube.Z_OP + .1f;
                _speed.Z = 0;
            }
        }
        */
        public void Update(KeyboardState keyboardState)
        {
            if (IsSurvival)
            {
                ApplyGravity();
            }
            MoveKeyPressed(keyboardState);
            Friction();
            UpdateShake();
        }
        public void ApplyGravity()
        {
            _speed.Y += 0.5f;
        }
        public void Friction()
        {
            _speed.X *= .97f;
            _speed.Y *= .99f;
            _speed.Z *= .97f;
        }
        public void Dash()
        {
            _speed += dirVector * 40;
        }
        DateTime lastHit = DateTime.Now;
        public bool HitGround(KeyboardState keyBoardState, Vector3 normal)
        {
            _speed.X *= 0.95f;
            _speed.Z *= 0.95f;

            if (_speed.Y > 10 || keyBoardState.IsKeyDown(Keys.Space))
            {
                _speed.Y *= -0.6f;
                return true;
            }

            //if (_speed.Y > 15 &&
            //    (DateTime.Now - lastHit).TotalMilliseconds > 200
            //    )
            //{
            //    _speed = Vector3.Reflect(_speed, normal) * 1f;
            //    lastHit = DateTime.Now;
            //    return true;
            //}

            _speed.Y = Math.Min(5, _speed.Y);
            return false;
        }
        public void Jump()
        {
            _speed.Y = -40;
        }
        public void SetPosition(Vector3 vector)
        {
            _position = vector;
            PlayerCamera.Position = EyePos;
        }
        public void Move(Vector3 vector)
        {
            _position += vector;
            PlayerCamera.Position = EyePos;
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
        public void MoveKeyPressed(KeyboardState keyboardState)
        {
            Vector3 movement = Vector3.Zero;

            var dirVector = this.dirVector;
            dirVector.Y = 0;
            dirVector.Normalize();
            Vector3 RightDirection = Vector3.Cross(dirVector, PlayerCamera.Up);

            if (keyboardState.IsKeyDown(Keys.W))
                movement += dirVector;
            if (keyboardState.IsKeyDown(Keys.S))
                movement -= dirVector;
            if (keyboardState.IsKeyDown(Keys.A))
                movement -= RightDirection;
            if (keyboardState.IsKeyDown(Keys.D))
                movement += RightDirection;

            // Normalize if moving diagonally
            if (movement != Vector3.Zero)
            {
                movement.Normalize();
            }

            if (IsCreative)
            {
                if (keyboardState.IsKeyDown(Keys.Space))
                    movement.Y -= 1;
                if (keyboardState.IsKeyDown(Keys.LeftShift))
                    movement.Y += 1;
            }

            _speed += movement * 1f;
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
                Debug.WriteLine($"Player position file {_saveDirectory} does not exist.");
                return; // Chunk does not exist in archive. Pull failed
            }
            string[] PlayerPosition = File.ReadAllLines(_saveDirectory);
            if (PlayerPosition.Length < 3)
            {
                Debug.WriteLine("Could not read PlayerFile");
                return; // Not enough data to set position
            }
            if (float.TryParse(PlayerPosition[0], out float xPos) && float.TryParse(PlayerPosition[1], out float yPos) && float.TryParse(PlayerPosition[2], out float zPos))
            {
                _position = new Vector3(xPos, yPos, zPos);
                Debug.WriteLine("Loaded Player position Successfully");
            }
        }
        public void SafeControlAngleWithMouse(bool PreviousIsMouseVisible, bool IsMouseVisible, Point screenSize, float sensitivity)
        {
            if (!PreviousIsMouseVisible)
            {
                UpdateRotation(screenSize, sensitivity);
            }
            if (!IsMouseVisible)
            {
                Mouse.SetPosition((int)screenSize.X / 2, (int)screenSize.Y / 2);
            }
        }
        public void UpdateRotation(Point screenSize, float sensitivity)
        {
            _angle.X += (Mouse.GetState().X - screenSize.X / 2) * sensitivity;
            _angle.Y += (Mouse.GetState().Y - screenSize.Y / 2) * sensitivity;
            _angle.Y = MathHelper.Clamp(_angle.Y, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
            PlayerCamera.SetRotation(Angle.X, Angle.Y);

        }
        public void Shake(int intensity, Random rnd)
        {
            _shakeDifference = new (
                (float)rnd.NextDouble() * rnd.Next(-intensity, intensity), 
                (float)rnd.NextDouble() * rnd.Next(-intensity, intensity)
                );
        }
        void UpdateShake()
        {
            _shake += (_shakeDifference - _shake) * 0.1f;
            _shakeDifference *= 0.9f;
            if (_shake.Length() < 0.1f)
            {
                _shake = Vector2.Zero;
                _shakeDifference = Vector2.Zero;
            }
        }

        public bool IsSurvival { get => gameMode == GameMode.Survival; }
        public bool IsCreative { get => gameMode == GameMode.Creative; }
        public Vector2 XZ { get => new(_position.X, _position.Z); }
        public Vector3 Speed { get => _speed; }
        public Vector3 Position { get => _position; }
        public Vector3 OppositeCorner => Position + new Vector3(sizeX, sizeY, sizeZ);
        public Vector3 Center { get => _position + new Vector3(sizeX, sizeY, sizeZ) / 2; }
        public Vector3 EyePos => _position + new Vector3(sizeX / 2, 0, sizeZ / 2);
        public Rectangle Rectangle { get => new((int)_position.X, (int)_position.Y, sizeX, sizeY); }
        //public Cube Cube { get => new(_position, sizeX, sizeY, sizeZ);}
        public Vector3 dirVector { get => General.angleToVector3(Angle); }


    }
}
