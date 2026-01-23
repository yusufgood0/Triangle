using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Drawing.Models.Shapes;

namespace SlimeGame.Drawing.Models
{
    internal class MeshHelper
    {
        
        public static (VertexPositionColor[] vertices, int[] indeces) ConvertToSquares(VertexPositionColorNormal[] vertices)
        {
            List<VertexPositionColor> squareVertices = new();
            List<int> indeces = new();
            Vector3 TL = new Vector3(-1, 1, 0) * 10;
            Vector3 TR = new Vector3(1, 1, 0) * 10;
            Vector3 BR = new Vector3(1, -1, 0) * 10;
            Vector3 BL = new Vector3(-1, -1, 0) * 10;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                indeces.Add(squareVertices.Count());
                indeces.Add(squareVertices.Count() + 1);
                indeces.Add(squareVertices.Count() + 2);
                indeces.Add(squareVertices.Count() + 3);
                squareVertices.Add(new VertexPositionColor(vertices[i].Position + TL, Color.White));
                squareVertices.Add(new VertexPositionColor(vertices[i].Position + TR, Color.White));
                squareVertices.Add(new VertexPositionColor(vertices[i].Position + BR, Color.White));
                squareVertices.Add(new VertexPositionColor(vertices[i].Position + BL, Color.White));
            }
            return (squareVertices.ToArray(), indeces.ToArray());
        }
        public static (VertexPositionColorNormal[] Vertices, int[] Indeces) CreateVerticesAndIndeces(
            List<Triangle> triangles)
        {
            List<VertexPositionColorNormal> vertices = new();
            List<int> indeces = new();
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle triangle = triangles[i];
                int baseIndex = vertices.Count;
                vertices.Add(new VertexPositionColorNormal(triangle.P1, triangle.Color, triangle.Normal()));
                vertices.Add(new VertexPositionColorNormal(triangle.P2, triangle.Color, triangle.Normal()));
                vertices.Add(new VertexPositionColorNormal(triangle.P3, triangle.Color, triangle.Normal()));
                indeces.Add(baseIndex);
                indeces.Add(baseIndex + 1);
                indeces.Add(baseIndex + 2);
            }
            return (vertices.ToArray(), indeces.ToArray());
        }
        public static VertexPositionColorNormal[] ApplyShading(Vector3 lightPosition, VertexPositionColorNormal[] vertices, Color lightColor, float strength, Random rnd)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 normalDir = vertices[i].Normal;

                Vector3 lightDirection = vertices[i].Position - lightPosition;
                lightDirection.Normalize();
                // Calculate the difference in rays between the light direction and the normal vector using Vector3.Dot
                float dotProduct = (Vector3.Dot(normalDir, lightDirection) + 1 )/ 2;

                // mix colors based on the difference in rays
                vertices[i].Color = Color.Lerp(vertices[i].Color, lightColor, dotProduct/2);

            }
            return vertices;

        }
        public static int[] CreateMesh(int width, int height, bool endConnectors, bool sideConnectors)
        {
            List<int> indeces = new();
            int VertexCount = width * height;
            if (endConnectors)
            {
                ConnectIndeces(ref indeces, 0, width, VertexCount + 1);
                ConnectIndeces(ref indeces, VertexCount - width, width, VertexCount + 2);
            }
            if (sideConnectors)
            {
                // LEFT connector at index VertexCount + 1
                int leftConnectorIndex = VertexCount;
                for (int row = 0; row < height - 1; row++)
                {
                    int top = row * width;
                    int bottom = (row + 1) * width;

                    indeces.Add(leftConnectorIndex);
                    indeces.Add(top);
                    indeces.Add(bottom);
                }

                // RIGHT connector at index VertexCount + 2
                int rightConnectorIndex = VertexCount + 1;
                int rightCol = width - 1;
                for (int row = 0; row < height - 1; row++)
                {
                    int top = row * width + rightCol;
                    int bottom = (row + 1) * width + rightCol;

                    indeces.Add(rightConnectorIndex);
                    indeces.Add(bottom);
                    indeces.Add(top);
                }
            }
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int topLeft = y * width + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (y + 1) * width + x;
                    int bottomRight = bottomLeft + 1;
                    indeces.Add(topLeft);
                    indeces.Add(bottomLeft);
                    indeces.Add(topRight);
                    indeces.Add(topRight);
                    indeces.Add(bottomLeft);
                    indeces.Add(bottomRight);
                }
            }
            return indeces.ToArray();
            // Example for 2x3 grid with end connectors
            //  12
            // 7348
            //  56
            // Example for 2x3 grid without end connectors
            //  12
            //  34
            //  56

        }
        public static void ConnectIndeces(ref List<int> indeces, int offset, int indexAmount, int VertexCount)
        {
            int newIndex = VertexCount;
            int endIndex = offset + indexAmount;
            for (int i = offset + 1; i < endIndex; i++)
            {
                indeces.Add(newIndex);
                indeces.Add(i - 1);
                indeces.Add(i);
            }
        }
        public static VertexPositionColorNormal[] CreateBoat(Vector3 orgin, float w, float h, int rows, int columns, Color color)
        {
            List<VertexPositionColorNormal> vertices = new();
            float WidthOver2 = (float)w / 2f;
            Vector3 center = new Vector3(w / 2, 0, 0);
            Vector3 closingPoint = orgin + new Vector3(w, 0, 0);
            columns -= 1;
            for (int y = 0; y < columns+1; y++)
            {
                for (int xindex = 0; xindex < rows; xindex++)
                {
                    float rotation = y / (float)columns * MathF.PI;
                    float x = w * ((xindex+1) / (float)(rows+1));
                    float z = -h / (WidthOver2 * WidthOver2) * (x - w) * x;

                    Vector3 pos = General.Rotate(new Vector3(x, 0, -z), 0, rotation);

                    vertices.Add(new VertexPositionColorNormal(orgin + pos, color, Vector3.Normalize(pos - center)));
                }
            }
            vertices.Add(new VertexPositionColorNormal(orgin, color, Vector3.Normalize(Vector3.Zero - center)));
            vertices.Add(new VertexPositionColorNormal(closingPoint, color, Vector3.Normalize(closingPoint - center)));
            return vertices.ToArray();
        }
        public static VertexPositionColorNormal[] RotateMesh(VertexPositionColorNormal[] Vertices, Vector3 pivot, Vector2 rotation)
        {
            var RotatedVertices = new VertexPositionColorNormal[Vertices.Length];

            foreach (var vertexIndex in Enumerable.Range(0, Vertices.Length))
            {
                RotatedVertices[vertexIndex].Position = Rotation(Vertices[vertexIndex].Position, pivot, rotation);
                RotatedVertices[vertexIndex].Normal = Rotation(Vertices[vertexIndex].Normal, pivot, rotation);
                RotatedVertices[vertexIndex].Color = Vertices[vertexIndex].Color;
            }
            return RotatedVertices;
        }
        public static void RotateMesh(ref VertexPositionColorNormal[] Vertices, Vector3 pivot, Vector2 rotation)
        {
            foreach (var vertexIndex in Enumerable.Range(0, Vertices.Length))
            {
                Vertices[vertexIndex].Position = Rotation(Vertices[vertexIndex].Position, pivot, rotation);
            }
        }
        public static Vector3 Rotation(Vector3 vector, Vector3 pivotPoint, Vector2 rotation)
            => General.RotateVector(vector - pivotPoint, rotation.X, rotation.Y) + pivotPoint;
    }
}
