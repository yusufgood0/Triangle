using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame
{
    enum ActionCatagory
    {
        AddElement = 0,
        CastSpell = 1,
    }
    internal class Action(Keys Key, ActionCatagory ActionType, int Value)
    {
        public Keys Key = Key;
        public ActionCatagory ActionType = ActionType;
        public int Value = Value;
    }
}
