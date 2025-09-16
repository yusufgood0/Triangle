using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Triangle
{
    internal struct Mesh(int[][] Indices, Vector3[] Vertices)
    {
        static Point _screenCenter;
        static Point _CachedscreenSize;
        static float _fov_scale;
        static float ScaleX;
        static float ScaleY;
        static Point _errorPoint = Point.Zero;
        public static void Initialize(SpriteBatch spritebatch, TextureBuffer screenBuffer) // call once per frame
        {

            _screenCenter = new Point(screenBuffer.width / 2, screenBuffer.height / 2);
            _CachedscreenSize = new Point(screenBuffer.width, screenBuffer.height);
        }
        public static void UpdateConstants(float FOV)
        {
            _fov_scale = 1f / MathF.Tan(FOV / 2);
            ScaleX = _fov_scale * _screenCenter.X;
            ScaleY = _fov_scale * _screenCenter.Y;
        }
        public unsafe void Draw(
            ref TextureBuffer screenBuffer,
            Color[] colors,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            Vector3 lightPos,
            Color lightColor
            )
        {
            Point[] Points = new Point[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                WorldPosToScreenPos(cameraPosition, pitch, yaw, Vertices[i], out var Point);
                Points[i] = Point;
            }
            for (int i = 0; i < Indices.Length; i++)
            {
                var indexes = Indices[i];
                if (Points[indexes[0]] == _errorPoint)
                    continue;
                if (Points[indexes[1]] == _errorPoint)
                    continue;
                if (Points[indexes[2]] == _errorPoint)
                    continue;
                if (Points[indexes[3]] == _errorPoint)
                    continue;

                Vector3 lightDirection = Vertices[indexes[0]] - lightPos;
                lightDirection.Normalize();

                Vector3 side1 = Vertices[indexes[0]] - Vertices[indexes[1]];
                Vector3 side2 = Vertices[indexes[0]] - Vertices[indexes[2]];
                Vector3 normalDir = Vector3.Cross(side1, side2);
                normalDir.Normalize();

                // Calculate the difference in rays between the light direction and the normal vector using Vector3.Dot
                float dotProduct = Vector3.Dot(-normalDir, lightDirection);

                // mix colors based on the difference in rays
                Color color = Color.Lerp(colors[i], lightColor, dotProduct/3);

                int distance = (int)Vector3.Distance(cameraPosition, Vertices[indexes[0]]);
                DrawSquare(ref screenBuffer, color, cameraPosition, pitch, yaw, distance, indexes, Points);
                continue;
            }

        }
        public unsafe void DrawSquare(
            ref TextureBuffer screenBuffer,
            Color color,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            int distance,
            int[] indexes,
            Point[] Points
            )
        {
            Point p1 = Points[indexes[0]];
            Point p2 = Points[indexes[1]];
            Point p3 = Points[indexes[2]];
            Point p4 = Points[indexes[3]];


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

            if (rotatedrelativePos.Z < 0) { screenPos = _errorPoint; return false; } // Object is behind the camera, return as failed

            screenPos = new Point(
                (int)((rotatedrelativePos.X / rotatedrelativePos.Z) * ScaleX + _screenCenter.X),
                (int)((rotatedrelativePos.Y / rotatedrelativePos.Z) * ScaleY + _screenCenter.Y)
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
    }
}
