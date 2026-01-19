using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Enemies;
using SlimeGame.GameAsset;
using SlimeGame.GameAsset.IFrames;
using SlimeGame.GameAsset.Projectiles;
using SlimeGame.GameAsset.SpriteSheets;
using SlimeGame.GameAsset.StatusEffects;
using SlimeGame.Generation;
using SlimeGame.Input;
using SlimeGame.Menus;
using SlimeGame.Models;
using SlimeGame.Models.Shapes;
using TextureArray;
using static SlimeGame.GameAsset.SpriteSheets.SpriteSheetManager;
namespace SlimeGame
{
    public class Game1 : Game
    {
        public static DateTime PlayingGameTime = DateTime.Now;

        // Configuration constants
        private const bool _menusFillScreen = false;
        private readonly Color _defaultButtonColor = Color.DarkSlateBlue;
        private const float _FOV = MathHelper.Pi * 0.75f;
        private readonly Point mapSize = new(400, 400);
        private const int MapCellSize = 80;

        // Possibly adjustable gameplay constants
        private const float _sensitivity = 0.01f;

        // One and done tools which are used throughout the game
        private static readonly Random rnd = new Random();
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private Texture2D _gradiantSquare; //for buttons
        private WorldDraw _worldDraw;
        private PauseMenu _pauseMenu;
        private SettingsMenu _settingsMenu;
        private CrystalBall _crystalBall = new CrystalBall(new Vector3(40, 40, 50));
        private readonly MasterInput _masterInput = new MasterInput();
        private readonly SpellBook _spellbook = new SpellBook(rnd);
        private readonly FramesPerSecondTimer _framesPerSecondTimer = new();

        // storing values and perameters to communicate between draw and other methods
        private Point _screenSize;
        private Rectangle _drawParemeters;
        private bool _previousIsMouseVisible = false;
        private gamestates _currentGameState = gamestates.Playing;
        private const int _maxParticleCount = 1000;

        // for terrain drawing
        readonly Color[] _chunkManagerColors = new Color[3];

        // only used for testing
        private List<GenericModel> _models = new List<GenericModel>();
        private Texture2D _chadGabeTexture;
        private readonly bool _debugMode = false;

        // Game objects
        ObjectManager _objectManager;

        // Drawing hearts for health
        SpriteSheetManager _spriteSheetManager;
        private (int X, int Y) Offset = (416, 209);
        private (int X, int Y) PixelSize = (16, 15);
        private (int X, int Y) MaxPixel = (5, 3);

        // Ui Manager
        UIManager _UIManager;

        // Day/Night cycle length in minutes
        float DayLength = 10f;

        // Game state properties
        private bool Playing { get => _currentGameState == gamestates.Playing; set => _currentGameState = value ? gamestates.Playing : _currentGameState; }
        private bool Paused { get => _currentGameState == gamestates.Paused; set => _currentGameState = value ? gamestates.Paused : _currentGameState; }
        private bool SettingsMenu { get => _currentGameState == gamestates.SettingsMenu; set => _currentGameState = value ? gamestates.SettingsMenu : _currentGameState; }
        private bool LevelUpMenu { get => _currentGameState == gamestates.LevelUpMenu; set => _currentGameState = value ? gamestates.LevelUpMenu : _currentGameState; }
        private enum gamestates { Playing, Paused, SettingsMenu, LevelUpMenu }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {

            /* resizes screen */

            _graphics.PreferredBackBufferWidth = _screenSize.X = _graphics.IsFullScreen ? GraphicsDevice.Adapter.CurrentDisplayMode.Width : 800;
            _graphics.PreferredBackBufferHeight = _screenSize.Y = _graphics.IsFullScreen ? GraphicsDevice.Adapter.CurrentDisplayMode.Height : 800;
            _graphics.ApplyChanges();

            /* Initilizes blank texture as a 1x1 white pixel texture 
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
        (VertexPositionColorNormalTexture[] Vertices, int[] Indices, BoundingBox BoundingBox) tempMesh;
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("ExperienceFont");
            _gradiantSquare = Content.Load<Texture2D>("GradiantSquare");
            _chadGabeTexture = Content.Load<Texture2D>("CHAD GABE");
            Texture2D Elements = Content.Load<Texture2D>("Elements");
            Slime.LoadContent(Content);

            Vector2 MapCenter = new Vector2(mapSize.X / 2 * MapCellSize, mapSize.Y / 2 * MapCellSize);
            if (_menusFillScreen)
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

            /* Determine player spawn position */
            var PlayerPos = Vector3.Zero;

            /* Initilize player object */
            _objectManager =
                new ObjectManager(
                new Player(
                    PlayerPos,
                    new Camera(_screenSize.Y / _screenSize.X, PlayerPos, Vector3.Zero, Vector3.Down, 15000, _FOV * 1.1f),
                    rnd,
                    _screenSize,
                    0.05f,
                    0.1f,
                    Content.Load<Texture2D>("PixelScrollNoBG")
                    ), //bloats FOV to ensure no clipping on camera
                new ChunkManager(rnd.Next(Int32.MinValue, Int32.MaxValue), PlayerPos, 6, 3, 300, 15000f)
                );

            /* Initialize Map Vertices */
            _objectManager.ChunkManager._MapVertexPositionColorNormals = _objectManager.ChunkManager.GetVerticesAsync(_chunkManagerColors, _objectManager.Player.EyePos).GetAwaiter().GetResult();

            /* Initilize world draw object */
            _worldDraw = new WorldDraw(
                GraphicsDevice,
                doubleSided: true,
                texture: null
                );

            SpriteSheetManager healthDisplay;
            {
                (int Width, int Height) drawSize = (60, 60);
                int columns = 5;
                int rows = 1;
                int totalSprites = columns * rows;
                var healthSprites = new (int X, int Y)[totalSprites];
                for (int x = 0; x < columns; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        healthSprites[x + y * columns] = (10 + x * (drawSize.Width + 10), 10 + y * (drawSize.Height + 10));
                    }
                }
                healthDisplay = new SpriteSheetManager(
                    Content.Load<Texture2D>("RpgSpriteCollection"),
                    new SpriteSheetManager.FinishBehavior[] { SpriteSheetManager.FinishBehavior.WaitOnFirstFrame },
                    new (int, int)[] { (10, 10), (10 + drawSize.Width, 10) },
                    Offset,
                    PixelSize,
                    MaxPixel,
                    drawSize: drawSize
                    );

            }

            int elementDisplayCount = SpellBook.MaxElements;
            TextureHolder[] elementDisplay = new TextureHolder[elementDisplayCount];
            {
                int spacing = 10;
                (int XSize, int Ysize) = (60, 60);
                (int Xoffset, int Yoffset) = (_screenSize.X - (spacing + XSize) * elementDisplayCount, spacing);
                for (int i = 0; i < elementDisplayCount; i++)
                {
                    elementDisplay[i] = new TextureHolder(
                        Elements,
                        TextureHolder.EndEffect.SuppressUpdate,
                        new Rectangle(Xoffset + (spacing + XSize) * i, Yoffset, XSize, Ysize),
                        SpriteSheet: 0,
                        defaultFrame: 0
                        );
                }
            }
            Slider expSlider;
            {
                int xpos, ypos;
                float width, height; //percentages
                width = 0.5f;
                height = 0.025f;
                width *= _screenSize.X;
                height *= _screenSize.Y;

                xpos = (int)(_screenSize.X * 0.5f - width / 2);
                ypos = (int)(_screenSize.Y * 0.75f - height / 2);

                expSlider = new Slider(
                    new Rectangle(xpos, ypos, (int)width, (int)height),
                    0f,
                    Color.DarkGray,
                    Color.Green
                    );
            }

            _UIManager = new UIManager(
                elementDisplay,
                healthDisplay,
                expSlider,
                new Slider(
                    new Rectangle(20, _screenSize.Y - 60 - 20, 200, 20),
                    0f,
                    Color.DarkGray,
                    Color.LightGreen
                    ),
                _spriteFont,
                _spriteBatch
                );

            IFrameInstance.AddIFrame(new IFrameInstance(new object(), 3, IFrameType.Universal));
        }
        bool temp = false;
        protected override void Update(GameTime gameTime)
        {
            if (_masterInput.OnPress(Buttons.Back) || _masterInput.OnPress(Keys.Escape) && _masterInput.OnPress(Keys.LeftAlt))
                Exit();

            _masterInput.PreviousMouseState = _masterInput.MouseState;
            _masterInput.MouseState = Mouse.GetState();
            _previousIsMouseVisible = IsMouseVisible;
            _masterInput.UpdateStates();
            IsMouseVisible = !Playing || !IsActive;

            _UIManager.Update(
                _objectManager.Player,
                _spellbook,
                _objectManager.Player.LevelManager,
                0.75f
                );


            if (_masterInput.OnPress(Keys.Enter))
            {
                foreach (var effect in StatusEffect._activeEffects)
                {
                    Debug.WriteLine(effect.GetType().ToString());

                }
            }

            //foreach (var modelIndex in Enumerable.Rang
            //{
            //    _models[modelIndex].ChangeRotation(0.01f, 1f);
            //}

            if (_masterInput.OnPress(KeybindActions.GamePadClick))
            {
                _masterInput.MouseState = new MouseState(
                _masterInput.MouseState.X, _masterInput.MouseState.Y, _masterInput.MouseState.ScrollWheelValue,                // X, Y, ScrollWheelValue
                ButtonState.Pressed,        // LeftButton
                _masterInput.MouseState.MiddleButton,       // MiddleButton
                _masterInput.MouseState.RightButton,       // RightButton
                _masterInput.MouseState.XButton1,       // XButton1
                _masterInput.MouseState.XButton2        // XButton2
            );
            }

            Vector2 controllerOffset = 10 * _masterInput.GamePadState.ThumbSticks.Right;
            controllerOffset.Y *= -1;

            if (controllerOffset != Vector2.Zero)
            {
                Point newMousePos = _masterInput.MouseState.Position + controllerOffset.ToPoint();
                Mouse.SetPosition(newMousePos.X, newMousePos.Y);
            }
            else if (Playing)
            {
                PlayingGameTime = PlayingGameTime.AddSeconds((float)gameTime.ElapsedGameTime.TotalSeconds);
                if (!IsActive)
                {
                    Paused = true;
                }

                if (_masterInput.OnPress(KeybindActions.BackButton))
                {
                    _currentGameState = gamestates.Paused;
                }
                _objectManager.PlayUpdate(_masterInput);
                _objectManager.Player.SafeControlAngleWithMouse(_previousIsMouseVisible, IsMouseVisible, _screenSize, _sensitivity);

                // Update player, and animates heart rotate after full heal
                if (_objectManager.Player.Update(_masterInput, _objectManager.ChunkManager) && _objectManager.Player.Health % 1 == 0)
                {
                    _UIManager.RotateLastHeart((int)_objectManager.Player.Health);
                }

                StatusEffect.UpdateAllEffects(_objectManager);
                IFrameInstance.CullExpiredIframes();

                RenewMapVerticesAsync();
                if (_objectManager.Player.LevelManager.LevelUpMenu)
                {
                    _currentGameState = gamestates.LevelUpMenu;
                }
                foreach (Enemy enemy in _objectManager.Enemies)
                {
                    foreach (Enemy enemy2 in _objectManager.Enemies)
                    {
                        if (!enemy.Equals(enemy2) && enemy.Hitbox.Intersects(enemy2.Hitbox))
                        {
                            enemy.Knockback(enemy2.Position - enemy.Position, 0);
                        }
                    }
                }
                for (int i = 0; i < _objectManager.Particles.Count; i++)
                {
                    var particle = _objectManager.Particles[i];
                    int LifeTime = (int)(Game1.PlayingGameTime - particle.CreationTime).TotalSeconds;
                    if (LifeTime > 1)
                    {
                        _objectManager.Particles.RemoveAt(i);
                        i--;
                        continue;
                    }
                    _objectManager.Particles[i] = _objectManager.Particles[i].Float(20, rnd);
                }
                for (int i = 0; i < _objectManager.Enemies.Count; i++)
                {
                    _objectManager.Enemies[i].Update(_objectManager, rnd);
                    if (Vector3.DistanceSquared(_objectManager.Enemies[i].Position, _objectManager.Player.Position) > 20000 * 20000)
                    {
                        _objectManager.Enemies.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (_objectManager.Enemies[i].Hurtbox.Intersects(_objectManager.Player.HitBox))
                    {
                        // Player is hit by enemy
                        _objectManager.Enemies[i].EnemyHitPlayer(_objectManager.Player);

                    }
                }
                for (int i = 0; i < _objectManager.Projectiles.Count; i++)
                {
                    var projectile = _objectManager.Projectiles[i];
                    if (projectile.Move(_objectManager.ChunkManager))
                    {
                        _objectManager.Particles.AddRange(projectile.GetPixelDeathParticles(rnd, _objectManager));
                        _objectManager.Projectiles.Remove(projectile);
                        i--;
                        continue;
                    }
                    bool HitSomething = false;
                    if (projectile.TargetType == TargetType.Enemy)
                    {
                        for (int j = 0; j < _objectManager.Enemies.Count; j++)
                        {
                            var enemy = _objectManager.Enemies[j];
                            if (enemy.Hitbox.Intersects(projectile.HitBox))
                            {
                                HitSomething = true;
                                if (projectile.Persistant)
                                {
                                    if (enemy.EnemyIsHit(Vector3.Zero, projectile.HitDamage, _objectManager))
                                    {
                                        j--;
                                        continue;
                                    }
                                    enemy.ImmuneFor(projectile.IFrameDuration);
                                }
                                else
                                {
                                    if (enemy.EnemyIsHit(enemy.Position - projectile.Position, projectile.HitDamage, _objectManager))
                                    {
                                        j--;
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    else if (projectile.TargetType == TargetType.Player)
                    {
                        if (projectile.HitBox.Intersects(_objectManager.Player.HitBox))
                        {
                            // Player hit logic
                            HitSomething = true;
                        }
                    }
                    if (!projectile.Persistant && HitSomething)
                    {
                        _objectManager.Particles.AddRange(projectile.GetPixelDeathParticles(rnd, _objectManager));
                        _objectManager.Projectiles.Remove(projectile);
                        i--;
                        continue;
                    }

                    projectile.AddParticles(_objectManager.Particles, rnd);
                }
                while (_objectManager.Particles.Count > _maxParticleCount)
                {
                    _objectManager.Particles.RemoveAt(rnd.Next(0, _objectManager.Particles.Count()));
                }


                bool AddedElement = false;
                int currentElements = _spellbook.ElementsCount;

                if (_masterInput.OnLeftPress)
                {
                    //_objectManager.ChunkManager.OffsetHeightAtPosition(_objectManager.Player.Position, -10000, ref _objectManager.ChunkManager.VertexPositionColorNormals);
                    _objectManager.Player.LevelManager.CheatAllRewards(_objectManager.Player);
                }

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
                    _spellbook.TryCast(_objectManager);
                    //_spellbook.ClearElements();
                }
            }
            else if (Paused)
            {
                if (_masterInput.OnPress(KeybindActions.BackButton))
                {
                    _currentGameState = gamestates.Playing;
                }

                PauseMenu.Options input = (PauseMenu.Options)_pauseMenu.GetBehaviorValueOnClick(_masterInput.PreviousMouseState, _masterInput.MouseState);

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
                _settingsMenu.Update(_masterInput, _masterInput.MouseState, _masterInput.PreviousMouseState);
            }
            else if (LevelUpMenu)
            {
                _objectManager.Player.LevelManager.UpdateLevelUpMenu(_masterInput, _objectManager.Player);
                if (!_objectManager.Player.LevelManager.LevelUpMenu)
                {
                    _currentGameState = gamestates.Playing;
                }
            }
            if (_masterInput.OnPress(Keys.R))
            {
                LoadContent();
            }

            base.Update(gameTime);
        }
        async void RenewMapVerticesAsync()
        {
            if (_objectManager.ChunkManager.TryTranslateAndGetNewLoadedChunks(_objectManager.Player.EyePos))
            {
                _objectManager.ChunkManager._MapVertexPositionColorNormals = await _objectManager.ChunkManager.GetVerticesAsync(_chunkManagerColors, _objectManager.Player.EyePos);
            }
        }
        protected override void Draw(GameTime gameTime)
        {
            float dayLightTime = (float)(gameTime.TotalGameTime.TotalMinutes / DayLength + 0.5f) % 1f;
            Color dayLightColor = SkyColorGenerator.GetSkyColor(dayLightTime);
            GraphicsDevice.Clear(dayLightColor);
            _worldDraw = new WorldDraw(
                GraphicsDevice,
                daylightTime: dayLightTime,
                doubleSided: true,
                texture: null,
                view: Matrix.CreateLookAt(
                    _objectManager.Player.EyePos,  // camera position (looking toward origin)
                    _objectManager.Player.EyePos + _objectManager.Player.dirVector,          // target
                    Vector3.Down             // up vector
                ),
                projection: Matrix.CreatePerspectiveFieldOfView(
                    _FOV + 0.5f * (Math.Clamp(_objectManager.Player.SpeedEffectMultiplierReverse, 0, 0.3f)), // FOV
                    GraphicsDevice.Viewport.AspectRatio, // Aspect Ratio
                    0.1f, // Near Plane
                    15000f // Far Plane
                    )
                );

            List<GenericModel> testModels = new List<GenericModel>(_models);
            List<Shape> visibleShapes = new();
            BoundingFrustum viewFrustrum = _objectManager.Player.PlayerCamera.Frustum;

            // Get merged heightmap from ChunkManager
            (int totalWidth, int totalHeight) = _objectManager.ChunkManager.MergeLoadedChunksHeightMapDimensions;

            // Draw the terrain
            if (_objectManager.ChunkManager._MapVertexPositionColorNormals != null)
            {
                VertexPositionColorNormal[] vertexPositionColorNormals = new VertexPositionColorNormal[totalWidth * totalHeight];
                Vector2 worldOffset = new Vector2(
                    _objectManager.ChunkManager._loadedChunks[0, 0].XIndex * Chunk.ChunkSize * _objectManager.ChunkManager.TileSize,
                    _objectManager.ChunkManager._loadedChunks[0, 0].YIndex * Chunk.ChunkSize * _objectManager.ChunkManager.TileSize
                );

                for (int y = 0; y < totalHeight; y++)
                {
                    for (int x = 0; x < totalWidth; x++)
                    {
                        int index = y * totalWidth + x;
                        vertexPositionColorNormals[index] = _objectManager.ChunkManager._MapVertexPositionColorNormals[x, y];
                    }
                }

                // Build mesh indices with proper frustum culling
                List<int> meshIndices = new();

                for (int y = 0; y < totalHeight - 1; y++)
                {
                    for (int x = 0; x < totalWidth - 1; x++)
                    {
                        int index = y * totalWidth + x;

                        Vector3 p1 = vertexPositionColorNormals[index].Position;
                        Vector3 p2 = vertexPositionColorNormals[index + 1].Position;
                        Vector3 p3 = vertexPositionColorNormals[index + 1 + totalWidth].Position;
                        Vector3 p4 = vertexPositionColorNormals[index + totalWidth].Position;

                        // Check if any corner of the quad is in the frustum
                        if (viewFrustrum.Contains(p1) == ContainmentType.Disjoint &&
                            viewFrustrum.Contains(p2) == ContainmentType.Disjoint &&
                            viewFrustrum.Contains(p3) == ContainmentType.Disjoint &&
                            viewFrustrum.Contains(p4) == ContainmentType.Disjoint)
                        {
                            continue; // fully outside
                        }

                        // Add two triangles for this quad
                        meshIndices.Add(index);
                        meshIndices.Add(index + 1);
                        meshIndices.Add(index + 1 + totalWidth);

                        meshIndices.Add(index + 1 + totalWidth);
                        meshIndices.Add(index + totalWidth);
                        meshIndices.Add(index);
                    }
                }

                // Draw mesh if any indices exist
                if (meshIndices.Count > 0)
                {
                    _worldDraw.DrawMesh(
                        GraphicsDevice,
                        meshIndices.ToArray(),
                        vertexPositionColorNormals,
                        0,
                        vertexPositionColorNormals.Length
                    );
                }
            }

            for (int i = 0; i < _objectManager.Projectiles.Count; i++)
            {
                GenericModel model = _objectManager.Projectiles[i].Model;
                BoundingBox boundingBox = model.BoundingBox;
                bool inView = ContainmentType.Disjoint != viewFrustrum.Contains(boundingBox);
                if (inView)
                {
                    model.Draw(_worldDraw, GraphicsDevice);
                }
            }

            foreach (Enemy enemy in _objectManager.Enemies)
            {
                bool inView = viewFrustrum.Contains(enemy.Hitbox) != ContainmentType.Disjoint;
                if (inView)
                {
                    foreach (GenericModel model in enemy.models)
                    {
                        model.Draw(_worldDraw, GraphicsDevice);
                    }
                }
                //_worldDraw.DrawPositionMarker(
                //    GraphicsDevice,
                //    enemy.BoundingBox.Min,
                //    enemy.BoundingBox.Max,
                //    Color.Red * 0.5F
                //    );
                visibleShapes.AddRange(enemy.GetHealthBar(_objectManager.Player));
            }

            foreach (GenericModel model in testModels)
            {
                BoundingBox boundingBox = model.BoundingBox;
                bool inView = ContainmentType.Disjoint != viewFrustrum.Contains(boundingBox);
                if (inView)
                {
                    model.Draw(_worldDraw, GraphicsDevice);
                }
            }
            _objectManager.DrawAllTemporaryObjects(_worldDraw, viewFrustrum, GraphicsDevice);

            /* Draw and rotate the Crystal Ball */
            _crystalBall.SetPosition(_objectManager.Player);
            _crystalBall.UpdateHighlights(_spellbook, _objectManager.Player);
            _crystalBall.Draw(_spellbook, _worldDraw, GraphicsDevice);

            /* Draw Shapes */
            for (int i = 0; i < visibleShapes.Count; i++)
            {
                Shape shape = visibleShapes[i];

                Vector3 shapePos = shape.Position;

                Color color = shape.Color;
                //color = shape.ApplyShading(shapePos - _objectManager.Player.EyePos, color, Color.LightGoldenrodYellow);

                if (shape is Square)
                {
                    var square = (Square)shape;
                    _worldDraw.DrawQuad(GraphicsDevice, square.P1, square.P2, square.P3, square.P4, color);
                }
                else
                {

                    var triangle = (Triangle)shape;
                    _worldDraw.DrawTriangle(GraphicsDevice, triangle.P1, triangle.P2, triangle.P3, color);
                }
            }

            /* Draw Particles */
            foreach (var particle in _objectManager.Particles)
            {
                _worldDraw.DrawTriangle(GraphicsDevice, particle.P1, particle.P2, particle.P3, particle.Color);
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
                Debug.WriteLine("FPS: " + _framesPerSecondTimer.FPS);
            }

            _UIManager.Draw(_gradiantSquare);

            if (Paused)
            {
                _pauseMenu.Draw(_spriteBatch, _gradiantSquare, _spriteFont, _drawParemeters, _masterInput.MouseState, Color.Red, Color.White);
            }
            else if (SettingsMenu)
            {
                _settingsMenu.Draw(_spriteBatch, _gradiantSquare, _spriteFont, _drawParemeters, _masterInput.MouseState, Color.Red, Color.White);
            }
            else if (LevelUpMenu)
            {
                _objectManager.Player.LevelManager.DrawLevelUpMenu(_spriteBatch, _spriteFont);
            }
            //GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            base.Draw(gameTime);
        }
    }
}
