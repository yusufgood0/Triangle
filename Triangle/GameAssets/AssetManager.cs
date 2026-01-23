using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.Drawing;
using SlimeGame.Drawing.Models;
using SlimeGame.GameAsset.Projectiles;
using SlimeGame.GameAssets.Enemies;
using SlimeGame.Generation;
using SlimeGame.Input;

namespace SlimeGame.GameAsset
{
    internal class AssetManager
    {
        public Player Player => _player;
        public ChunkManager ChunkManager => _chunkManager;
        public List<Projectile> Projectiles => _projectiles;
        public List<Particle> Particles => _particles;
        public List<Enemy> Enemies => _enemies;
        public EnemySpawner EnemySpawner => _enemySpawner;
        public List<GenericModel> OneTimeDrawModels => _oneTimeDrawModels;

        private Player _player;
        private ChunkManager _chunkManager;
        private List<Projectile> _projectiles = new();
        private List<Particle> _particles = new();
        private List<Enemy> _enemies = new();
        private EnemySpawner _enemySpawner = new();
        private List<GenericModel> _oneTimeDrawModels = new();

        public AssetManager(Player player, ChunkManager chunkManager)
        {
            _player = player;
            _chunkManager = chunkManager;
        }
        public void PlayUpdate(MasterInput input)
        {
            EnemySpawner.Update(this);
        }
        public void PlaySound(SoundEffectInstance soundEffectInstance, Vector3 Pos = default, Vector3 Velocity = default)
        {
            AudioListener listener = _player.AudioListener;
            AudioEmitter emitter = new AudioEmitter()
            {
                Position = Pos,
                Up = listener.Up,
                Velocity = Velocity,
                DopplerScale = 1f
            };
            if (Pos != default)
            {
                Debug.WriteLine("applying 3d");
                soundEffectInstance.Apply3D(listener, emitter);
            }
            Debug.WriteLine("playing sound effect");
            soundEffectInstance.Stop();
            soundEffectInstance.Play();
        }
        public void DrawAllTemporaryObjects(WorldDraw _worldDraw, BoundingFrustum viewFrustrum, GraphicsDevice GraphicsDevice)
        {
            foreach (var model in _oneTimeDrawModels)
            {
                BoundingBox boundingBox = model.BoundingBox;
                bool inView = ContainmentType.Disjoint != viewFrustrum.Contains(boundingBox);
                if (inView)
                {
                    model.Draw(_worldDraw, GraphicsDevice);
                }
            }
            _oneTimeDrawModels.Clear();
        }
        public void Add(Projectile projectile) => _projectiles.Add(projectile);
        public void Add(Particle particle) => _particles.Add(particle);
        public void Add(Enemy enemy) => _enemies.Add(enemy);
        public void Add(GenericModel model) => _oneTimeDrawModels.Add(model);

    }
}
