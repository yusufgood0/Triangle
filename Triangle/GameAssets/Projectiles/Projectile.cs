using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Drawing.Models;
using SlimeGame.Generation;

namespace SlimeGame.GameAsset.Projectiles
{
    internal abstract class Projectile
    {
        public abstract TargetType TargetType { get; }
        public abstract GenericModel Model { get; }
        public abstract BoundingBox HitBox { get; }
        public abstract float HitDamage { get; }
        public abstract Vector3 Velocity { get; set; }
        public abstract Vector3 Position { get; }
        public abstract Color Color { get; }
        public abstract float IFrameDuration { get; }
        public bool Persistant { get => IFrameDuration > 0; }
        public abstract float Volume { get; }

        public abstract bool Move(ChunkManager chunkManager);
        public abstract void AddParticles(List<Particle> Particles, Random rnd);
        public abstract Particle[] GetPixelDeathParticles(Random rnd, AssetManager manager);
    }
    public enum TargetType
    {
        Enemy = 0,
        Player = 1,
    }

}
