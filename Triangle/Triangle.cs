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
        public Vector3 p1 { get; set; }
        public Vector3 p2 { get; set; }
        public Vector3 p3 { get; set; }
        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        static Color transparentColor = new Color(0, 0, 0, 255);
        static Point errorPoint = new Point(-1, -1);
        static float fov_scale;
        static Point screenCenter;
        static Point CachescreenSize;

        public static void UpdateConstants(float FOV, Point screenSize) // call once per frame
        {
            fov_scale = 1f / MathF.Tan(FOV / 2);
            screenCenter = new Point(screenSize.X / 2, screenSize.Y / 2);
            CachescreenSize = screenSize;
        }

        public void Draw(
            SpriteBatch spriteBatch,
            GraphicsDevice graphicsDevice,
            Color color,
            Vector3 cameraPosition,
            float pitch,
            float yaw
            )
        {
            if (
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.p1, out Point p1) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.p2, out Point p2) ||
            !WorldPosToScreenPos(cameraPosition, pitch, yaw, this.p3, out Point p3)
            ) { return; }

            /* calculates a bounding rectangle for the triangle */
            int xmin = Math.Min(Math.Min(p1.X, p2.X), p3.X);
            int ymin = Math.Min(Math.Min(p1.Y, p2.Y), p3.Y);
            int xmax = Math.Max(Math.Max(p1.X, p2.X), p3.X);
            int ymax = Math.Max(Math.Max(p1.Y, p2.Y), p3.Y);

            int width = xmax - xmin + 1;
            int height = ymax - ymin + 1;

            //if (xmax < 0 || xmin > CachescreenSize.X || ymax < 0 || ymin > CachescreenSize.Y) return;

            if (width < 0 || height < 0) return;

            int numOfPixels = width * height;
            if (numOfPixels > 1000000) return;
            Color[] data = new Color[numOfPixels];
            Array.Fill(data, transparentColor);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsPointInTriangle(new Point(x + xmin, y + ymin), p1, p2, p3))
                    {
                        data[y * width + x] = Color.White;
                    }
                }
            }
            using (Texture2D texture = new Texture2D(spriteBatch.GraphicsDevice, width, height))
            {
                texture.SetData(data);

                spriteBatch.Begin();
                spriteBatch.Draw(texture, new Rectangle(xmin, ymin, width, height), color);
                spriteBatch.End();
            }
            //texture.SetData(data);


        }
        bool IsPointInTriangle(Point P, Point A, Point B, Point C)
        {
            bool sign1 = Sign(P, A, B) < 0f;
            bool sign2 = Sign(P, B, C) < 0f;
            bool sign3 = Sign(P, C, A) < 0f;

            return (sign1 == sign2) && (sign2 == sign3);
        }

        float Sign(Point p1, Point p2, Point p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public bool WorldPosToScreenPos(
                Vector3 cameraPosition,
                float pitch,
                float yaw,
                Vector3 objectPosition,
                out Point screenPos
            )
        {
            Vector3 relativePos = objectPosition - cameraPosition;
            Vector3 rotatedrelativePos = General.RotateVector(relativePos, yaw, pitch);

            if (rotatedrelativePos.Z < 0) { screenPos = errorPoint; return false; } // Object is behind the camera, return null

            screenPos = new Point(
                (int)((rotatedrelativePos.X / rotatedrelativePos.Z) * fov_scale * screenCenter.X + screenCenter.X),
                (int)((rotatedrelativePos.Y / rotatedrelativePos.Z) * fov_scale * screenCenter.Y + screenCenter.Y)
            );

            return true;
        }
    }
}
