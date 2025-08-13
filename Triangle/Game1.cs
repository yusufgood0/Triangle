using System.Collections.Generic;
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
        private Point screenSize = new Point(400, 400);
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
            base.Initialize();
            Triangles.Add(new Triangle(new(100, 0, 0), new(0, 100, 0), new(0, 0, 100)));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
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
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here\
            //General.DrawObject(_spriteBatch, texture, screenSize, FOV);
            foreach (Triangle triangle in Triangles)
            {
                triangle.Draw(_spriteBatch, GraphicsDevice, Color.FloralWhite, _player.Center, _player._angle.Y, _player._angle.X);
            }

            base.Draw(gameTime);
        }
    }
}
