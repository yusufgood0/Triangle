using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Enemies;
using SlimeGame.GameAsset;
using SlimeGame.Generation;
using SlimeGame.Input;
using SlimeGame.Menus;
using SlimeGame.Models;
using SlimeGame.Models.Shapes;
namespace SlimeGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private Texture2D _blankTexture; //for testing purposes
        private Texture2D _gradiantSquare; //for buttons
        private Color _defaultButtonColor = Color.DarkSlateBlue;
        private Player _player;
        private bool _previousIsMouseVisible = false;
        private Point _screenSize;
        private int _pixelSize = 4;
        private float _FOV = MathHelper.Pi / 1.5f;
        private bool _fillScreen = false;
        private Rectangle _drawParemeters;
        private float _sensitivity = 0.01f;
        private MasterInput _masterInput = new MasterInput();
        private MouseState _mouseState;
        private MouseState _previousMouseState;
        private List<GenericModel> _models = new List<GenericModel>();
        private TextureBuffer _screenBuffer;
        private FramesPerSecondTimer _framesPerSecondTimer = new();
        private SeedMapper _seedMapper;
        private Point mapSize = new(400, 400);
        private Point playerTileIndex;
        private const int MapCellSize = 40;
        private List<Projectile> _projectiles = new List<Projectile>();
        private Random rnd = new Random();
        private SpellBook _spellbook = new SpellBook();
        private List<SquareParticle> _squareParticles = new();
        private CrystalBall _crystalBall = new CrystalBall(new Vector3(40, 40, 50));
        private List<Enemy> Enemies = new();
        private PauseMenu _pauseMenu;
        private SettingsMenu _settingsMenu;
        private gamestates _currentGameState = gamestates.Playing;
        WorldDraw worldDraw;
        VertexPositionColorNormal[] MapVertexPositionColorNormal;
        private bool Playing => _currentGameState == gamestates.Playing;
        private bool Paused => _currentGameState == gamestates.Paused;
        private bool SettingsMenu => _currentGameState == gamestates.SettingsMenu;
        private enum gamestates
        {
            Playing,
            Paused,
            SettingsMenu
        }
        public enum Actions
        {
            AddFire = 0,
            AddWater = 1,
            AddAir = 2,
            AddEarth = 3,
            CastSpell = 4
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = false;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {

            /* resizes screen */
            _screenSize.X = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            _screenSize.Y = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            _screenSize.X = 800;
            _screenSize.Y = 800;
            _graphics.PreferredBackBufferWidth = _screenSize.X;
            _graphics.PreferredBackBufferHeight = _screenSize.Y;
            _graphics.ApplyChanges();



            /* Initilizes blank texture as a 1x1 white pixel texture */
            _blankTexture = new Texture2D(GraphicsDevice, 1, 1);
            _blankTexture.SetData(new Color[] { Color.White });
            // */


            /* randomly places many orbs around for testing 
            for (int i = 0; i < 500; i++)
            {
                int quality = rnd.Next(5, 10);
                _models.Add(new Sphere(new Vector3(rnd.Next(-10000, 10000), rnd.Next(-10000, 10000), rnd.Next(-10000, 10000)), quality * 50, quality, Color.Green));
            }
            // */

            /* randomly places many cubes around for testing  
            int size = 400;
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    for (int z = 0; z < 5; z++)
                    {
                        Vector3 pos = new Vector3(size * x * 2, size * y * -2, size * z * 2);
                        _models.Add(new Cube(pos, size, size, size));
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
            _screenBuffer = new TextureBuffer(_screenSize.X / _pixelSize, _screenSize.Y / _pixelSize, Color.Transparent);
            _spriteFont = Content.Load<SpriteFont>("GameFont");
            _gradiantSquare = Content.Load<Texture2D>("GradiantSquare");

            Triangle.Initialize(_screenBuffer);
            Square.Initialize(_screenBuffer);
            Vector2 MapCenter = new Vector2(mapSize.X / 2 * MapCellSize, mapSize.Y / 2 * MapCellSize);
            if (_fillScreen)
            {
                _drawParemeters = new Rectangle(0, 0, _screenSize.X, _screenSize.Y);
            }
            else
            {
                int paddingx = Math.Max((_screenSize.X - _screenSize.Y) / 2, 0);
                int paddingy = Math.Max((_screenSize.Y - _screenSize.X) / 2, 0);
                int screenWidthHeight = Math.Min(_screenSize.Y, _screenSize.X);

                _drawParemeters = new Rectangle(paddingx, paddingy, screenWidthHeight, screenWidthHeight);
            }
            _pauseMenu = new PauseMenu(GraphicsDevice, _spriteFont, _drawParemeters, _defaultButtonColor);
            _settingsMenu = new ElementKeybindMenu(GraphicsDevice, _spriteFont, _masterInput, _drawParemeters, _defaultButtonColor);

            int p1x = 0, p1y = 0, p2x = 0, p2y = 0, p3x = 0, p3y = 0, p4x = 0, p4y = 0;


            /* Load Map */
            Random rnd = new Random();
            _seedMapper = new SeedMapper(
                mapSize.X,
                mapSize.Y,
                new int[] { 9, 10 },
                0,
                rnd.Next(1, int.MaxValue)
                );

            int H = _seedMapper.height;
            int W = _seedMapper.width;
            _seedMapper.SmoothenHeights(10);

            for (int i = 0; i < 5; i++)
            {
                (p1x, p1y) = (0, rnd.Next(0, H));
                (p2x, p2y) = (W, rnd.Next(0, H));
                (p3x, p3y) = (rnd.Next(0, W), 0);
                (p4x, p4y) = (rnd.Next(0, W), H);
                if (rnd.Next(0, 2) == 1) { (p1x, p2x) = (p2x, p1x); }
                if (rnd.Next(0, 2) == 1) { (p3y, p4y) = (p4y, p3y); }
                if (rnd.Next(0, 2) == 1) { (p1x, p2x) = (p4y, p3y); }
                if (rnd.Next(0, 2) == 1) { (p2x, p1x) = (p4y, p3y); }
                _seedMapper.BezierSmoother(10,
                    p1x, p1y,
                    p2x, p2y,
                    p3x, p3y,
                    p4x, p4y
                );
            }
            _seedMapper.SmoothenHeights(15);
            _seedMapper.ApplySeaLevel(-50);

            var (PlayerX, PlayerY) = _seedMapper.CubicBezier(0.1f,
                p1x, p1y,
                p2x, p2y,
                p3x, p3y,
                p4x, p4y
                );
            var PlayerPos =
            new Vector3(
            PlayerX * MapCellSize,
            _seedMapper.Heights[PlayerX, PlayerY] + 50,
            PlayerY * MapCellSize
            );


            Enemies.Add(new Archer(PlayerPos));
            Enemies.Add(new Archer(PlayerPos));
            Enemies.Add(new Archer(PlayerPos));

            /* Initilize player object */
            _player = new Player(
                PlayerPos,
                new Camera(_screenSize.Y / _screenSize.X, PlayerPos, Vector3.Zero, Vector3.Down, 15000, _FOV * 1.2f) //bloats FOV to ensure no clipping on camera
                );

            worldDraw = new WorldDraw(
                GraphicsDevice,
                doubleSided: true,
                texture: Content.Load<Texture2D>("GradiantSquare")
                );

            MapVertexPositionColorNormal = new VertexPositionColorNormal[_seedMapper.height * _seedMapper.width];
            var valueMap = _seedMapper.Values;
            var Colors = new Color[] { Color.DarkSeaGreen, Color.DarkSlateGray };

            for (int y = 0; y < _seedMapper.height; y++)
            {
                for (int x = 0; x < _seedMapper.width; x++)
                {
                    int index = y * _seedMapper.width + x;

                    MapVertexPositionColorNormal[index].Position = new Vector3(
                        x * MapCellSize,
                        _seedMapper.Heights[x, y],
                        y * MapCellSize
                        );


                }
            }
            for (int y = 0; y < _seedMapper.height - 1; y++)
            {
                for (int x = 0; x < _seedMapper.width - 1; x++)
                {
                    int index = y * _seedMapper.width + x;
                    Vector3 p1 = MapVertexPositionColorNormal[index].Position;
                    Vector3 p2 = MapVertexPositionColorNormal[index + 1].Position;
                    Vector3 p3 = MapVertexPositionColorNormal[index + 1 + _seedMapper.width].Position;
                    Vector3 p4 = MapVertexPositionColorNormal[index + _seedMapper.width].Position;

                    MapVertexPositionColorNormal[index].Color = Colors[valueMap[x, y]];

                    Vector3 side1 = p2 - p1;
                    Vector3 side2 = p3 - p1;
                    Vector3 side3 = p4 - p1;

                    Vector3 normalDir1 = Vector3.Cross(side1, side2);
                    normalDir1.Normalize();
                    Vector3 normalDir2 = Vector3.Cross(side2, side3);
                    normalDir2.Normalize();

                    Vector3 normalDir = (normalDir1 + normalDir2) / 2;
                    MapVertexPositionColorNormal[index].Color = Color.Lerp(MapVertexPositionColorNormal[index].Color, Color.LightGoldenrodYellow, Vector3.Dot(normalDir, Vector3.Down) * 0.15f);
                }
            }

            Square.UpdateConstants(_FOV);
            Triangle.UpdateConstants(_FOV);
        }
        protected override void Update(GameTime gameTime)
        {
            if (_masterInput.OnPress(Buttons.Back) || _masterInput.OnPress(Keys.Escape) && _masterInput.OnPress(Keys.LeftAlt))
                Exit();

            _previousMouseState = _mouseState;
            _mouseState = Mouse.GetState();
            _previousIsMouseVisible = IsMouseVisible;
            _masterInput.UpdateStates();
            IsMouseVisible = !Playing || !IsActive;

            foreach (var modelIndex in Enumerable.Range(0, _models.Count))
            {
                _models[modelIndex].ChangeRotation(0.01f, 1f);
            }

            if (_masterInput.OnPress(KeybindActions.GamePadClick))
            {
                _mouseState = new MouseState(
                _mouseState.X, _mouseState.Y, _mouseState.ScrollWheelValue,                // X, Y, ScrollWheelValue
                ButtonState.Pressed,        // LeftButton
                _mouseState.MiddleButton,       // MiddleButton
                _mouseState.RightButton,       // RightButton
                _mouseState.XButton1,       // XButton1
                _mouseState.XButton2        // XButton2
            );
            }

            Vector2 controllerOffset = 10 * _masterInput.GamePadState.ThumbSticks.Right;
            controllerOffset.Y *= -1;

            if (controllerOffset != Vector2.Zero)
            {
                Point newMousePos = _mouseState.Position + controllerOffset.ToPoint();
                Mouse.SetPosition(newMousePos.X, newMousePos.Y);
            }


            if (Playing)
            {
                if (_masterInput.OnPress(KeybindActions.BackButton))
                {
                    _currentGameState = gamestates.Paused;
                }

                _player.SafeControlAngleWithMouse(_previousIsMouseVisible, IsMouseVisible, _screenSize, _sensitivity);
                _player.Update(_masterInput);

                foreach (Enemy enemy in Enemies)
                {
                    foreach (Enemy enemy2 in Enemies)
                    {
                        if (enemy.Hitbox.Intersects(enemy2.Hitbox))
                        {
                            enemy.Knockback(enemy2.Position);
                        }
                    }
                }
                for (int i = 0; i < _squareParticles.Count; i++)
                {
                    var particle = _squareParticles[i];
                    int LifeTime = (int)(DateTime.Now - particle.CreationTime).TotalSeconds;
                    if (LifeTime > 1)
                    {
                        _squareParticles.RemoveAt(i);
                        i--;
                        continue;
                    }
                    _squareParticles[i] = _squareParticles[i].Float(20, rnd);
                }
                for (int i = 0; i < Enemies.Count; i++)
                {
                    Enemies[i].Update(in _player, in _projectiles, in rnd, _seedMapper, MapCellSize);
                    if (Vector3.DistanceSquared(Enemies[i].Position, _player.Position) > 20000 * 20000)
                    {
                        Enemies.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                for (int i = 0; i < _projectiles.Count; i++)
                {
                    var projectile = _projectiles[i];
                    if (projectile.Move(_seedMapper, MapCellSize))
                    {
                        _squareParticles.AddRange(projectile.HitGround(rnd));
                        _projectiles.Remove(projectile);
                        i--;
                        continue;
                    }
                    bool HitSomething = false;
                    if (projectile.TargetType == TargetType.Enemy)
                    {
                        for (int j = 0; j < Enemies.Count; j++)
                        {
                            var enemy = Enemies[j];
                            if (enemy.Hitbox.Intersects(projectile.HitBox))
                            {
                                Debug.WriteLine("Enemy Hit!");
                                HitSomething = true;
                                enemy.EnemyIsHit(ref _player, _player.Position, projectile.HitDamage);
                                if (enemy.Health <= 0)
                                {
                                    Enemies.RemoveAt(j--);
                                    continue;
                                }
                            }
                        }
                    }
                    else if (projectile.TargetType == TargetType.Player)
                    {
                        if (projectile.HitBox.Intersects(_player.HitBox))
                        {
                            // Player hit logic
                            HitSomething = true;
                        }
                    }
                    if (HitSomething)
                    {
                        _squareParticles.AddRange(projectile.HitGround(rnd));
                        _projectiles.Remove(projectile);
                        i--;
                        continue;
                    }

                    SquareParticle? particle = projectile.GetParticles(rnd);
                    if (particle == null) { continue; }
                    _squareParticles.Add((SquareParticle)particle);
                }

                playerTileIndex = new Point(
                    (int)(_player.Position.X / MapCellSize),
                    (int)(_player.Position.Z / MapCellSize)
                    );

                int terrainHeightAtPlayerPosition;

                /* Find tile at player position */
                int playerXIndex = (int)_player.Center.X / MapCellSize;
                int playerYIndex = (int)_player.Center.Z / MapCellSize;


                /* Bounds check */
                if (playerXIndex >= 0 && playerYIndex >= 0 && playerXIndex < _seedMapper.width && playerYIndex < _seedMapper.height)
                {
                    terrainHeightAtPlayerPosition = _seedMapper.Heights[playerXIndex, playerYIndex] - Player.sizeY;
                }
                else
                {
                    terrainHeightAtPlayerPosition = int.MaxValue;
                }

                /* If player is below terrain, move them to terrain height */
                if (terrainHeightAtPlayerPosition < _player.Position.Y)
                {
                    Vector3 normal = _seedMapper.GetNormal(playerXIndex, playerYIndex);
                    if (normal.Y < 0)
                    {
                        normal *= -1;
                    }
                    _player.HitGround(_masterInput.KeyboardState, normal);
                    _player.SetPosition(
                    new Vector3(
                        _player.Position.X,
                        terrainHeightAtPlayerPosition,
                        _player.Position.Z
                    ));
                    //_squareParticles.Add(new SquareParticle(
                    //    new Vector3(playerXIndex * MapCellSize, terrainHeightAtPlayerPosition + Player.sizeY, playerYIndex * MapCellSize),
                    //    Color.SandyBrown,
                    //    -normal * 50,
                    //    rnd
                    //    ));
                }
                if (terrainHeightAtPlayerPosition - 20 <= _player.Position.Y && _masterInput.IsPressed(KeybindActions.Jump))
                {
                    _player.Jump();
                }
                bool AddedElement = false;
                int currentElements = _spellbook.ElementsCount;
                if (_masterInput.OnPress(KeybindActions.AddElementFire))
                {
                    _spellbook.AddElement(Element.Fire);
                    AddedElement = true;
                }
                if (_masterInput.OnPress(KeybindActions.AddElementAir))
                {
                    _spellbook.AddElement(Element.Air);
                    AddedElement = true;
                }
                if (_masterInput.OnPress(KeybindActions.AddElementEarth))
                {
                    _spellbook.AddElement(Element.Earth);
                    AddedElement = true;
                }
                if (_masterInput.OnPress(KeybindActions.AddElementWater))
                {
                    _spellbook.AddElement(Element.Water);
                    AddedElement = true;
                }
                if (_masterInput.IsPressed(KeybindActions.CastSpell) || currentElements == 3 && AddedElement)
                {
                    _spellbook.TryCast(_projectiles, ref _player, ref _squareParticles);
                    _spellbook.ClearElements();
                }
            }
            else if (Paused)
            {
                if (_masterInput.OnPress(KeybindActions.BackButton))
                {
                    _currentGameState = gamestates.Playing;
                }
                PauseMenu.Options input = (PauseMenu.Options)_pauseMenu.GetBehaviorValueOnClick(_previousMouseState, _mouseState);

                switch (input)
                {
                    case PauseMenu.Options.Resume:
                        _currentGameState = gamestates.Playing;
                        break;
                    case PauseMenu.Options.Settings:
                        _currentGameState = gamestates.SettingsMenu;
                        break;
                    case PauseMenu.Options.Quit:
                        Exit();
                        break;
                }

            }
            else if (SettingsMenu)
            {
                if (_masterInput.OnPress(KeybindActions.BackButton))
                {
                    _currentGameState = gamestates.Playing;
                }
                _settingsMenu.Update(_masterInput, _mouseState, _previousMouseState);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            worldDraw = new WorldDraw(
                GraphicsDevice,
                doubleSided: true,
                texture: null,
                world: null,
                //world: Matrix.CreateRotationX(_player.Angle.X) * Matrix.CreateRotationY(_player.Angle.Y),
                //world: Matrix.CreateRotationY(_player.Angle.Y) * Matrix.CreateRotationX(_player.Angle.X),
                view: Matrix.CreateLookAt(
                    _player.EyePos,  // camera position (looking toward origin)
                    _player.EyePos + _player.dirVector,          // target
                    Vector3.Down             // up vector
                ),
                projection: Matrix.CreatePerspectiveFieldOfView(
                    _FOV, // FOV
                    GraphicsDevice.Viewport.AspectRatio, // Aspect Ratio
                    0.1f, // Near Plane
                    5000f // Far Plane
                    )
                );


            //worldDraw.SetWorld(Matrix.CreateRotationY(_player.Angle.Y) * Matrix.CreateRotationX(_player.Angle.X));
            //worldDraw.SetView(_player.PlayerCamera.ViewMatrix);
            //worldDraw.SetProjection(_player.PlayerCamera.ProjectionMatrix);
            //GraphicsDevice.SetRenderTarget(null);

            DateTime startTime = DateTime.Now;

            List<Shape> visibleShapes = new();
            BoundingFrustum viewFrustrum = _player.PlayerCamera.Frustum;

            int renderDistance = 200;
            int startIndexX = Math.Max(0, playerTileIndex.X - renderDistance);
            int startIndexY = Math.Max(0, playerTileIndex.Y - renderDistance);
            int endIndexX = Math.Min(_seedMapper.width - 1, playerTileIndex.X + renderDistance);
            int endIndexY = Math.Min(_seedMapper.height - 1, playerTileIndex.Y + renderDistance);
            int startIndex = startIndexY * _seedMapper.width + startIndexX;
            int endIndex = endIndexY * _seedMapper.width + endIndexX;
            List<int> meshIndeces = new();
            //Mesh TargetMesh;
            VertexPositionColorNormal[] vertexPositionColorNormals = new VertexPositionColorNormal[MapVertexPositionColorNormal.Length];

            for (int i = 0; i < MapVertexPositionColorNormal.Length; i++)
            {
                vertexPositionColorNormals[i] = MapVertexPositionColorNormal[i];
            }
            for (int y = startIndexY; y < endIndexY; y++)
            {
                for (int x = startIndexX; x < endIndexX; x++)
                {
                    int index = y * _seedMapper.width + x;
                    vertexPositionColorNormals[index].Color = MapVertexPositionColorNormal[index].Color * Math.Clamp(1000f/(float)Vector3.Distance(
                        _player.EyePos,
                        MapVertexPositionColorNormal[index].Position
                        ), 0, 1);
                }
            }

            for (int y = startIndexY; y < endIndexY; y++)
            {
                for (int x = startIndexX; x < endIndexX; x++)
                {
                    int index = y * _seedMapper.width + x;
                    Vector3 p1 = MapVertexPositionColorNormal[index].Position;
                    Vector3 p2 = MapVertexPositionColorNormal[index + 1].Position;
                    Vector3 p3 = MapVertexPositionColorNormal[index + 1 + _seedMapper.width].Position;
                    Vector3 p4 = MapVertexPositionColorNormal[index + _seedMapper.width].Position;
                    BoundingBox box = new BoundingBox(p1, p3);
                    //if (!(Triangle.WorldPosToScreenPos(_player.EyePos, _player._angle.Y, _player._angle.X, p1, out Point screenPos) && screenRect.Contains(screenPos))) 
                    if (viewFrustrum.Contains(box) == ContainmentType.Disjoint
                        //viewFrustrum.Contains(p2) == ContainmentType.Disjoint &&
                        //viewFrustrum.Contains(p3) == ContainmentType.Disjoint &&
                        //viewFrustrum.Contains(p4) == ContainmentType.Disjoint
                        )
                    {
                        continue;
                    }

                    meshIndeces.Add(index);
                    meshIndeces.Add(index + 1);
                    meshIndeces.Add(index + 1 + _seedMapper.width);
                    meshIndeces.Add(index + 1 + _seedMapper.width);
                    meshIndeces.Add(index + _seedMapper.width);
                    meshIndeces.Add(index);
                    //index,
                    //index + 1,
                    //index + 1 + _seedMapper.width,
                    //index + _seedMapper.width


                }
            }
            if (meshIndeces.Count > 0)
                worldDraw.DrawMesh(
                    GraphicsDevice,
                    meshIndeces.ToArray(),
                    vertexPositionColorNormals,
                    0,
                    vertexPositionColorNormals.Count()
                    );
            //TargetMesh = new Mesh(
            //    meshIndeces.ToArray(),
            //    new (int, int, int)[] { },
            //    heightMap,
            //    Vector3.Zero,
            //    Vector2.Zero
            //    );
            //TargetMesh.Draw(
            //    ref _screenBuffer,
            //    meshColors.ToArray(),
            //    0.2f,
            //    _player.EyePos,
            //    _player.Angle.Y,
            //    _player.Angle.X,
            //    lightSource,
            //    Color.LightGoldenrodYellow
            //    );

            List<GenericModel> allModels = new List<GenericModel>(_models);

            for (int i = 0; i < _projectiles.Count; i++)
            {
                GenericModel model = _projectiles[i].Model;
                BoundingBox boundingBox = model.BoundingBox;
                bool inView = ContainmentType.Disjoint != viewFrustrum.Contains(boundingBox);
                if (inView)
                {
                    model.Draw(worldDraw, GraphicsDevice);
                }
            }

            foreach (Enemy enemy in Enemies)
            {
                bool inView = viewFrustrum.Contains(enemy.Hitbox) != ContainmentType.Disjoint;
                if (inView)
                {
                    foreach (GenericModel model in enemy.models)
                    {
                        model.Draw(worldDraw, GraphicsDevice);
                    }
                }
                visibleShapes.AddRange(enemy.GetHealthBar(_player));
            }

            foreach (GenericModel model in allModels)
            {
                BoundingBox boundingBox = model.BoundingBox;
                bool inView = ContainmentType.Disjoint != viewFrustrum.Contains(boundingBox);
                if (inView)
                {
                    model.Draw(worldDraw, GraphicsDevice);
                }
            }

            _crystalBall.SetPosition(_player);
            _crystalBall.Draw(_spellbook, worldDraw, GraphicsDevice);

            /* Moves Colors on the CrystalBall */
            _crystalBall.UpdateHighlights(_spellbook, _player);

            /* Draw Shapes */
            for (int i = 0; i < visibleShapes.Count; i++)
            {
                Shape shape = visibleShapes[i];

                Vector3 shapePos = shape.Position;

                Color color = shape.ApplyShading(shapePos - _player.EyePos, shape.Color, Color.LightGoldenrodYellow);

                if (shape is Square)
                {
                    var square = (Square)shape;
                    worldDraw.DrawQuad(GraphicsDevice, new Vector3[] { square.P1, square.P2, square.P3, square.P4 }, color);
                }
                else
                {

                    var triangle = (Triangle)shape;
                    worldDraw.DrawTriangle(GraphicsDevice, new Vector3[] { triangle.P1, triangle.P2, triangle.P3 }, color);
                }
            }

            /* Draw Particles */
            foreach (var particle in _squareParticles)
            {
                worldDraw.DrawTriangle(GraphicsDevice, new Vector3[] { particle.P1, particle.P2, particle.P3 }, particle.Color);
            }
            /*  Draw Particles using Mesh (Slower)
            VertexPositionColor[] particleVertices = new VertexPositionColor[_squareParticles.Count() * 3];
            int[] particleIndeces = new int[_squareParticles.Count() * 3];
            for (int i = 0; i < _squareParticles.Count(); i += 3)
            {
                SquareParticle Particle = _squareParticles[i / 3];
                particleVertices[i] = new VertexPositionColor(Particle.P1, Particle.Color * ((float)rnd.NextDouble()));
                particleVertices[i + 1] = new VertexPositionColor(Particle.P2, Particle.Color * ((float)rnd.NextDouble()));
                particleVertices[i + 2] = new VertexPositionColor(Particle.P3, Particle.Color * ((float)rnd.NextDouble()));
            }
            for (int i = 0; i < particleIndeces.Length; i++)
            {
                particleIndeces[i] = i;
            }
            worldDraw.DrawMesh(
                GraphicsDevice,
                particleIndeces,
                particleVertices,
                0,
                particleVertices.Count()
                );
            */

            /* FPS Counter */
            if (_framesPerSecondTimer.Update())
            {
                /* Runs Every second */
                Debug.WriteLine("DrawTime: " + (DateTime.Now - startTime).Milliseconds);
                Debug.WriteLine("FPS: " + _framesPerSecondTimer.FPS);
            }

            if (Paused)
            {
                _pauseMenu.Draw(_spriteBatch, _gradiantSquare, _spriteFont, _drawParemeters, _mouseState, Color.Red, Color.White);
            }
            else if (SettingsMenu)
            {
                _settingsMenu.Draw(_spriteBatch, _gradiantSquare, _spriteFont, _drawParemeters, _mouseState, Color.Red, Color.White);
            }

            base.Draw(gameTime);
        }
    }
}
