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
using Triangle;
namespace Triangle
{
    internal class Player
    {
        static Texture2D _texture;
        static int sizeX = 30;
        static int sizeY = 70;
        static int sizeZ = 30;
        Vector3 _position = new();
        Vector3 _speed = new();
        public Vector2 _angle = Vector2.Zero;
        private static readonly string _saveDirectory = Path.Combine(Environment.CurrentDirectory, "PlayerInfo", "PlayerPos.txt");

        public enum GameMode
        {
            Survival,
            Creative
        }

        GameMode gameMode = GameMode.Creative;

        public Player(Vector3 position)
        {
            _position = position;
            TryPullPositionFromArchive();
        }
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
        public void update(KeyboardState keyboardState)
        {
            if (IsSurvival)
            {
                _speed.Y += 0.5f;
            }
            Vector2 normalizedSpeed = new();
            MoveKeyPressed(keyboardState);
            normalizedSpeed = General.Normalize(normalizedSpeed, 1f);

            _speed.X += normalizedSpeed.X;
            _speed.Z += normalizedSpeed.Y;

            _speed.X *= .75f;
            _speed.Y *= .95f;
            _speed.Z *= .75f;

        }
        public void Jump()
        {
            _speed.Y -= 10;
        }
        public void Move(Vector3 vector)
        {
            _position += vector;
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
            Vector2 speedChange = new();

            if (keyboardState.IsKeyDown((Keys)Direction.FORWARD))
            {
                speedChange += General.AngleToVector2(_angle.X);
            }
            if (keyboardState.IsKeyDown((Keys)Direction.BACKWARDS))
            {
                speedChange -= General.AngleToVector2(_angle.X);
            }
            if (keyboardState.IsKeyDown((Keys)Direction.LEFT))
            {
                speedChange += General.AngleToVector2(_angle.X - (float)Math.PI / 2);
            }
            if (keyboardState.IsKeyDown((Keys)Direction.RIGHT))
            {
                speedChange += General.AngleToVector2(_angle.X + (float)Math.PI / 2);
            }
            if (IsCreative)
            {
                if (keyboardState.IsKeyDown((Keys)Direction.DOWN))
                {
                    _speed.Y += 1;
                }
                if (keyboardState.IsKeyDown((Keys)Direction.UP))
                {
                    _speed.Y -= 1;
                }
            }

            if (speedChange != new Vector2(0, 0))
            {
                speedChange.Normalize();
                _speed.X += speedChange.X * 1;
                _speed.Z += speedChange.Y * 1;
            }
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
        }
        public bool IsSurvival { get => gameMode == GameMode.Survival;}
        public bool IsCreative { get => gameMode == GameMode.Creative;}
        public Vector2 XY { get => new(_position.X, _position.Y);}
        public Vector3 Speed { get => _speed;}
        public Vector3 Position { get => _position;}
        public Vector3 Center { get => _position + new Vector3(sizeX, sizeY, sizeZ) / 2;}
        public Rectangle Rectangle { get => new((int)_position.X, (int)_position.Y, sizeX, sizeY);}
        //public Cube Cube { get => new(_position, sizeX, sizeY, sizeZ);}
        public Vector3 dirVector { get => General.angleToVector3(_angle); }


    }
}
