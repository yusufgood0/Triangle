using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Triangle
{
    public enum Element
    {
        Earth = 1000,
        Water = 100,
        Fire = 10,
        Air = 1,
    }
    internal class SpellBook()
    {
        const int _maxElements = 3;
        Element?[] _elements = new Element?[_maxElements];
        static Dictionary<int, Spell> Spells = new Dictionary<int, Spell>()
        {
            { FireBall.SpellIdentity, new FireBall() },
            { Dash.SpellIdentity, new Dash() },
        };
        public void AddElement(Element elementToAdd, List<Projectile> projectiles, ref Player player, ref List<SquareParticle> squareParticles)
        {
            for (int i = 0; i < _maxElements; i++)
            {
                if (_elements[i] == null)
                {
                    _elements[i] = elementToAdd;
                    if (i < (_maxElements - 1))
                    {
                        return;
                    }
                }
            }

            CastSpell(projectiles, ref player, ref squareParticles);

            for (int j = 0; j < _elements.Length; j++)
            {
                _elements[j] = null;
            }
        }
        private void CastSpell(List<Projectile> projectiles, ref Player player, ref List<SquareParticle> squareParticles)
        {
            int spellIdentity = ConvertSpellIndex(
                (Element)_elements[0],
                (Element)_elements[1],
                (Element)_elements[2]
                );
            Debug.WriteLine($"{_elements[0]} {_elements[1]} {_elements[2]}: ");
            if (Spells.ContainsKey(spellIdentity))
            {
                Spells[spellIdentity].CastSpell(projectiles, ref player, ref squareParticles);
            }
            else
            {
                Debug.Write("Misfire");
            }
            Debug.WriteLine("");

        }
        public static int ConvertSpellIndex(Element a, Element b, Element c)
        {
            return ((int)a) + ((int)b) + ((int)c);
        }
        internal interface Spell
        {
            int SpellIdentity { get; }
            public void CastSpell(List<Projectile> projectiles, ref Player player, ref List<SquareParticle> squareParticles)
            {

            }
        }
        internal class FireBall : Spell
        {
            int Spell.SpellIdentity => SpellIdentity;
            public static int SpellIdentity = ConvertSpellIndex(Element.Fire, Element.Fire, Element.Fire);
            public void CastSpell(List<Projectile> projectiles, ref Player player, ref List<SquareParticle> squareParticles)
            {
                Debug.Write("Casting FireBall");
                projectiles.Add(
                new FireBallProjectile(player.EyePos, player.dirVector)
                );
            }
        }
        internal class Dash : Spell
        {
            int Spell.SpellIdentity => SpellIdentity;
            public static int SpellIdentity = ConvertSpellIndex(Element.Air, Element.Air, Element.Air);
            public void CastSpell(List<Projectile> projectiles, ref Player player, ref List<SquareParticle> squareParticles)
            {
                Debug.Write("Casting Dash");
                player.Dash();
            }
        }
        internal class FireBurst : Spell
        {
            int Spell.SpellIdentity => SpellIdentity;
            public static int SpellIdentity = ConvertSpellIndex(Element.Fire, Element.Fire, Element.Air);
            public void CastSpell(List<Projectile> projectiles, ref Player player, ref List<SquareParticle> squareParticles)
            {
                Debug.Write("Casting Fire Burst");
            }
        }
    }
}
