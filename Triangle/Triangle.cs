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
    internal struct Triangle
    {
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
        static TriangleBulkDraw _triangleBulkDraw;
        public static void Initialize(SpriteBatch spritebatch, Point screenSize) // call once per frame
        {
            _screenCenter = new Point(screenSize.X / 2, screenSize.Y / 2);
            _CachedscreenSize = screenSize;
            _triangleBulkDraw = new TriangleBulkDraw(spritebatch, screenSize);
            _texture = new Texture2D(spritebatch.GraphicsDevice, 1, 1);
            _texture.SetData(new Color[1] { Color.White });
        }
        public static void UpdateConstants(float FOV, Point screenSize) // call once per frame
        {
            _fov_scale = 1f / MathF.Tan(FOV / 2);
        }
        public static unsafe void BulkDraw(
            List<Triangle> triangles,
            ref SpriteBatch spriteBatch,
            Color color,
            Vector3 cameraPosition,
            float pitch,
            float yaw
            )
        {
            fixed (Triangle* trianglesPtr = triangles.ToArray())
            {
                for (int i = 0; i < triangles.Count; i++)
                {
                    if (
                        !WorldPosToScreenPos(cameraPosition, pitch, yaw, trianglesPtr[i].P1, out Point p1) ||
                        !WorldPosToScreenPos(cameraPosition, pitch, yaw, trianglesPtr[i].P2, out Point p2) ||
                        !WorldPosToScreenPos(cameraPosition, pitch, yaw, trianglesPtr[i].P3, out Point p3)
                        ) { return; }

                    /* calculates a bounding box for the triangle */
                    int xmin = Math.Max(Math.Min(Math.Min(p1.X, p2.X), p3.X), 0);
                    int ymin = Math.Max(Math.Min(Math.Min(p1.Y, p2.Y), p3.Y), 0);
                    int xmax = Math.Min(Math.Max(Math.Max(p1.X, p2.X), p3.X), _CachedscreenSize.X);
                    int ymax = Math.Min(Math.Max(Math.Max(p1.Y, p2.Y), p3.Y), _CachedscreenSize.Y);

                    int width = xmax - xmin;
                    int height = ymax - ymin;

                    int numOfPixels = width * height;
                    if (width < 1 || width > 1000 || height < 1 || height > 1000) continue;
                    //if (numOfPixels < 0 || numOfPixels > 1000000) continue;
                    Color[] data = new Color[numOfPixels];
                    //Array.Fill(data, Byte);
                    fixed (Color* dataPtr = data)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                if (IsPointInTriangle(x + xmin, y + ymin, p1, p2, p3))
                                {
                                    dataPtr[y * width + x] = Color.White;
                                }
                                else
                                {
                                    dataPtr[y * width + x] = Color.Transparent;
                                }
                            }
                        }
                    }
                    _triangleBulkDraw.AddTriangle(
                        new Rectangle(xmin, ymin, width, height),
                        color,
                        data
                    );
                }
            }
            _triangleBulkDraw.ProcessAll(ref spriteBatch);
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


            fixed (Color* dataPtr = _pixelBuffer)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (IsPointInTriangle(x + xmin, y + ymin, p1, p2, p3))
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

            /* calculates a bounding rectangle for the triangle */
            int xmin = Math.Max(General.min3(p1.X, p2.X, p3.X), 0);
            int ymin = Math.Max(General.min3(p1.Y, p2.Y, p3.Y), 0);
            int xmax = Math.Min(General.max3(p1.X, p2.X, p3.X), _CachedscreenSize.X);
            int ymax = Math.Min(General.max3(p1.Y, p2.Y, p3.Y), _CachedscreenSize.Y);

            int width = xmax - xmin;
            int height = ymax - ymin;

            if (width < 1 || height < 1 ) return;

            fixed (Color* screenBufferColorPtr = screenBuffer.Pixels)
            fixed (int* screenBufferLayerPtr = screenBuffer.Layer)
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int currentpx = x + xmin;
                        int currentpy = y + ymin;
                        int index = currentpy * _CachedscreenSize.X + currentpx;

                        if (distance < screenBufferLayerPtr[index] && IsPointInTriangle(currentpx, currentpy, p1, p2, p3))
                        {
                            screenBufferColorPtr[index] = color;
                            screenBufferLayerPtr[index] = distance;
                        }
                    }
                }
        }
        public unsafe void DirectDraw(
            ref SpriteBatch spriteBatch,
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

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int currentpx = x + xmin;
                    int currentpy = y + ymin;
                    if (IsPointInTriangle(currentpx, currentpy, p1, p2, p3))
                    {
                        spriteBatch.Draw(_texture, new Rectangle(currentpx, currentpy, 1, 1), null, color, 0, Vector2.Zero, 0, 1f / distance);
                    }
                }
            }
            spriteBatch.End();
        }
        static bool IsPointInTriangle(int px, int py, Point a, Point b, Point c)
        {
            int denom = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
            int w1 = (b.Y - c.Y) * (px - c.X) + (c.X - b.X) * (py - c.Y);
            int w2 = (c.Y - a.Y) * (px - c.X) + (a.X - c.X) * (py - c.Y);
            int w3 = denom - w1 - w2;

            if (denom < 0) { w1 = -w1; w2 = -w2; w3 = -w3; denom = -denom; }

            return w1 >= 0 && w2 >= 0 && w3 >= 0;
        }

        static float Sign(Point p1, Point p2, Point p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
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

            if (rotatedrelativePos.Z < 0) { screenPos = _errorPoint; return false; } // Object is behind the camera, return null

            screenPos = new Point(
                (int)((rotatedrelativePos.X / rotatedrelativePos.Z) * _fov_scale * _screenCenter.X + _screenCenter.X),
                (int)((rotatedrelativePos.Y / rotatedrelativePos.Z) * _fov_scale * _screenCenter.Y + _screenCenter.Y)
            );

            return true;
        }
    }
}
