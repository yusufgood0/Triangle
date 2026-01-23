using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SlimeGame.Drawing.Models
{
    internal struct MeshCollection
    {
        Mesh[] _meshes;
        BoundingBox _boundingBox;
        public GenericModel[] GenericModels => _meshes.Cast<GenericModel>().ToArray();
        public Mesh[] Meshes => _meshes;
        public BoundingBox BoundingBox => _boundingBox; 
        public Vector3 Dimensions => _boundingBox.Max - _boundingBox.Min; 
        public MeshCollection(Mesh[] meshes)
        {
            _meshes = meshes;
            CalculateBoundingBox();
        }
        public void Move(Vector3 position)
        {
            _meshes = MoveTo(position).ToArray();
            CalculateBoundingBox();
        }
        public MeshCollection GetMovedCopy(Vector3 position)
        {
            MeshCollection movedCollection = new MeshCollection(MoveTo(position).ToArray());
            movedCollection.CalculateBoundingBox();
            return movedCollection;
        }
        private IEnumerable<Mesh> MoveTo(Vector3 position)
        {
            Mesh[] movedMeshes = new Mesh[_meshes.Length];
            foreach (var mesh in _meshes)
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
                yield return new Mesh(movedVertices, mesh.Indices, movedBoundingBox);
            }
        }
        private void CalculateBoundingBox()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            foreach (var mesh in _meshes)
            {
                min = Vector3.Min(min, mesh.BoundingBox.Min);
                max = Vector3.Max(max, mesh.BoundingBox.Max);
            }
            _boundingBox = new BoundingBox(min, max);
        }
    }
}
