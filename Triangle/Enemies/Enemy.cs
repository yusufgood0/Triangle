using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triangle.Enemies
{
    internal interface Enemy
    {
        public void Update(in Player player, in Random rnd);
        public void EnemyHitPlayer(ref Player player);
        public void EnemyIsHit(ref Player player);
        public BoundingBox BoundingBox { get; }
        public BoundingBox Hitbox { get; }
        public Model[] models { get; }
    }
}
