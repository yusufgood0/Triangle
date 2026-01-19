using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.Enemies;
using SlimeGame.GameAsset;
using SlimeGame.GameAsset.Projectiles;
using SlimeGame.Generation;
using SlimeGame.Input;
using SlimeGame.Models;

namespace SlimeGame
{
    internal class ObjectManager
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

        public ObjectManager(Player player, ChunkManager chunkManager)
        {
            _player = player;
            _chunkManager = chunkManager;
        }
        public void PlayUpdate(MasterInput input)
        {
            EnemySpawner.Update(this);
        }
        public void DrawAllTemporaryObjects(WorldDraw _worldDraw, BoundingFrustum viewFrustrum, GraphicsDevice GraphicsDevice)
        {
            foreach (GenericModel model in _oneTimeDrawModels)
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
