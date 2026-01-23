using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Drawing.Models
{
    internal interface GenericModel
    {
        public int[] Indeces { get; }
        public VertexPositionColorNormal[] Vertices { get; }
        public BoundingBox BoundingBox { get; }
        public Color Color { get; set; }
        public Vector3 Position { get; set; }
        public GenericModel Move(Vector3 offset)
        {
            return null;
        }
        public void Draw(WorldDraw draw, GraphicsDevice GraphicsDevice)
        {
            draw.DrawMesh(GraphicsDevice, Indeces, Vertices, 0, Vertices.Length);
        }

        public void SetRotation(Vector3 pivot, float yaw, float pitch)
            => SetRotation(pivot, new Vector2(yaw, pitch));
        public void SetRotation(float yaw, float pitch)
            => SetRotation(Position, new Vector2(yaw, pitch));
        public void SetRotation(Vector2 rotation)
            => SetRotation(Position, rotation);
        public void SetRotation(Vector3 pivot, Vector2 rotation)
        {
            // debug.writeline("GenericModel.SetRotation called - no implementation: " + this.GetType());
        }
        public void ChangeRotation(Vector3 pivot, float yaw, float pitch)
            => ChangeRotation(pivot, new Vector2(yaw, pitch));
        public void ChangeRotation(float yaw, float pitch)
            => ChangeRotation(Position, new Vector2(yaw, pitch));
        public void ChangeRotation(Vector2 rotation)
            => ChangeRotation(Position, rotation);
        public void ChangeRotation(Vector3 pivot, Vector2 rotation)
        {
            // debug.writeline("GenericModel.ChangeRotation called - no implementation: " + this.GetType());
        }
    }
}
