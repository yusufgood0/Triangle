using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SlimeGame.GameAsset;
using SlimeGame.GameAssets.Enemies;

namespace SlimeGame
{
    internal class EnemySpawner
    {
        static Random _rnd = new Random();
        (Type enemyType, int spawnWeight, float batchWeight)[] _variants;
        DateTime _enemySpawnTimer;
        float _multiplier;
        float GetDistanceFromTarget => _rnd.Next(5000, 12000);
        float SpawnInterval => _rnd.Next(15, 35) * (1f / _multiplier);
        bool CanSpawnEnemies => _enemySpawnTimer < Game1.PlayingGameTime;
        public EnemySpawner()
        {
            _variants = new (Type enemy, int spawnWeight, float batchWeight)[]
            {
                (new Slime(Vector3.Zero).GetType(), 5, 0.5f),
                (new Bat(Vector3.Zero).GetType(), 7, 0.25f),
            };
        }
        public EnemySpawner((Type enemyType, int spawnWeight, float batchWeight)[] Variants)
        {
            _variants = Variants;
        }
        public void Update(AssetManager assetManager)
        {
            _multiplier = 0.2f + assetManager.Player.LevelManager.Level/5;
            if (assetManager.Enemies.Count != 0 && !CanSpawnEnemies) return;

            Vector3 center = assetManager.Player.Position;
            Type[] newEnemies = GetNewEnemies();

            foreach (Type enemyType in newEnemies)
            {
                float angle = MathF.Tau * (float)_rnd.NextDouble();
                Vector3 direction = new Vector3(
                    MathF.Cos(angle),
                    0,
                    MathF.Sin(angle)
                    );
                Vector3 spawnPos = center + direction * GetDistanceFromTarget;

                Enemy enemy = (Enemy)Activator.CreateInstance(enemyType, spawnPos);
                assetManager.Add(enemy);
            }

            ResetSpawnTimer();
        }
        public void ResetSpawnTimer()
        {
            _enemySpawnTimer = Game1.PlayingGameTime.AddSeconds(SpawnInterval);
        }
        public Type[] GetNewEnemies()
        {
            int totalWeight = 0;
            int randomValue;
            int currentIndex;
            float totalBatchWeight = 0;
            List<Type> newEnemies = new();

            foreach (var variant in _variants)
            {
                totalWeight += variant.spawnWeight;
            }

            while (totalBatchWeight < 1)
            {
                // reset for each enemy pick
                currentIndex = 0;
                randomValue = _rnd.Next(1, totalWeight);

                // cycles though enemy variants until randomValue is less than or equal to 0
                while (randomValue > 0)
                {
                    randomValue -= _variants[currentIndex++].spawnWeight;
                }

                // now that weve picked an enemy, step back one index to get the correct enemy
                currentIndex--;

                // add enemy to the list and add its batch weight to the total
                newEnemies.Add(_variants[currentIndex].enemyType);
                totalBatchWeight += _variants[currentIndex].batchWeight;
            }

            return newEnemies.ToArray();
        }
    }
}
