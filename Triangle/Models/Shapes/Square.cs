using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.GameAsset;

namespace SlimeGame.Models.Shapes
{
    internal struct Square : Shape
    {

        Vector3 _p1;
        Vector3 _p2;
        Vector3 _p3;
        Vector3 _p4;

        public Vector3 P1 { get => this._p1; set => this._p1 = value; }
        public Vector3 P2 { get => this._p2; set => this._p2 = value; }
        public Vector3 P3 { get => this._p3; set => this._p3 = value; }
        public Vector3 P4 { get => this._p4; set => this._p4 = value; }
        public Color Color { get; set; }

        static readonly (int, int, int)[] triangleIndexes = new (int, int, int)[]
                {
                    (1, 2, 3),
                    (1, 3, 4)
                };
        public Shape[] Triangles { get => Triangle.ModelConstructor(triangleIndexes, new Vector3[] { _p1, _p2, _p3, _p4}, Color); }
        Vector3 Shape.Position => _p1;
        Color Shape.Color
        {
            get => Color;
            set => Color = value;
        }

        static Point _screenCenter;
        static Point _CachedscreenSize;
        static float _fov_scale;
        static float ScaleX;
        static float ScaleY;
        public static void Initialize(TextureBuffer screenBuffer) // call once per frame
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
        public static Vector3 Normal(Vector3 P1, Vector3 P2, Vector3 P3, Vector3 P4)
        {
            Vector3 side1 = P2 - P1;
            Vector3 side2 = P3 - P1;
            Vector3 side3 = P4 - P1;

            Vector3 normalDir1 = Vector3.Cross(side1, side2);
            normalDir1.Normalize();
            Vector3 normalDir2 = Vector3.Cross(side2, side3);
            normalDir2.Normalize();

            Vector3 normalDir = (normalDir1 + normalDir2) / 2;
            return normalDir;
        }
        public readonly Vector3 Normal()
            => Normal(_p1, _p2, _p3,_p4);
        
        public Color ApplyShading(Vector3 lightDirection, Color triangleColor, Color lightColor)
        {
            lightDirection.Normalize();

            Vector3 normalDir = Normal();

            // Calculate the difference in rays between the light direction and the normal vector using Vector3.Dot
            float dotProduct = Vector3.Dot(normalDir, lightDirection);

            // mix colors based on the difference in rays
            return Color.Lerp(triangleColor, lightColor, dotProduct / 2);
        }
        public Square(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color = new())
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;
            Color = color;
        }
        public unsafe void Draw(
            ref TextureBuffer screenBuffer,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            int distance,
            Color color
            )
        {
            if (
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, _p1, out Point p1) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, _p2, out Point p2) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, _p3, out Point p3) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, _p4, out Point p4)
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

                        if (distance > screenBufferDistancePtr[index]) continue;
                        if (IsPointNotInQuad(x, y, p1, p2, p3, p4)) continue;

                        screenBufferColorPtr[index] = color;
                        screenBufferDistancePtr[index] = distance;
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
                (int)(rotatedrelativePos.X / rotatedrelativePos.Z * ScaleX + _screenCenter.X),
                (int)(rotatedrelativePos.Y / rotatedrelativePos.Z * ScaleY + _screenCenter.Y)
            );

            return true;
        }
        public static bool IsPointNotInQuad(int x, int y, Point p1, Point p2, Point p3, Point p4)
        {
            int c1 = (p2.X - p1.X) * (y - p1.Y) - (p2.Y - p1.Y) * (x - p1.X);
            int c2 = (p3.X - p2.X) * (y - p2.Y) - (p3.Y - p2.Y) * (x - p2.X);
            int c3 = (p4.X - p3.X) * (y - p3.Y) - (p4.Y - p3.Y) * (x - p3.X);
            int c4 = (p1.X - p4.X) * (y - p4.Y) - (p1.Y - p4.Y) * (x - p4.X);

            bool hasNeg = (c1 < 0) || (c2 < 0) || (c3 < 0) || (c4 < 0);
            bool hasPos = (c1 > 0) || (c2 > 0) || (c3 > 0) || (c4 > 0);

            return hasNeg && hasPos;
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
