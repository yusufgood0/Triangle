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
    internal struct Sphere
    {
        List<(int, int, int)> Triangles;
        List<Vector3> Vertices;
        public Sphere(Vector3 center, float radius, int quality)
        {
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
            Triangles = new List<(int, int, int)>();

            // Top cap triangles (connect to north pole)
            for (int i = 0; i < columns; i++)
            {
                int next = (i + 1) % columns;
                Triangles.Add((0, i + 1, next + 1));
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
                    Triangles.Add((
                        rowStart + nextCol,
                        rowStart + col,
                        nextRowStart + col
                    ));

                    // Second triangle of quad
                    Triangles.Add((
                        nextRowStart + nextCol,
                        rowStart + nextCol,
                        nextRowStart + col
                    ));
                }
            }

            // Bottom cap triangles (connect to south pole)
            int lastRowStart = (rows - 2) * columns + 1;
            for (int i = 0; i < columns; i++)
            {
                int next = (i + 1) % columns;
                Triangles.Add((bottomVertexIndex, lastRowStart + next, lastRowStart + i));
            }
        }
        public Triangle[] GetTriangles
        {
            get => Triangle.ModelConstructor(Triangles.ToArray(), Vertices.ToArray());
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
                    Debug.WriteLine($"Drawing vertex {i}/{Vertices.Count}");

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
