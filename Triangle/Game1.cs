using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Triangle
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player _player;
        private bool PreviousIsMouseVisible = false;
        private Point screenSize = new Point(800, 800);
        private float FOV = 2;
        private KeyboardState _keyboardState;
        private List<Triangle> Triangles = new List<Triangle>();
        private TextureBuffer _screenBuffer;
        private Texture2D screenTextureBuffer;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = screenSize.X;
            _graphics.PreferredBackBufferHeight = screenSize.Y;
            _graphics.ApplyChanges();

            Vector3 p1 = new Vector3(0, 0, 0);
            Vector3 p2 = new Vector3(100, 0, 0);
            Vector3 p3 = new Vector3(0, 100, 0);
            Vector3 p4 = new Vector3(0, 0, 100);
            Triangles.Add(new Triangle(p1, p3, p4));
            Triangles.Add(new Triangle(p2, p3, p4));
            Triangles.Add(new Triangle(p1, p2, p4));
            Triangles.Add(new Triangle(p1, p3, p2));
            //for (int i = 0; i < 100; i++)
            //{
            //    Triangles.Add(new Triangle(p1, p3, p4));
            //}

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _screenBuffer = new TextureBuffer(screenSize.X, screenSize.Y, Color.Transparent);

            Triangle.Initialize(_spriteBatch, screenSize);

            _player = new Player(Vector3.Zero);
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _keyboardState = Keyboard.GetState();

            IsMouseVisible = !IsActive;

            _player.SafeControlAngleWithMouse(PreviousIsMouseVisible, IsMouseVisible, screenSize, 0.01f);
            _player.update(_keyboardState);
            _player.Move(_player.Speed);

            Triangle.UpdateConstants(FOV, screenSize);

            PreviousIsMouseVisible = IsMouseVisible;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            // TODO: Add your drawing code here\
            //General.DrawObject(_spriteBatch, texture, screenSize, FOV);
            DateTime startTime = DateTime.Now;

            foreach (Triangle triangle in Triangles)
            {
                int distanceSquared = (int)Vector3.Distance(triangle.Average, _player.Center);
                int colorValue = 255 - Math.Min(distanceSquared, 255);
                triangle.Draw(ref _screenBuffer, new Color(colorValue, colorValue, colorValue), _player.Center, _player._angle.Y, _player._angle.X, distanceSquared);
            }

            //Triangle.BulkDraw(Triangles, ref _spriteBatch, Color.White, _player.Center, _player._angle.Y, _player._angle.X);
            Debug.WriteLine((DateTime.Now - startTime).Milliseconds);

            _screenBuffer.ToTexture2D(GraphicsDevice, out screenTextureBuffer);
            _spriteBatch.Begin();
            _spriteBatch.Draw(screenTextureBuffer, new Rectangle(0, 0, screenSize.X, screenSize.Y), Color.White);
            _spriteBatch.End();

            screenTextureBuffer.Dispose();

            _screenBuffer.Clear(Color.Transparent);

            base.Draw(gameTime);
        }
    }
}
