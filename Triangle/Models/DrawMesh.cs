using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Models;

namespace SlimeGame.Models
{
    internal class DrawMesh
    {
        public short VerticesPerShape;
        public short[] Indices;
        public VertexPositionColor[] Vertices;

        public int IndexCount => Indices.Length;
        public int VertexCount => Vertices.Length;
        public int ShapeCount => Indices.Length / VerticesPerShape;

        public DrawMesh(short[] indices, VertexPositionColor[] vertices, short verticesPerShape)
        {
            Indices = indices;
            Vertices = vertices;
            VerticesPerShape = verticesPerShape;
        }
        public DrawMesh(short[] indices, Vector3[] vertices, Color[] colors, short verticesPerShape)
        {
            Indices = indices;
            Vertices = new VertexPositionColor[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(vertices[i], colors[i]);
            }
            VerticesPerShape = verticesPerShape;
        }
    }
}
