using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Menus
{
    internal class PauseMenu
    {
        public Menu Menu;
        public PauseMenu(GraphicsDevice graphicsDevice, SpriteFont font, Rectangle menuParameters)
        {
            Rectangle VirtualRect = new Rectangle(0, 0, 800, 700);
            Point ButtonSize = new Point(200, 100);

            MenuButton[] buttons = new MenuButton[]
            {
                new MenuButton(new Rectangle(new(300, 100), ButtonSize), font, 0, "Resume"),
                new MenuButton(new Rectangle(new(300, 300), ButtonSize), font, 1, "Settings"),
                new MenuButton(new Rectangle(new(300, 500), ButtonSize), font, 2, "Quit"),
            };
            Menu = new Menu(graphicsDevice, menuParameters, VirtualRect, buttons);
        }
    }
}
