using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Models.Shapes;

namespace SlimeGame.Models
{
    internal interface GenericModel
    {
        Shape[] Shapes { get; }
        BoundingBox BoundingBox { get; }
        Color Color { get; set; }
        public GenericModel Move(Vector3 offset)
        {
            return null;
        }
    }
}
