using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame
{
    internal class General
    {
        static Stopwatch _doubleClickTimer = new();
        public static Vector3 RandomVector3(int min, int max, Random rnd)
        {
            return new Vector3(rnd.Next(min, max), rnd.Next(min, max), rnd.Next(min, max));
        }
        public static int min4(int a, int b, int c, int d)
        {
            int m = (a < b) ? a : b;
            m = (m < c) ? m : c;
            return (m < d) ? m : d;
            
        }
        public static int max4(int a, int b, int c, int d)
        {
            int m = (a > b) ? a : b;
            m = (m > c) ? m : c;
            return (m > d) ? m : d;
        }
        public static int min3(int a, int b, int c)
        {
            int t = (a < b) ? a : b;
            return (t < c) ? t : c;
        }
        public static int max3(int a, int b, int c)
        {
            int t = (a > b) ? a : b;
            return (t > c) ? t : c;
        }
        public static Color Divide(Color color, float num) => new Color(color.R / num, color.G / num, color.B / num);
        public static Color Multiply(Color color, float num) => new Color(color.R * num, color.G * num, color.B * num);

        public static void DrawObject(
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
            Color color,
            Vector3 objectPosition)
        {
            if (texture == null) return;

            Vector3 relativePos = objectPosition - cameraPosition;
            Vector3 rotatedrelativePos = RotateVector(relativePos, yaw, pitch);

            if (rotatedrelativePos.Z < 0) return; // Object is behind the camera, skip drawing
            //calculate screen Position
            float fov_scale = 1f / MathF.Tan(FOV / 2);
            Point screenPos = new Point(
                (int)((rotatedrelativePos.X / rotatedrelativePos.Z) * fov_scale * (screenSize.X / 2) + (screenSize.X / 2)),
                (int)((rotatedrelativePos.Y / rotatedrelativePos.Z) * fov_scale * (screenSize.Y / 2) + (screenSize.Y / 2))
            );

            // 2. Calculate size (world units to screen units)
            float scale = 50 / rotatedrelativePos.Z;
            int finalWidth = (int)(width * scale);
            int finalHeight = (int)(height * scale);

            if (finalWidth < 4 || finalHeight < 4) return;

            // 3. Center the object
            screenPos.X -= finalWidth / 2;
            screenPos.Y -= finalHeight / 2;

            // 4. Draw with depth-aware coloring
            spriteBatch.Draw(
                texture,
                new Rectangle(screenPos.X, screenPos.Y, finalWidth, finalHeight),
                sourceRect,
                color,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                1 / Vector3.DistanceSquared(cameraPosition, objectPosition)
            );
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

        public static float AngleDifference(float a, float b)
        {
            float diff = a - b;
            while (diff > MathF.PI) diff -= MathF.Tau;
            while (diff < -MathF.PI) diff += MathF.Tau;
            return diff;
        }
        
        public static void Bound(ref float value, float maxValue) //handles overflows of a value by setting it back to zero, and zero to the maxvalue
        {
            value = (value % maxValue + maxValue) % maxValue;
        }
        public static int Bound(int value, int maxValue) //handles overflows of a value by setting it back to zero, and zero to the maxvalue
        {
            return (value % maxValue + maxValue) % maxValue;
        }
        public static float ToRadians(float degrees)
        {
            return degrees * (MathF.PI / 180f);
        }

        public static Vector3 RotateX(Vector3 Vector, float yaw)
        {

            float cosYaw = (float)Math.Cos(yaw);
            float sinYaw = (float)Math.Sin(yaw);
            float x1 = Vector.X * cosYaw + Vector.Z * sinYaw;
            float z1 = -Vector.X * sinYaw + Vector.Z * cosYaw;
            return new(x1, Vector.Y, z1);
        }
        public static Vector3 RotateY(Vector3 Vector, float yaw)
        {

            float cosYaw = (float)Math.Cos(yaw);
            float sinYaw = (float)Math.Sin(yaw);
            float x1 = Vector.X * cosYaw + Vector.Z * sinYaw;
            float z1 = -Vector.X * sinYaw + Vector.Z * cosYaw;
            return new(x1, Vector.Y, z1);
        }
        public static Vector3 Rotate(Vector3 Vector, float yaw, float pitch)
        {
            // Yaw: rotate around Y-axis
            float cosYaw = (float)Math.Cos(yaw);
            float sinYaw = (float)Math.Sin(yaw);
            float x1 = Vector.X * cosYaw + Vector.Z * sinYaw;
            float z1 = -Vector.X * sinYaw + Vector.Z * cosYaw;

            // Pitch: rotate around X-axis
            float cosPitch = (float)Math.Cos(pitch);
            float sinPitch = (float)Math.Sin(pitch);
            float y1 = Vector.Y * cosPitch - z1 * sinPitch;
            float z2 = Vector.Y * sinPitch + z1 * cosPitch;

            Vector.X = x1;
            Vector.Y = y1;
            Vector.Z = z2;

            return Vector;
        }
        public static Vector2 AngleBetween(Vector3 from, Vector3 to)
        {
            // Normalize
            from.Normalize();
            to.Normalize();

            // --- YAW DIFFERENCE (around Y axis) ---
            // Project vectors onto XZ plane
            Vector3 fromYaw = new Vector3(from.X, 0f, from.Z);
            Vector3 toYaw = new Vector3(to.X, 0f, to.Z);

            fromYaw.Normalize();
            toYaw.Normalize();

            float yaw = (float)Math.Atan2(
                toYaw.X * fromYaw.Z - toYaw.Z * fromYaw.X,   // cross (Y sign)
                Vector3.Dot(fromYaw, toYaw)                  // dot
            );

            // --- PITCH DIFFERENCE (around X axis) ---
            // Pitch is based on vertical angle between vectors
            float pitchFrom = (float)Math.Asin(from.Y);
            float pitchTo = (float)Math.Asin(to.Y);

            float pitch = pitchTo - pitchFrom;

            return new Vector2(yaw, pitch);
        }
        public static Vector3 angleToVector3(Vector2 angle)
        {
            return
            new Vector3(
            (float)(Math.Cos(angle.Y) * Math.Sin(angle.X)),
            (float)(Math.Sin(angle.Y)),
            (float)(Math.Cos(angle.Y) * Math.Cos(angle.X))
            );
        }
        public static bool OnDoubleClick(MouseState mouseState, MouseState previousMouseState)
        {
            if (OnLeftPress(mouseState, previousMouseState))
            {
                if (_doubleClickTimer.ElapsedMilliseconds > 10 && (_doubleClickTimer.ElapsedMilliseconds < 250))
                {
                    _doubleClickTimer.Reset();
                    return true;
                }
                else
                {
                    _doubleClickTimer.Reset();
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static bool OnRightReleased(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed);
        }
        public static bool OnRightPress(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released);
        }
        public static bool OnLeftReleased(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed);
        }
        public static bool OnLeftPress(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released);
        }
        public static bool OnRelease(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed);
        }
        public static bool OnPress(KeyboardState keyboardState, KeyboardState previousKeyboardState, Keys key)
        {
            return (keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key));
        }
        public static bool OnRelease(KeyboardState keyboardState, KeyboardState previousKeyboardState, Keys key)
        {
            return (keyboardState.IsKeyUp(key) && previousKeyboardState.IsKeyDown(key));
        }
        public static Vector2 Normalize(Vector2 vector, float hypotinuse)
        {
            float length = vector.X * vector.X + vector.Y * vector.Y;
            if (length > 0)
            {
                vector *= hypotinuse / MathF.Sqrt(length);
            }
            return vector;
        }
        public static Vector3 Normalize(Vector3 vector, float hypotenuse)
        {
            float length = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            if (length > 0)
            {
                vector *= hypotenuse / length;
            }

            return vector;
        }

        public static Color colorMultiply(Color color, float num)
        {
            return new Color(color.R * num, color.G * num, color.B * num, color.A);
        }
        public static Vector2 ToVector2(Vector3 vector)
        {
            return new(vector.X, vector.Y);
        }
        public static float Vector2ToAngle(Vector2 angle)
        {
            return (float)Math.Atan2(angle.Y, angle.X);
        }
        public static Vector2 AngleToVector2(double angle)
        {
            return new((float)Math.Sin(angle), (float)Math.Cos(angle));
        }
    }
}
