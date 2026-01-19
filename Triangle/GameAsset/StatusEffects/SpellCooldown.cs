using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Enemies;
using SlimeGame.GameAsset.Projectiles;
using SlimeGame.Generation;
using SlimeGame.Models;
using static SlimeGame.GameAsset.SpellBook;

namespace SlimeGame.GameAsset.StatusEffects
{
    internal class SpellCooldown : StatusEffect
    {
        static Random rnd = new Random();
        public DateTime ExpireTime { get; }
        public Spell Spell;
        public float RemainingSeconds => (float)(ExpireTime - Game1.PlayingGameTime).TotalSeconds;
        public SpellCooldown(Spell Spell)
        {
            ExpireTime = Game1.PlayingGameTime.AddSeconds(Spell.CoolDown);
            this.Spell = Spell;
        }
    }
}
