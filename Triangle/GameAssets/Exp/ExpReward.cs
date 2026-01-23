using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimeGame.GameAsset;
using static SlimeGame.GameAsset.SpellBook;

namespace SlimeGame.GameAsset.Exp
{
    internal abstract class ExpReward
    {
        public abstract string Description { get; }
        public abstract string Name { get; }
        public abstract void Activate(Player Player);
    }
    class SpellReward(Spell spell) : ExpReward
    {
        public Spell Spell => spell;
        public int Complexity
            => GetComplexity(spell);
        protected static int GetComplexity(Spell spell)
        {
            int complexity = 0;
            int[] identity = spell.SpellIdentity.ToString().Select(t => int.Parse(t.ToString())).ToArray(); ;
            foreach (int identityInt in identity)
            {
                if (identityInt != 0)
                    complexity++;
            }
            return complexity;
        }
        public override string Name => spell.GetType().Name;
        public override string Description => spell.Description;
        public override void Activate(Player Player)
        {
            spell.Active = true;
        }

    }
    class HealthReward() : ExpReward
    {
        public override string Name => "Heart Container";
        public override string Description => "Permanently increase your maximum health by one heart. A silent beat fills your essence, enduring what you could never before. 'A stronger heart endures where skill alone cannot.'";
        public override void Activate(Player Player)
        {
            Player.IncreaseMaxHealth(1f);
        }
    }
}
