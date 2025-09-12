using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using random_generation_in_a_pixel_grid;
using static System.Collections.Specialized.BitVector32;
namespace Triangle
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player _player;
        private bool _previousIsMouseVisible = false;
        private Point screenSize;
        private int pixelSize = 4;
        private float FOV = MathHelper.Pi / 1.5f;
        private KeyboardState _keyboardState;
        private KeyboardState _previousKeyboardState;
        private MouseState _mouseState;
        private MouseState _previousMouseState;
        private List<Model> Models = new List<Model>();
        private TextureBuffer _screenBuffer;
        private Texture2D screenTextureBuffer;
        private FramesPerSecondTimer framesPerSecondTimer = new();
        private SeedMapper seedMapper;
        private Point mapSize = new(300, 300);
        private Point playerTileIndex;
        private const int MapCellSize = 40;
        private List<Projectile> _projectiles = new List<Projectile>();
        private Random rnd = new Random();
        private SpellBook _spellbook = new SpellBook();
        private List<SquareParticle> _squareParticles = new();
        private int _screenShake;
        private Vector3 lightSource;
        private CrystalBall _crystallBall = new CrystalBall(new Vector3(40, 40, 50));

        private List<Action> KeyBinds = new();

        //private Texture2D blankTexture; //for testing purposes
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {

            /* resizes screen */
            screenSize.X = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            screenSize.Y = GraphicsDevice.Adapter.CurrentDisplayMode.Height;

            _graphics.PreferredBackBufferWidth = screenSize.X;
            _graphics.PreferredBackBufferHeight = screenSize.Y;
            _graphics.ApplyChanges();

            /* Initilizes blank texture as a 1x1 white pixel texture 
            blankTexture = new Texture2D(GraphicsDevice, 1, 1);
            blankTexture.SetData(new Color[] { Color.White });
            // */

            /* Initilize Keybinds with early values */
            KeyBinds.Add(new Action(Keys.D1, ActionCatagory.AddElement, (int)Element.Fire));
            KeyBinds.Add(new Action(Keys.D2, ActionCatagory.AddElement, (int)Element.Water));
            KeyBinds.Add(new Action(Keys.D3, ActionCatagory.AddElement, (int)Element.Air));
            KeyBinds.Add(new Action(Keys.D4, ActionCatagory.AddElement, (int)Element.Earth));
            KeyBinds.Add(new Action(Keys.Q, ActionCatagory.CastSpell, 0));

            /* randomly places many orbs around for testing 
            for (int i = 0; i < 1250; i++)
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

            /* Spawns a Cube at the origin point with a height width and depth of 100 
            Cubes.Add(new Cube(new Vector3(100, 0, 0), 100, 100, 100));
            // */

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _screenBuffer = new TextureBuffer(screenSize.X / pixelSize, screenSize.Y / pixelSize, Color.Transparent);
            Triangle.Initialize(_spriteBatch, _screenBuffer);
            Square.Initialize(_spriteBatch, _screenBuffer);

            Vector2 MapCenter = new Vector2(mapSize.X / 2 * MapCellSize, mapSize.Y / 2 * MapCellSize);
            lightSource = new(MapCenter.X, -1000, MapCenter.Y);
            Vector3 PlayerPos = new(MapCenter.X, 0, MapCenter.Y);

            /* Initilize player object */
            _player = new Player(
                PlayerPos,
                new Camera(screenSize.Y / screenSize.X, PlayerPos, Vector3.Zero, Vector3.Down, 5000, FOV * 1.2f)
                );

            /* Load Map */
            seedMapper = new SeedMapper(
                mapSize.X,
                mapSize.Y,
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
            _mouseState = Mouse.GetState();

            IsMouseVisible = !IsActive;

            _player.SafeControlAngleWithMouse(_previousIsMouseVisible, IsMouseVisible, screenSize, 0.01f);
            _player.Update(_keyboardState);
            _player.Move(_player.Speed);
            float speedSquared =
                _player.Speed.X * _player.Speed.X +
                _player.Speed.Y * _player.Speed.Y +
                _player.Speed.Z * _player.Speed.Z;
            int TargetScreenShake = (int)speedSquared / 200;
            _screenShake = Math.Max(_screenShake - 1, TargetScreenShake);

            Square.UpdateConstants(FOV);
            Triangle.UpdateConstants(FOV);
            for (int i = 0; i < _squareParticles.Count; i++)
            {
                var particle = _squareParticles[i];
                int LifeTime = (int)(DateTime.Now - particle.CreationTime).TotalMilliseconds;
                if (rnd.Next(0, LifeTime) > 750)
                {
                    _squareParticles.RemoveAt(i);
                    i--;
                    continue;
                }
                particle.Float(4);
            }
            for (int i = 0; i < _projectiles.Count; i++)
            {
                var projectile = _projectiles[i];
                if (projectile.Move(seedMapper, MapCellSize))
                {
                    _squareParticles.AddRange(projectile.HitGround(rnd));
                    _projectiles.Remove(projectile);
                    i--;
                    continue;
                }

                var particle = projectile.GetParticles(rnd);
                if (particle == null) { continue; }
                _squareParticles.Add((SquareParticle)particle);
            }

            playerTileIndex = new Point(
                (int)(_player.Position.X / MapCellSize),
                (int)(_player.Position.Z / MapCellSize)
                );
            var terrainHeightAtPlayerPosition = seedMapper.HeightAtPosition(_player.Position, MapCellSize) - Player.sizeY;

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


            if (_keyboardState.GetPressedKeyCount() > 0 || _previousKeyboardState.GetPressedKeyCount() > 0)
            {
                foreach (Action action in KeyBinds)
                {
                    if (action.ActionType == ActionCatagory.AddElement && General.OnPress(_keyboardState, _previousKeyboardState, action.Key))
                    {
                        if (_spellbook.ElementsCount == 3)
                        {
                            _spellbook.TryCast(_projectiles, ref _player, ref _squareParticles, ref _screenShake);
                        }
                        _spellbook.AddElement((Element)action.Value);
                    }
                    else if (action.ActionType == ActionCatagory.CastSpell && General.OnPress(_keyboardState, _previousKeyboardState, action.Key))
                    {
                        _spellbook.TryCast(_projectiles, ref _player, ref _squareParticles, ref _screenShake);
                    }
                }
                
            }

            _previousMouseState = _mouseState;
            _previousKeyboardState = _keyboardState;
            _previousIsMouseVisible = IsMouseVisible;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);

            DateTime startTime = DateTime.Now;

            List<Shape> VisibleShapes = new();
            List<Color> ShapesColors = new();
            BoundingFrustum viewFrustrum = _player.PlayerCamera.Frustum;
            var heightMap = seedMapper.Heights;
            var valueMap = seedMapper.Values;
            var Colors = new Color[] { Color.Gray, Color.Green };

            int renderDistance = 100;
            int startIndexX = Math.Max(0, playerTileIndex.X - renderDistance);
            int startIndexY = Math.Max(0, playerTileIndex.Y - renderDistance);
            int endIndexX = Math.Min(seedMapper.width - 1, playerTileIndex.X + renderDistance);
            int endIndexY = Math.Min(seedMapper.height - 1, playerTileIndex.Y + renderDistance);
            Rectangle screenRect = new Rectangle(Point.Zero, screenSize);
            for (int y = startIndexY; y < endIndexY; y++)
            {
                for (int x = startIndexX; x < endIndexX; x++)
                {
                    int p1TrueX = x * MapCellSize;
                    int p1TrueY = y * MapCellSize;

                    Vector3 p1 = new Vector3(p1TrueX, heightMap[x, y], p1TrueY);

                    //if (!(Triangle.WorldPosToScreenPos(_player.EyePos, _player._angle.Y, _player._angle.X, p1, out Point screenPos) && screenRect.Contains(screenPos))) 
                    if (viewFrustrum.Contains(p1) == ContainmentType.Disjoint)
                    {
                        continue;
                    }

                    int yPlus1 = y + 1;
                    int xPlus1 = x + 1;

                    int p2x = xPlus1;
                    int p2y = y;
                    Vector3 p2 = new Vector3(p1TrueX + MapCellSize, heightMap[p2x, p2y], p1TrueY);
                    int p3x = xPlus1;
                    int p3y = yPlus1;
                    Vector3 p3 = new Vector3(p1TrueX + MapCellSize, heightMap[p3x, p3y], p1TrueY + MapCellSize);
                    int p4x = x;
                    int p4y = yPlus1;
                    Vector3 p4 = new Vector3(p1TrueX, heightMap[p4x, p4y], p1TrueY + MapCellSize);
                    VisibleShapes.Add(new Square(p1, p2, p3, p4));
                    ShapesColors.Add(Colors[valueMap[p4x, p4y]]);
                }
            }
            for (int i = 0; i < _projectiles.Count; i++)
            {
                Model model = _projectiles[i].Model;
                BoundingBox boundingBox = model.BoundingBox;
                var value = viewFrustrum.Contains(boundingBox);
                if (value != ContainmentType.Disjoint)
                {
                    Shape[] shapes = model.Shapes;
                    VisibleShapes.AddRange(shapes);
                    for (int j = 0; j < shapes.Length; j++)
                    {
                        ShapesColors.Add(_projectiles[i].Color);
                    }
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

            _crystallBall.SetPosition(_player);
            Shape[] orbShapes = _crystallBall.Model.Shapes;
            VisibleShapes.AddRange(orbShapes);

            _crystallBall.UpdateHighlights(rnd);
            Color[] colors = _spellbook.ElementColors;
            Color backGroundColor = _spellbook.ElementsCount == 3 ? Color.MediumPurple : Color.Black;
            for (int i = 0; i < orbShapes.Length; i++)
            {
                int iPlusSwirlPosition = _crystallBall.SwirlPos + i;
                int columns = CrystalBall.SphereQuality + 1;
                int lerpAmount = iPlusSwirlPosition % columns;
                int colorIndex = iPlusSwirlPosition / columns % 3;
                Color color = colors[colorIndex];
                if (color == Color.Black)
                {
                    ShapesColors.Add(backGroundColor);
                    continue;
                }
                ShapesColors.Add(Color.Lerp(color, backGroundColor, lerpAmount / (float)columns));
            }

            /* Draw Shapes */
            for (int i = 0; i < VisibleShapes.Count; i++)
            {
                Shape shape = VisibleShapes[i];

                Vector3 shapePos = shape.Position;
                int distance = (int)Vector3.Distance(shapePos, _player.EyePos);

                Color color = shape.ApplyShading(shapePos - lightSource, ShapesColors[i], Color.LightGoldenrodYellow);

                shape.Draw(ref _screenBuffer, color, _player.EyePos, _player._angle.Y, _player._angle.X, distance);
            }

            /* Draw Particles */
            foreach (var Particle in _squareParticles)
            {
                Shape shape = Particle.Square;

                Vector3 shapePos = shape.Position;
                int distance = (int)Vector3.Distance(shapePos, _player.EyePos);

                shape.Draw(ref _screenBuffer, Particle.Color, _player.EyePos, _player._angle.Y, _player._angle.X, distance);
            }

            /* FPS Counter */
            if (framesPerSecondTimer.update())
            {
                /* Runs Every second */
                Debug.WriteLine("DrawTime: " + (DateTime.Now - startTime).Milliseconds);
                Debug.WriteLine("FPS: " + framesPerSecondTimer.FPS);
            }


            Point shake = new Point(rnd.Next(-_screenShake, _screenShake), rnd.Next(-_screenShake, _screenShake));

            _screenBuffer.applyDepth(800);
            _screenBuffer.ToTexture2D(GraphicsDevice, out screenTextureBuffer);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(screenTextureBuffer, new Rectangle(shake, screenSize), Color.White);
            _spriteBatch.End();


            screenTextureBuffer.Dispose();

            _screenBuffer.Clear(Color.DarkSlateBlue);

            base.Draw(gameTime);
        }
    }
}
