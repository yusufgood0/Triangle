using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Input
{
    enum ActionCatagory
    {
        AddElement = 0,
        CastSpell = 1,
    }
    internal struct Action
    {
        public Keys Key { get; set; }
        public ActionCatagory ActionType { get; set; }
        public int Value { get; set; }
        public Action(Keys Key, ActionCatagory ActionType, int Value)
        {
            this.Key = Key;
            this.ActionType = ActionType;
            this.Value = Value;
        }
    }
}
