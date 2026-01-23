using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SlimeGame.Drawing.Models
{
    internal static class ModelMeshConverter
    {
        public static MeshCollection LoadModel(string AssetName, ContentManager Content)
        {
            Model model = Content.Load<Model>(AssetName);
            return ConvertToMesh(model);
        }
        public static MeshCollection ConvertToMesh(Model model)
        {
            List<Mesh> meshes = new();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    // calculate vertices
                    VertexPositionColorNormalTexture[] ImportVertices = new VertexPositionColorNormalTexture[part.NumVertices];
                    part.VertexBuffer.GetData(ImportVertices);
                    part.VertexBuffer.SetData(ImportVertices);

                    VertexPositionColorNormal[] ConvertedVertices = new VertexPositionColorNormal[part.NumVertices];
                    for (int i = 0; i < ImportVertices.Length; i++)
                    {
                        //ref Vector3 pos = ref ImportVertices[i].Position;
                        //pos.Y *= -1; // invert Y axis
                        //pos *= 3; // inflate size
                        ConvertedVertices[i] = new VertexPositionColorNormal(
                            ImportVertices[i].Position,
                            ImportVertices[i].Color,
                            ImportVertices[i].Normal);
                    }

                    // calculate indeces
                    int[] Indices;
                    if (part.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
                    {
                        short[] temp = new short[part.PrimitiveCount * 3];
                        Indices = new int[part.PrimitiveCount * 3];
                        part.IndexBuffer.GetData(temp);
                        part.IndexBuffer.SetData(temp);
                        Indices = Array.ConvertAll(temp, n => n + 0);
                        //for (int i = 0; i < Indices.Length; i++)
                        //{
                        //    ref var n = ref temp[i];
                        //    Indices[i] = (int)n;
                        //}
                    }
                    else
                    {
                        Indices = new int[part.PrimitiveCount * 3];
                        part.IndexBuffer.GetData(Indices);
                    }
                    // calculate bounding box
                    Vector3 min = new Vector3(float.MaxValue);
                    Vector3 max = new Vector3(float.MinValue);
                    foreach (var vertex in ConvertedVertices)
                    {
                        min = Vector3.Min(min, vertex.Position);
                        max = Vector3.Max(max, vertex.Position);
                    }
                    BoundingBox box = new BoundingBox(min, max);

                    meshes.Add(new Mesh(ConvertedVertices, Indices, box));
                }
            }
            return new MeshCollection(meshes.ToArray());
        }
    }
}
