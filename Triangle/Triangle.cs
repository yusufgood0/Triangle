using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Triangle
{
    internal struct Triangle : Shape
    {
        Vector3 Shape.Position => P1;
        public Vector3 P1 { get; set; }
        public Vector3 P2 { get; set; }
        public Vector3 P3 { get; set; }
        public Vector3 Average { get => (P1 + P2 + P3) / 3; }
        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.P1 = p1;
            this.P2 = p2;
            this.P3 = p3;
        }

        static Color _transparentColor = new Color(0, 0, 0, 255);
        static readonly Point _errorPoint = new Point(-1, -1);
        static float _fov_scale;
        static Point _screenCenter;
        static Point _CachedscreenSize;
        static Texture2D _texture;
        static Color[] _pixelBuffer;
        public static void Initialize(SpriteBatch spritebatch, TextureBuffer screenBuffer) // call once per frame
        {
            _screenCenter = new Point(screenBuffer.width / 2, screenBuffer.height / 2);
            _CachedscreenSize = new Point(screenBuffer.width, screenBuffer.height);
            _texture = new Texture2D(spritebatch.GraphicsDevice, 1, 1);
            _texture.SetData(new Color[1] { Color.White });
        }
        public static void UpdateConstants(float FOV, Point screenSize) // call once per frame
        {
            _fov_scale = 1f / MathF.Tan(FOV / 2);
        }
        public unsafe void Draw(
            SpriteBatch spriteBatch,
            Color color,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            int distance
            )
        {
            if (
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.P1, out Point p1) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.P2, out Point p2) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.P3, out Point p3)
            ) { return; }

            /* calculates a bounding rectangle for the triangle */
            int xmin = Math.Max(Math.Min(Math.Min(p1.X, p2.X), p3.X), 0);
            int ymin = Math.Max(Math.Min(Math.Min(p1.Y, p2.Y), p3.Y), 0);
            int xmax = Math.Min(Math.Max(Math.Max(p1.X, p2.X), p3.X), _CachedscreenSize.X);
            int ymax = Math.Min(Math.Max(Math.Max(p1.Y, p2.Y), p3.Y), _CachedscreenSize.Y);

            int width = xmax - xmin + 1;
            int height = ymax - ymin + 1;

            //if (xmax < 0 || xmin > CachescreenSize.X || ymax < 0 || ymin > CachescreenSize.Y) return;

            //if (width < 0 || height < 0) return;

            int numOfPixels = width * height;
            if (numOfPixels < 0 || numOfPixels > 1000000) return;

            _texture = new Texture2D(spriteBatch.GraphicsDevice, width, height);
            _pixelBuffer = new Color[numOfPixels];


            int BYminusCY = p2.Y - p3.Y;
            int AXminusCX = p1.X - p3.X;
            int CXminusBX = p3.X - p2.X;

            fixed (Color* dataPtr = _pixelBuffer)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (IsPointInTriangle(x + xmin, y + ymin, p1, p2, p3, BYminusCY, AXminusCX, CXminusBX))
                        {
                            _pixelBuffer[y * width + x] = color;
                        }
                    }
                }
            }

            _texture.SetData(_pixelBuffer);

            spriteBatch.Begin();
            spriteBatch.Draw(_texture, new Rectangle(xmin, ymin, width, height), null, Color.White, 0, Vector2.Zero, 0, 1f / distance);
            spriteBatch.End();
        }
        public unsafe void Draw(
            ref TextureBuffer screenBuffer,
            Color color,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            int distance
            )
        {
            if (
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.P1, out Point p1) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.P2, out Point p2) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.P3, out Point p3)
            ) { return; }

            if (IsBackFacing(p1, p2, p3)) return; // Skip back-facing triangles

            /* calculates a bounding rectangle for the triangle */
            int xmin = Math.Max(General.min3(p1.X, p2.X, p3.X), 0);
            int ymin = Math.Max(General.min3(p1.Y, p2.Y, p3.Y), 0);
            int xmax = Math.Min(General.max3(p1.X, p2.X, p3.X), _CachedscreenSize.X);
            int ymax = Math.Min(General.max3(p1.Y, p2.Y, p3.Y), _CachedscreenSize.Y);

            int width = xmax - xmin;
            int height = ymax - ymin;

            if (width < 1 || height < 1) return;


            int BYminusCY = p2.Y - p3.Y;
            int AXminusCX = p1.X - p3.X;
            int CXminusBX = p3.X - p2.X;

            fixed (Color* screenBufferColorPtr = screenBuffer.Pixels)
            fixed (int* screenBufferDistancePtr = screenBuffer.Distance)
                for (int y = ymin; y < ymax; y++)
                {
                    int yTimesWidth = y * _CachedscreenSize.X;
                    for (int x = xmin; x < xmax; x++)
                    {
                        int index = yTimesWidth + x;

                        if (distance <= screenBufferDistancePtr[index] && IsPointInTriangle(x, y, p1, p2, p3, BYminusCY, AXminusCX, CXminusBX))
                        {
                            screenBufferColorPtr[index] = color;
                            screenBufferDistancePtr[index] = distance;
                        }
                    }
                }
        }
        public static bool IsPointInTriangle(int px, int py, Point a, Point b, Point c, int BYminusCY, int AXminusCX, int CXminusBX)
        {
            int PXminusCX = px - c.X;
            int PYminusCY = py - c.Y;
            int denom = BYminusCY * AXminusCX + CXminusBX * (a.Y - c.Y);
            int w1 = BYminusCY * PXminusCX + CXminusBX * PYminusCY;
            int w2 = (c.Y - a.Y) * PXminusCX + AXminusCX * PYminusCY;
            int w3 = denom - w1 - w2;

            //if (denom < 0) { w1 = -w1; w2 = -w2; w3 = -w3; denom = -denom; }

            return w1 >= 0 && w2 >= 0 && w3 >= 0;
        }
        private static bool IsBackFacing(Point a, Point b, Point c)
        {
            // Compute the edge vectors
            int edge1X = b.X - a.X;
            int edge1Y = b.Y - a.Y;
            int edge2X = c.X - a.X;
            int edge2Y = c.Y - a.Y;

            // Compute the cross product (determines winding order)
            int crossProduct = (edge1X * edge2Y) - (edge1Y * edge2X);

            // If cross product is negative, it's back-facing (assuming CCW winding)
            return crossProduct <= 0;
        }
        public static bool WorldPosToScreenPos(
                Vector3 cameraPosition,
                float pitch,
                float yaw,
                Vector3 objectPosition,
                out Point screenPos
            )
        {
            Vector3 relativePos = objectPosition - cameraPosition;
            Vector3 rotatedrelativePos = General.RotateVector(relativePos, yaw, pitch);

            if (rotatedrelativePos.Z < 0) { screenPos = Point.Zero; return false; } // Object is behind the camera, return as failed

            screenPos = new Point(
                (int)((rotatedrelativePos.X / rotatedrelativePos.Z) * _fov_scale * _screenCenter.X + _screenCenter.X),
                (int)((rotatedrelativePos.Y / rotatedrelativePos.Z) * _fov_scale * _screenCenter.Y + _screenCenter.Y)
            );

            return true;
        }
        public static Vector3 RotateVector(Vector3 vector, float yaw, float pitch)
        {
            // Yaw rotation (around Y axis) // first for fps feel
            float cosYaw = MathF.Cos(yaw);
            float sinYaw = MathF.Sin(yaw);
            float x1 = vector.X * cosYaw - vector.Z * sinYaw;
            float z1 = vector.X * sinYaw + vector.Z * cosYaw;

            // Pitch rotation (around X axis)
            float cosPitch = MathF.Cos(pitch);
            float sinPitch = MathF.Sin(pitch);
            float y1 = vector.Y * cosPitch - z1 * sinPitch;
            float z2 = vector.Y * sinPitch + z1 * cosPitch;

            return new Vector3(x1, y1, z2);
        }
        public Color ApplyShading(Vector3 lightDirection, Color triangleColor, Color lightColor)
        {
            lightDirection.Normalize();

            Vector3 side1 = P1 - P2;
            Vector3 side2 = P1 - P3;
            Vector3 normalDir = Vector3.Cross(side1, side2);
            normalDir.Normalize();

            // Calculate the difference in rays between the light direction and the normal vector using Vector3.Dot
            float dotProduct = Vector3.Dot(normalDir, lightDirection);

            // mix colors based on the difference in rays
            return Color.Lerp(triangleColor, lightColor, dotProduct);
        }
        public unsafe static Shape[] ModelConstructor((int, int, int)[] Triangles, Vector3[] Vertices)
        {
            Shape[] ProcessedTriangles = new Shape[Triangles.Length];
            fixed (Shape* processedTrianglesPtr = ProcessedTriangles)
            fixed ((int, int, int)* trianglesPtr = Triangles)
            fixed (Vector3* verticesPtr = Vertices)
            {
                // Process each triangle and construct Triangle objects
                for (int i = 0; i < Triangles.Length; i++)
                {
                    processedTrianglesPtr[i] = new Triangle(
                        verticesPtr[trianglesPtr[i].Item1],
                        verticesPtr[trianglesPtr[i].Item2],
                        verticesPtr[trianglesPtr[i].Item3]
                    );
                }
            }
            return ProcessedTriangles;
        } // constructs triangles out of models, takes in vertices and triangles as indeces
    }
}
