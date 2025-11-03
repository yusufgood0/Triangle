using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Triangle
{
    internal class PauseMenu
    {
        public Menu Menu;
        public PauseMenu(SpriteFont font)
        {
            MenuButton[] buttons = new MenuButton[]
            {
                new MenuButton("Resume", new Rectangle(300, 150, 200, 50), font, 0),
                new MenuButton("Settings", new Rectangle(300, 250, 200, 50), font, 1),
                new MenuButton("Quit", new Rectangle(300, 350, 200, 50), font, 2),
            };
        }
    }
}
