using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SlimeGame.GameAssets.Enemies;
using SlimeGame.GameAsset.Projectiles;
using SlimeGame.Generation;

namespace SlimeGame.GameAsset.StatusEffects
{
    internal interface StatusEffect
    {
        static List<StatusEffect> _activeEffects = new List<StatusEffect>();
        DateTime ExpireTime { get; }
        public float TimeUntilExpire => (float)(ExpireTime - Game1.PlayingGameTime).TotalSeconds;
        protected virtual bool Update(AssetManager assetManager) => false;
        protected virtual bool EffectEnd(AssetManager assetManager) => false;

        static bool ContainSpell(SpellBook.Spell spell)
        {
            bool foundSpell = _activeEffects.Any(i => (i is SpellCooldown) ? ((SpellCooldown)i).Spell == spell : false);
            //
            // writeline($"Active effects contains {spell} : {foundSpell.ToString()}, {_activeEffects.Count()}");

            //returns true if any active effect is a SpellCooldown with the same spell as the given effect
            return _activeEffects.Any(i => (i is SpellCooldown) ? ((SpellCooldown)i).Spell == spell : false);
        }
        public void AddEffect()
        {
            _activeEffects.Add(this);
        }
        public void CancelEffect()
        {
            if (!_activeEffects.Remove(this))
            {
                //throw new Exception("Effect not found in active effects");
            }
        }
        public static void CancelEffect(StatusEffect effect)
        {
            if (!_activeEffects.Remove(effect))
            {
                throw new Exception("Effect not found in active effects");
            }
        }
        public static void AddEffect(StatusEffect effect)
        {
            _activeEffects.Add(effect);
        }
        public static void UpdateAllEffects(AssetManager assetManager)
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                StatusEffect effect = _activeEffects[i];
                if (effect.TimeUntilExpire < 0)
                {
                    _activeEffects.RemoveAt(i);
                    effect.EffectEnd(assetManager);
                    i--;
                    continue;
                }
                if (effect.Update(assetManager))
                {
					effect.EffectEnd(assetManager);
                    i--;
                    continue;
                }
            }
        }
    }
}
