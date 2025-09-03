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
    internal struct Square : Shape
    {
        Vector3 Shape.Position => vertices[0];

        public static (int, int, int)[] triangles = new (int, int, int)[]
                {
                    (1, 2, 3),
                    (1, 3, 4)
                };
        public Vector3[] vertices;
        public Shape[] Triangles { get => Triangle.ModelConstructor(triangles, vertices); }
        static Point _screenCenter;
        static Point _CachedscreenSize;
        static float _fov_scale;
        public Vector3 Average()
        {
            return (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4;
        }
        public static void Initialize(SpriteBatch spritebatch, TextureBuffer screenBuffer) // call once per frame
        {
            _screenCenter = new Point(screenBuffer.width / 2, screenBuffer.height / 2);
            _CachedscreenSize = new Point(screenBuffer.width, screenBuffer.height);
        }
        public static void UpdateConstants(float FOV)
        {
            _fov_scale = 1f / MathF.Tan(FOV / 2);
        }
        public Color ApplyShading(Vector3 lightDirection, Color triangleColor, Color lightColor)
        {
            lightDirection.Normalize();

            Shape triangle = Triangles[0];
            Vector3 side1 = vertices[0] - vertices[1];
            Vector3 side2 = vertices[0] - vertices[3];
            Vector3 normalDir = Vector3.Cross(side1, side2);
            normalDir.Normalize();

            // Calculate the difference in rays between the light direction and the normal vector using Vector3.Dot
            float dotProduct = Vector3.Dot(normalDir, lightDirection);

            // mix colors based on the difference in rays
            return Color.Lerp(triangleColor, lightColor, dotProduct);
        }
        public Square(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            vertices = new Vector3[] { p1, p2, p3, p4 };

            float distance12 = Vector3.DistanceSquared(p1, p2);
            float distance13 = Vector3.DistanceSquared(p1, p3);
            float distance14 = Vector3.DistanceSquared(p1, p4);


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
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.vertices[0], out Point p1) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.vertices[1], out Point p2) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.vertices[2], out Point p3) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.vertices[3], out Point p4)
            ) { return; }

            //if (IsBackFacing(p1, p2, p3)) return; // Skip back-facing triangles

            /* calculates a bounding rectangle for the triangle */
            int xmin = Math.Max(General.min4(p1.X, p2.X, p3.X, p4.X), 0);
            int ymin = Math.Max(General.min4(p1.Y, p2.Y, p3.Y, p4.Y), 0);
            int xmax = Math.Min(General.max4(p1.X, p2.X, p3.X, p4.X), _CachedscreenSize.X);
            int ymax = Math.Min(General.max4(p1.Y, p2.Y, p3.Y, p4.Y), _CachedscreenSize.Y);

            int width = xmax - xmin;
            int height = ymax - ymin;

            if (width < 1 || height < 1) return;

            fixed (Color* screenBufferColorPtr = screenBuffer.Pixels)
            fixed (int* screenBufferDistancePtr = screenBuffer.Distance)
                for (int y = ymin; y < ymax; y++)
                {
                    int yTimesWidth = y * _CachedscreenSize.X;
                    for (int x = xmin; x < xmax; x++)
                    {
                        int index = yTimesWidth + x;

                        if (distance <= screenBufferDistancePtr[index] &&
                            IsPointInQuad(x, y, p1, p2, p3, p4))
                        {
                            screenBufferColorPtr[index] = color;
                            screenBufferDistancePtr[index] = distance;
                        }
                    }
                }
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
        public static bool IsPointInQuad(int x, int y, Point p1, Point p2, Point p3, Point p4)
        {
            float v1x = p1.X - x, v1y = p1.Y - y;
            float v2x = p2.X - x, v2y = p2.Y - y;
            float v3x = p3.X - x, v3y = p3.Y - y;
            float v4x = p4.X - x, v4y = p4.Y - y;

            bool hasPos = false, hasNeg = false;

            float c1 = v1x * v2y - v1y * v2x;
            if (c1 > 0) hasPos = true; else if (c1 < 0) hasNeg = true;
            if (hasPos && hasNeg) return false;

            float c2 = v2x * v3y - v2y * v3x;
            if (c2 > 0) hasPos = true; else if (c2 < 0) hasNeg = true;
            if (hasPos && hasNeg) return false;

            float c3 = v3x * v4y - v3y * v4x;
            if (c3 > 0) hasPos = true; else if (c3 < 0) hasNeg = true;
            if (hasPos && hasNeg) return false;

            float c4 = v4x * v1y - v4y * v1x;
            if (c4 > 0) hasPos = true; else if (c4 < 0) hasNeg = true;

            return !(hasPos && hasNeg);
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
    }
}
