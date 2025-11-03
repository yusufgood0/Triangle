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
        public int ElementsCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _maxElements; i++)
                {
                    if (_elements[i] != null)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
        public Color[] ElementColors => new Color[]
        {
            _elements[0] == null ? Color.Black: GetElementColor((Element)_elements[0]),
            _elements[1] == null ? Color.Black: GetElementColor((Element)_elements[1]),
            _elements[2] == null ? Color.Black: GetElementColor((Element)_elements[2]),

        };
        const int _maxElements = 3;
        Element?[] _elements = new Element?[_maxElements];
        static Dictionary<int, Spell> Spells = new Dictionary<int, Spell>()
        {
            { FireBall.SpellIdentity, new FireBall() },
            { Dash.SpellIdentity, new Dash() },
        };
        public Color GetElementColor(Element element)
        {
            return element switch
            {
                Element.Earth => new Color(139, 69, 19),
                Element.Water => new Color(0, 191, 255),
                Element.Fire => new Color(255, 69, 0),
                Element.Air => new Color(211, 211, 211),
                _ => Color.White,
            };
        }
        public void AddElement(Element elementToAdd)
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
        }
        public void TryCast(List<Projectile> projectiles, ref Player player, ref List<SquareParticle> squareParticles, ref int screenShake)
        {
            if (ElementsCount == 3)
            {
                CastSpell(projectiles, ref player, ref squareParticles, ref screenShake);
            }

            for (int j = 0; j < _elements.Length; j++)
            {
                _elements[j] = null;
            }
        }
        private void CastSpell(List<Projectile> projectiles, ref Player player, ref List<SquareParticle> squareParticles, ref int screenShake)
        {
            int spellIdentity = ConvertSpellIndex(
                (Element)_elements[0],
                (Element)_elements[1],
                (Element)_elements[2]
                );
            if (Spells.ContainsKey(spellIdentity))
            {
                Spells[spellIdentity].CastSpell(projectiles, ref player, ref squareParticles);
            }

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
                float speed = MathF.Pow(player.Speed.X * player.Speed.X + player.Speed.Y * player.Speed.Y * player.Speed.Z * player.Speed.Z, 0.4f);
                projectiles.Add(
                new FireBallProjectile(player.EyePos, player.dirVector, 1 + speed, Math.Max(speed/100, 1))
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
            public void CastSpell(List<Projectile> projectiles, ref Player player, ref List<SquareParticle> squareParticles, ref int screenShake)
            {
                Debug.Write("Casting Fire Burst");
            }
        }
    }
}
