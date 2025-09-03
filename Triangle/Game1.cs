using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using random_generation_in_a_pixel_grid;
namespace Triangle
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player _player;
        private Camera _camera;
        private bool PreviousIsMouseVisible = false;
        private Point screenSize = new Point(800, 800);
        private int pixelSize = 4;
        private float FOV = MathHelper.Pi / 2;
        private BoundingFrustum frustrum;
        private KeyboardState _keyboardState;
        private KeyboardState _previousKeyboardState;
        private List<Triangle> Triangles = new List<Triangle>();
        private List<Model> Models = new List<Model>();
        private TextureBuffer _screenBuffer;
        private Texture2D screenTextureBuffer;
        private FramesPerSecondTimer framesPerSecondTimer = new();
        private seedMapper seedMapper;
        private const int MapCellSize = 40;
        Random rnd = new Random();

        //private BoundingBox;

        private Texture2D blankTexture; //for testing purposes
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            /* resizes screen */
            _graphics.PreferredBackBufferWidth = screenSize.X;
            _graphics.PreferredBackBufferHeight = screenSize.Y;
            _graphics.ApplyChanges();

            /* Initilizes blank texture as a 1x1 white pixel texture */
            blankTexture = new Texture2D(GraphicsDevice, 1, 1);
            blankTexture.SetData(new Color[] { Color.White });

            /* Inilizes random for testing */


            /* randomly places many orbs around for testing */
            for (int i = 0; i < 1750; i++)
            {
                int quality = rnd.Next(5, 10);
                Models.Add(new Sphere(new Vector3(rnd.Next(-1000, 1000), rnd.Next(-1000, 1000), rnd.Next(-1000, 1000)), quality * 5, quality));
            }
            // */

            /* randomly places many cubes around for testing 
            int size = 40;
            for (int x = 0; x < 25; x++)
            {
                for (int y = 0; y < 25; y++)
                {
                    for (int z = 0; z < 25; z++)
                    {
                        Models.Add(new Cube(new Vector3(size * x, size * y, size * z), size, size, size));
                    }
                }
            }
            // */

            /* Spawns a Cube at the origin point with a height width and depth of 100 */
            //Cubes.Add(new Cube(new Vector3(100, 0, 0), 100, 100, 100));
            // */

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _screenBuffer = new TextureBuffer(screenSize.X / pixelSize, screenSize.Y / pixelSize, Color.Transparent);

            Triangle.Initialize(_spriteBatch, _screenBuffer);
            Square.Initialize(_spriteBatch, _screenBuffer);

            _player = new Player(
                Vector3.Zero,
                new Camera(screenSize.Y / screenSize.X, Vector3.Zero, Vector3.Zero, Vector3.Down, 2000, FOV)
                );

            seedMapper = new seedMapper(
                100,
                100,
                new int[] { 9, 10 },
                1000,
                rnd.Next(1, int.MaxValue)
                );

            for (int i = 0; i <= 5; i++) { seedMapper.SmoothenTerrain(); }
            seedMapper.SmoothenHeights(10);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _keyboardState = Keyboard.GetState();

            IsMouseVisible = !IsActive;

            _player.SafeControlAngleWithMouse(PreviousIsMouseVisible, IsMouseVisible, screenSize, 0.01f);
            _player.Update(_keyboardState);
            _player.Move(_player.Speed);


            Square.UpdateConstants(FOV);
            Triangle.UpdateConstants(FOV, screenSize);

            int playerXIndex = (int)_player.Position.X / MapCellSize;
            int playerYIndex = (int)_player.Position.Z / MapCellSize;
            int terrainHeightAtPlayerPosition = seedMapper.Heights[playerXIndex, playerYIndex] - Player.sizeY;
            if (terrainHeightAtPlayerPosition < _player.Position.Y)
            {
                _player.SetPosition(
                new Vector3(
                    _player.Position.X,
                    terrainHeightAtPlayerPosition,
                    _player.Position.Z
                ));
                _player.HitGround(_keyboardState);
                if (General.OnPress(_keyboardState, _previousKeyboardState, Keys.Space))
                {
                    _player.Jump();
                }
            }
            else if (General.OnPress(_keyboardState, _previousKeyboardState, Keys.LeftShift))
            {
                _player.Dash();
            }

            _previousKeyboardState = _keyboardState;
            PreviousIsMouseVisible = IsMouseVisible;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DateTime startTime = DateTime.Now;

            List<Shape> VisibleShapes = new();
            List<Color> ShapesColors = new();
            BoundingFrustum viewFrustrum = _player.PlayerCamera.Frustum;
            var heightMap = seedMapper.Heights;
            var valueMap = seedMapper.Values;
            var Colors = new Color[] { Color.Aqua, Color.Green };

            for (int y = 0; y < seedMapper.height - 1; y++)
            {
                for (int x = 0; x < seedMapper.width - 1; x++)
                {
                    int p1x = x;
                    int p1y = y;
                    Vector3 p1 = new Vector3(p1x * MapCellSize, heightMap[p1x, p1y], p1y * MapCellSize);
                    var p2x = x + 1;
                    int p2y = y;
                    Vector3 p2 = new Vector3(p2x * MapCellSize, heightMap[p2x, p2y], p2y * MapCellSize);
                    var p3x = x + 1;
                    int p3y = y + 1;
                    Vector3 p3 = new Vector3(p3x * MapCellSize, heightMap[p3x, p3y], p3y * MapCellSize);
                    int p4x = x;
                    int p4y = y + 1;
                    Vector3 p4 = new Vector3(p4x * MapCellSize, heightMap[p4x, p4y], p4y * MapCellSize);
                    VisibleShapes.Add(new Square(p1, p2, p3, p4));
                    ShapesColors.Add(Colors[valueMap[p4x, p4y]]);
                }
            }

            foreach (Model model in Models)
            {
                BoundingBox boundingBox = model.BoundingBox;
                var value = viewFrustrum.Contains(boundingBox);
                if (value != ContainmentType.Disjoint)
                {
                    Shape[] shapes = model.Shapes;
                    VisibleShapes.AddRange(shapes);
                    for (int i = 0; i < shapes.Length; i++)
                    {
                        ShapesColors.Add(Color.DarkGray);
                    }
                }
            }

            Vector3 lightSource = new Vector3(0, 0, 0);
            for (int i = 0; i < VisibleShapes.Count; i++)
            {
                Vector3 trianglePos = VisibleShapes[i].Position;
                int distance = (int)Vector3.Distance(trianglePos, _player.Center);
                //Color color = triangle.ApplyShading(trianglePos - lightSource, Color.Black, Color.White);
                VisibleShapes[i].Draw(ref _screenBuffer, ShapesColors[i], _player.Center, _player._angle.Y, _player._angle.X, distance);
            }
            framesPerSecondTimer.update();

            Debug.WriteLine("DrawTime: " + (DateTime.Now - startTime).Milliseconds);
            Debug.WriteLine("FPS: " + framesPerSecondTimer.FPS);

            _screenBuffer.applyDepth(300);
            _screenBuffer.ToTexture2D(GraphicsDevice, out screenTextureBuffer);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(screenTextureBuffer, new Rectangle(0, 0, screenSize.X, screenSize.Y), Color.White);
            _spriteBatch.End();


            screenTextureBuffer.Dispose();

            _screenBuffer.Clear(Color.Transparent);

            base.Draw(gameTime);
        }
    }
}
