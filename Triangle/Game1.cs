using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
        private List<Cube> Cubes = new List<Cube>();
        private  static List<Sphere> Spheres = new();
        private TextureBuffer _screenBuffer;
        private Texture2D screenTextureBuffer;

        private Texture2D blankTexture; //for testing purposes
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

            blankTexture = new Texture2D(GraphicsDevice, 1, 1);
            blankTexture.SetData(new Color[] { Color.White });

            Random rnd = new Random();

            //for (int i = 0; i < 10000; i++)
            //{
            //    Triangles.Add(
            //        new(
            //            new(rnd.Next(-0, 100), rnd.Next(-0, 100), rnd.Next(-0, 100)),
            //            new(rnd.Next(-0, 100), rnd.Next(-0, 100), rnd.Next(-0, 100)),
            //            new(rnd.Next(-0, 100), rnd.Next(-0, 100), rnd.Next(-0, 100))
            //            ));
            //}

            for (int i = 0; i < 1750; i++)
            {
                int quality = rnd.Next(5, 10);
                Spheres.Add(new Sphere(new Vector3(rnd.Next(-1000, 1000), rnd.Next(-1000, 1000), rnd.Next(-1000, 1000)), quality*5, quality));
            }
            Cubes.Add(new Cube(new(100, 0, 0), 100, 100, 100));

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
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // TODO: Add your drawing code here\
            //General.DrawObject(_spriteBatch, texture, screenSize, FOV);
            DateTime startTime = DateTime.Now;
            int triangleCount = 0;
            Vector3 lightSource = new Vector3(0, 0, 0);
            object triangleLock = new object();

            foreach (Triangle triangle in Triangles)
            {
                int distance = (int)Vector3.Distance(triangle.Average, _player.Center);
                triangle.Draw(ref _screenBuffer, Color.White, _player.Center, _player._angle.Y, _player._angle.X, distance);
            }
            foreach (Cube cube in Cubes)
            {
                foreach (Square square in cube.getSquares())
                {
                    Vector3 trianglePos = square.Average;
                    int distance = (int)Vector3.Distance(trianglePos, _player.Center);
                    Color color = square.ApplyShading(trianglePos - lightSource, Color.Red, Color.White);
                    square.Draw(ref _screenBuffer, color, _player.Center, _player._angle.Y, _player._angle.X, distance);
                }
            }
            Parallel.ForEach(Spheres, sphere =>
            {
                foreach (Triangle triangle in sphere.GetTriangles)
                {
                    Vector3 trianglePos = triangle.Average;
                    int distance = (int)Vector3.Distance(trianglePos, _player.Center);
                    Color color = triangle.ApplyShading(trianglePos - lightSource, Color.Black, Color.White);
                    triangle.Draw(ref _screenBuffer, color, _player.Center, _player._angle.Y, _player._angle.X, distance);
                    lock (triangleLock)
                    {
                        triangleCount++;
                    }
                }
            });
            Debug.WriteLine($"Triangles drawn: {triangleCount}");
            Debug.WriteLine((DateTime.Now - startTime).Milliseconds);
            var a = _player.dirVector;

            _screenBuffer.applyDepth();
            _screenBuffer.ToTexture2D(GraphicsDevice, out screenTextureBuffer);
            _spriteBatch.Begin();
            _spriteBatch.Draw(screenTextureBuffer, new Rectangle(0, 0, screenSize.X, screenSize.Y), Color.White);
            //Sphere.drawVerticies(_spriteBatch, blankTexture, screenSize, FOV, _player.Center, _player._angle.Y, _player._angle.X, 50, 50, null, Color.BurlyWood);
            _spriteBatch.End();


            screenTextureBuffer.Dispose();

            _screenBuffer.Clear(Color.Transparent);

            base.Draw(gameTime);
        }
    }
}
