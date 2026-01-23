using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.Drawing.TextureDrawers;
using SlimeGame.GameAsset;

namespace SlimeGame
{
    internal static class TripleElementDisplay
    {
        static Texture2D ElementTexture;
        public static void Init(Texture2D ElementTexture)
        {
            TripleElementDisplay.ElementTexture = ElementTexture;
        }
        public static TextureHolder[] Draw(SpriteBatch spritebatch, (Element a, Element b, Element c) elements, int size, int offset, int Yoffset, int spacing, bool vertical = false)
            => Draw(spritebatch, new Element[] { elements.a, elements.b, elements.c}, size, offset, Yoffset, spacing, vertical);
        public static TextureHolder[] Draw(SpriteBatch spritebatch, Element[] elements, int size, int Xoffset, int Yoffset, int spacing, bool vertical = false)
        {
            if (ElementTexture == null) throw new Exception("Must init class");
            if (elements == null) return new TextureHolder[0];
            TextureHolder[] holder = new TextureHolder[elements.Length];
            {
                for (int i = 0; i < elements.Length; i++)
                {

                    holder[i] = new TextureHolder(
                        ElementTexture,
                        TextureHolder.EndEffect.SuppressUpdate,
                        new Rectangle(Xoffset, Yoffset, size, size),
                        SpriteSheet: 0,
                        defaultFrame: 0
                        );
                    if (vertical)
                    {
                        Yoffset += spacing + size;
                    }
                    else
                    {
                        Xoffset += spacing + size;
                    } 
                }
            }
            foreach (var element in holder)
            {
                element.SetColumn(-1);
            }
            for (int i = 0; i < elements.Length; i++)
            {
                int column;
                switch (elements[i])
                {
                    case Element.Fire:
                        column = 0;
                        break;
                    //case Element.Water:
                    //    column = 1;
                    //    break;
                    case Element.Earth:
                        column = 5;
                        break;
                    case Element.Air:
                        column = 3;
                        break;
                    default:
                        column = -1;
                        break;
                }
                holder[i].SetColumn(column);
            }

            foreach (TextureHolder element in holder)
            {
                element.Draw(spritebatch);
            }
            return holder;
        }
    }
}
