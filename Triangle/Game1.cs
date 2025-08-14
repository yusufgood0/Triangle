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
            for (int i = 0; i < 1; i++)
            {
                Triangles.Add(new Triangle(p1, p3, p4));
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Triangle.Initialize(_spriteBatch, screenSize);

            _player = new Player(Vector3.Zero);
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _keyboardState = Keyboard.GetState();

            _player.SafeControlAngleWithMouse(PreviousIsMouseVisible, IsMouseVisible, screenSize, 0.01f);
            _player.update(_keyboardState);
            _player.Move(_player.Speed);

            Triangle.UpdateConstants(FOV, screenSize);

            PreviousIsMouseVisible = IsMouseVisible;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here\
            //General.DrawObject(_spriteBatch, texture, screenSize, FOV);
            DateTime startTime = DateTime.Now;

            //foreach (Triangle triangle in Triangles)
            //{
            //    int distanceSquared = 0;
            //    int colorValue = 255 - distanceSquared;
            //    triangle.Draw(_spriteBatch, Color.White, _player.Center, _player._angle.Y, _player._angle.X, 1);
            //}

            Triangle.BulkDraw(Triangles, ref _spriteBatch, Color.White, _player.Center, _player._angle.Y, _player._angle.X);
            Debug.WriteLine((DateTime.Now - startTime).Milliseconds);
            base.Draw(gameTime);
        }
    }
}
