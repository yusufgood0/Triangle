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
        }
        public static void UpdateConstants(float FOV, Point screenSize) // call once per frame
        {
            _fov_scale = 1f / MathF.Tan(FOV / 2);
        }
        public static unsafe void BulkDraw(
            List<Triangle> triangles,
            SpriteBatch spriteBatch,
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
                    if (numOfPixels < 1 || numOfPixels > 1000000) return;

                    Byte[] data = new Byte[numOfPixels];
                    //Array.Fill(data, Byte);
                    fixed (Byte* dataPtr = data)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                data[y * width + x] = IsPointInTriangle(new Point(x + xmin, y + ymin), p1, p2, p3) ? (Byte)255: (Byte)0;
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
            int xmin = Math.Min(Math.Min(p1.X, p2.X), p3.X);
            int ymin = Math.Min(Math.Min(p1.Y, p2.Y), p3.Y);
            int xmax = Math.Max(Math.Max(p1.X, p2.X), p3.X);
            int ymax = Math.Max(Math.Max(p1.Y, p2.Y), p3.Y);

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
                        if (IsPointInTriangle(new Point(x + xmin, y + ymin), p1, p2, p3))
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
        static bool IsPointInTriangle(Point P, Point A, Point B, Point C)
        {
            bool sign1 = Sign(P, A, B) < 0f;
            bool sign2 = Sign(P, B, C) < 0f;
            bool sign3 = Sign(P, C, A) < 0f;

            return (sign1 == sign2) && (sign2 == sign3);
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
