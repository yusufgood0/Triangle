using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Generation;
using SlimeGame.Models;

namespace SlimeGame.GameAsset.Projectiles
{
    internal interface Projectile
    {
        public TargetType TargetType { get; }
        public GenericModel Model { get; }
        public BoundingBox HitBox { get; }
        public float HitDamage { get; }
        public Vector3 Velocity { get; set; }
        public Vector3 Position { get; }
        public Color Color { get; }
        public float IFrameDuration { get; }
        public bool Persistant { get => IFrameDuration >= 0; }
        public bool Move(ChunkManager chunkManager)
        {
            throw new MissingMemberException("Projectile move is unimplemented");
        }
        public void AddParticles(List<Particle> Particles, Random rnd);
        public abstract Particle[] GetPixelDeathParticles(Random rnd, ObjectManager manager);
    }
    public enum TargetType
    {
        Enemy = 0,
        Player = 1,
    }
    
}
