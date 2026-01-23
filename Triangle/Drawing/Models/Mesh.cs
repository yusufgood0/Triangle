using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SlimeGame.Drawing.Models
{
    internal struct Mesh : GenericModel
    {
        BoundingBox GenericModel.BoundingBox => BoundingBox;
        VertexPositionColorNormal[] GenericModel.Vertices => Vertices;
        int[] GenericModel.Indeces => Indices;
        Color GenericModel.Color
        {
            get => Vertices[0].Color;
            set => throw new NotImplementedException("Cannot set mesh color");
        }
        Vector3 GenericModel.Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public Mesh(VertexPositionColorNormal[] Vertices, int[] Indices, BoundingBox BoundingBox)
        {
            this.Vertices = Vertices;
            this.Indices = Indices;
            this.BoundingBox = BoundingBox;
        }
        public VertexPositionColorNormal[] Vertices;
        public int[] Indices;
        public BoundingBox BoundingBox;
        public Vector3 Dimensions => BoundingBox.Max - BoundingBox.Min;
        public static List<Mesh> MoveTo(Vector3 position, Mesh[] meshArray)
        {
            List<Mesh> movedMeshes = new();
            foreach (var mesh in meshArray)
            {
                Vector3 translation = position - mesh.BoundingBox.Min;
                VertexPositionColorNormal[] movedVertices = new VertexPositionColorNormal[mesh.Vertices.Length];
                for (int i = 0; i < mesh.Vertices.Length; i++)
                {
                    movedVertices[i] = new VertexPositionColorNormal(
                        mesh.Vertices[i].Position + translation,
                        mesh.Vertices[i].Color,
                        mesh.Vertices[i].Normal);
                }
                BoundingBox movedBoundingBox = new BoundingBox(mesh.BoundingBox.Min + translation, mesh.BoundingBox.Max + translation);
                movedMeshes.Add(new Mesh(movedVertices, mesh.Indices, movedBoundingBox));
            }
            return movedMeshes;
        }
        public Mesh MoveTo(Vector3 position)
        {
            Vector3 translation = position - BoundingBox.Min;
            VertexPositionColorNormal[] movedVertices = new VertexPositionColorNormal[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                movedVertices[i] = new VertexPositionColorNormal(
                    Vertices[i].Position + translation,
                    Vertices[i].Color,
                    Vertices[i].Normal);
            }
            BoundingBox movedBoundingBox = new BoundingBox(BoundingBox.Min + translation, BoundingBox.Max + translation);
            return new Mesh(movedVertices, Indices, movedBoundingBox);
        }
        public void Move(Vector3 translation)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position += translation;
            }
            BoundingBox = new BoundingBox(BoundingBox.Min + translation, BoundingBox.Max + translation);
        }

    }
}
