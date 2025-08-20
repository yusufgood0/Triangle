using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Triangle
{
    internal struct Cube
    {
        static (int, int, int)[] Triangles = new (int, int, int)[]
            {
                (0, 1, 2), (1, 3, 2), // Front
                (6, 5, 4), (6, 7, 5), // Back
                (4, 1, 0), (4, 5, 1), // Top
                (2, 3, 6), (3, 7, 6), // Bottom
                (0, 2, 4), (2, 6, 4), // Left
                (5, 3, 1), (5, 7, 3)  // Right
            };
        Vector3[] Vertices;
        public Cube(Vector3 TLF, float xSize, float ySize, float zSize)
        {
            Vector3 TRF = new Vector3(TLF.X + xSize, TLF.Y, TLF.Z);
            Vector3 BLF = new Vector3(TLF.X, TLF.Y + ySize, TLF.Z);
            Vector3 BRF = new Vector3(TLF.X + xSize, TLF.Y + ySize, TLF.Z);
            Vector3 TLB = new Vector3(TLF.X, TLF.Y, TLF.Z + zSize);
            Vector3 TRB = new Vector3(TLF.X + xSize, TLF.Y, TLF.Z + zSize);
            Vector3 BLB = new Vector3(TLF.X, TLF.Y + ySize, TLF.Z + zSize);
            Vector3 BRB = new Vector3(TLF.X + xSize, TLF.Y + ySize, TLF.Z + zSize);
            Vertices = new Vector3[]
            {
                TLF, TRF, BLF, BRF,
                TLB, TRB, BLB, BRB
            };
        }
        public Triangle[] GetTriangles
        {
            get => Triangle.ModelConstructor(Triangles, Vertices);
        }
        public Vector3 Average()
        {
            Vector3 sum = Vector3.Zero;
            foreach (Vector3 vertex in Vertices)
            {
                sum += vertex;
            }
            return (sum / 8);
        }
        public void DrawAsWhole(
            ref TextureBuffer screenBuffer,
            Color color,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            int distance
            )
        {
            foreach (Triangle triangle in GetTriangles)
            {
                // Draw each triangle in the square
                triangle.Draw(
                    ref screenBuffer,
                    color,
                    cameraPosition,
                    pitch,
                    yaw,
                    distance
                );
            }
        }
        public unsafe Square[] getSquares()
        {
            fixed (Triangle* GetTrianglesPtr = GetTriangles)
            {
                // Create an array of squares from the triangles
                return new Square[]
                {
                    new Square(GetTrianglesPtr[0], GetTrianglesPtr[1]),
                    new Square(GetTrianglesPtr[2], GetTrianglesPtr[3]),
                    new Square(GetTrianglesPtr[4], GetTrianglesPtr[5]),
                    new Square(GetTrianglesPtr[6], GetTrianglesPtr[7]),
                    new Square(GetTrianglesPtr[8], GetTrianglesPtr[9]),
                    new Square(GetTrianglesPtr[10], GetTrianglesPtr[11])
                };
            }

        }
    }
}
