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
    internal struct Sphere : GenericModel
    {
        BoundingBox GenericModel.BoundingBox => new(Center - new Vector3(Radius), Center + new Vector3(Radius));
        //Shape[] GenericModel.Shapes => GetTriangles;
        Color GenericModel.Color { get => Color; set => Color = value; }
        Vector3 GenericModel.Position { get => Center; set => Center = value; }
        VertexPositionColorNormal[] GenericModel.Vertices
        {
            get
            {
                VertexPositionColorNormal[] verts = new VertexPositionColorNormal[Vertices.Count];
                for (int i = 0; i < Vertices.Count; i++)
                {
                    verts[i] = new VertexPositionColorNormal(
                        Vertices[i],
                        Color,
                        Vector3.Normalize(Vertices[i] - Center));
                }
                return verts;
            }
        }
        int[] GenericModel.Indeces => TriangleIndeces.ToArray();

        Color Color;
        List<int> TriangleIndeces;
        List<Vector3> Vertices;
        Vector3 Center;
        float Radius;
        Vector2 _rotation;
        public Sphere(Sphere Sphere)
        {
            TriangleIndeces = new(Sphere.TriangleIndeces);
            Vertices = new(Sphere.Vertices);
            Center = Sphere.Center;
            Radius = Sphere.Radius;
            Color = Sphere.Color;
        }
        public GenericModel Move(Vector3 offset)
        {
            Sphere sphere = new(this);
            sphere.Center += offset;
            for (int i = 0; i < Vertices.Count; i++)
            {
                sphere.Vertices[i] += offset;
            }
            return sphere;
        }
        public Sphere(Vector3 center, float radius, int quality, Color color)
        {
            Color = color;

            Radius = radius;
            Center = center;
            float pitch = MathF.PI / 2;
            float yaw = 0;

            int rows = quality;
            int columns = quality * 2;

            float pitchStep = MathF.PI / rows;
            float yawStep = MathF.Tau / columns;

            // Create the vertices
            Vertices = new List<Vector3>();
            Vector3 up = new Vector3(0, radius, 0);

            // Add top vertex (north pole)
            Vertices.Add(center + up);

            // Add middle vertices
            for (int pitchEdit = 1; pitchEdit < rows; pitchEdit++)
            {
                pitch += pitchStep;
                for (int yawEdit = 0; yawEdit < columns; yawEdit++)
                {
                    yaw += yawStep;
                    Vertices.Add(center + new Vector3(
                        radius * MathF.Cos(pitch) * MathF.Cos(yaw),
                        radius * MathF.Sin(pitch),
                        radius * MathF.Cos(pitch) * MathF.Sin(yaw)
                    ));
                }
            }

            // Add bottom vertex (south pole)
            Vertices.Add(center - up);
            int bottomVertexIndex = Vertices.Count - 1;

            // Create triangles List
            TriangleIndeces = new List<int>();

            // Top cap triangles (connect to north pole)
            for (int i = 0; i < columns; i++)
            {
                int next = (i + 1) % columns;
                TriangleIndeces.Add(0);
                TriangleIndeces.Add(i + 1);
                TriangleIndeces.Add(next + 1);
            }

            // Middle rows triangles
            for (int row = 0; row < rows - 2; row++)
            {
                int rowStart = row * columns + 1;
                int nextRowStart = (row + 1) * columns + 1;

                for (int col = 0; col < columns; col++)
                {
                    int nextCol = (col + 1) % columns;

                    // First triangle of quad
                    TriangleIndeces.Add(rowStart + nextCol);
                    TriangleIndeces.Add(rowStart + col);
                    TriangleIndeces.Add(nextRowStart + col);

                    // Second triangle of quad
                    TriangleIndeces.Add(nextRowStart + nextCol);
                    TriangleIndeces.Add(rowStart + nextCol);
                    TriangleIndeces.Add(nextRowStart + col);
                }
            }

            // Bottom cap triangles (connect to south pole)
            int lastRowStart = (rows - 2) * columns + 1;
            for (int i = 0; i < columns; i++)
            {
                int next = (i + 1) % columns;
                TriangleIndeces.Add(bottomVertexIndex);
                TriangleIndeces.Add(lastRowStart + next);
                TriangleIndeces.Add(lastRowStart + i);
            }
        }
        public Vector3 Rotation(Vector3 vector, Vector3 pivotPoint, Vector2 rotation)
            => General.RotateVector(vector - pivotPoint, rotation.X, rotation.Y) + pivotPoint;
        public void SetRotation(Vector3 pivot, Vector2 rotation)
        {
            foreach (var vertexIndex in Enumerable.Range(0, Vertices.Count))
            {
                Vertices[vertexIndex] = Rotation(Vertices[vertexIndex], pivot, rotation - _rotation);
            }
            Center = Rotation(Center, pivot, rotation - _rotation);
            _rotation = rotation;
        }
        public void ChangeRotation(Vector3 pivot, Vector2 rotation)
        {
            foreach (var vertexIndex in Enumerable.Range(0, Vertices.Count))
            {
                Vertices[vertexIndex] = Rotation(Vertices[vertexIndex], pivot, rotation);
            }
            Center = Rotation(Center, pivot, rotation);
            _rotation += rotation;
        }
        public Shape[] GetTriangles
        {
            get => Triangle.ModelConstructor([.. TriangleIndeces], [.. Vertices], Color);
        }
        public void drawVerticies(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Point screenSize,
            float FOV,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            int width,
            int height,
            Rectangle? sourceRect,
            Color color)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                // debug.writeline($"Drawing vertex {i}/{Vertices.Count}");

                General.DrawObject(
                    spriteBatch,
                    texture,
                    screenSize,
                    FOV,
                    cameraPosition,
                    pitch,
                    yaw,
                    width,
                    height,
                    sourceRect,
                    color,
                    Vertices[i]);

            }
        }
    }
}
