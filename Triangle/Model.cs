using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Triangle
{
    internal interface Model
    {
        Shape[] Shapes { get; }
        BoundingBox BoundingBox { get; }
        public Model Move(Vector3 offset)
        {
            return null;
        }
    }
}
