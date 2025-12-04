//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using SlimeGame.GameAsset;
//using SlimeGame.Models.Shapes;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;


//namespace SlimeGame.Models
//{
//    internal struct Mesh((int p1, int p2, int p3, int p4)[] SquareIndices, (int p1, int p2, int p3)[] TriangleIndeces, Vector3[] Vertices, Vector3 PivotPoint = new(), Vector2 Rotation = new())
//    {

//        static Point _screenCenter;
//        static Point _CachedscreenSize;
//        static float _scaleX;
//        static float _scaleY;
//        static Point _errorPoint = Point.Zero;


//        public static void Initialize(TextureBuffer screenBuffer) // call once per frame
//        {
//            _screenCenter = new Point(screenBuffer.width / 2, screenBuffer.height / 2);
//            _CachedscreenSize = new Point(screenBuffer.width, screenBuffer.height);
//        }
//        public static void UpdateConstants(float FOV)
//        {
//            float Fov_scale = 1f / MathF.Tan(FOV / 2);
//            _scaleX = Fov_scale * _screenCenter.X;
//            _scaleY = Fov_scale * _screenCenter.Y;
//        }
//        public void Draw(
//            ref TextureBuffer screenBuffer,
//            Color[] colors,
//            float lightIntensity,
//            Vector3 cameraPosition,
//            float pitch,
//            float yaw,
//            Vector3 lightPos,
//            Color lightColor
//            )
//        {
//            bool dontDrawSquares = SquareIndices == null;
//            bool dontDrawTriangles = TriangleIndeces == null;
//            if (dontDrawSquares && dontDrawTriangles) return;

//            Point[] Points = new Point[Vertices.Length];
//            for (int i = 0; i < Vertices.Length; i++)
//            {
//                WorldPosToScreenPos(cameraPosition, pitch, yaw, Vertices[i], out var Point);
//                Points[i] = Point;
//            }
//            if (!dontDrawSquares)
//            {
//                for (int i = 0; i < SquareIndices.Length; i++)
//                {
//                    var indexes = SquareIndices[i];
//                    Vector3 p1 = Vertices[indexes.p1];
//                    Vector3 p2 = Vertices[indexes.p2];
//                    Vector3 p3 = Vertices[indexes.p3];
//                    Vector3 p4 = Vertices[indexes.p4];
//                    if (Points[indexes.p1] == _errorPoint)
//                        continue;
//                    if (Points[indexes.p2] == _errorPoint)
//                        continue;
//                    if (Points[indexes.p3] == _errorPoint)
//                        continue;
//                    if (Points[indexes.p4] == _errorPoint)
//                        continue;

//                    // Compute normalized light direction (from point toward light)
//                    Vector3 lightDirection = Vector3.Normalize(lightPos - p1);

//                    // Compute triangle sides
//                    Vector3 side1 = p2 - p1;
//                    Vector3 side2 = p3 - p1;
//                    Vector3 side3 = p4 - p1;

//                    // Compute two normals (e.g. for adjacent triangles)
//                    Vector3 normalDir1 = Vector3.Normalize(Vector3.Cross(side1, side2));
//                    Vector3 normalDir2 = Vector3.Normalize(Vector3.Cross(side2, side3));

//                    // Average the normals
//                    Vector3 normalDir = (normalDir1 + normalDir2) * 0.5f;

//                    // Calculate how much the surface faces the light (clamped so no negative lighting)
//                    float dotProduct = MathF.Max(Vector3.Dot(normalDir, lightDirection), 0f);

//                    // Mix (Lerp) colors based on light intensity
//                    Color color = Color.Lerp(colors[i], lightColor, dotProduct * lightIntensity + 0.25f);

//                    int distance = (int)Vector3.Distance(cameraPosition, p1);
//                    DrawSquare(ref screenBuffer, color, cameraPosition, pitch, yaw, distance, indexes, Points);
//                    continue;
//                }
//            }
//            if (!dontDrawTriangles)
//            {
//                for (int i = 0; i < TriangleIndeces.Length; i++)
//                {
//                    var indexes = TriangleIndeces[i];
//                    Vector3 p1 = Vertices[indexes.p1];
//                    Vector3 p2 = Vertices[indexes.p2];
//                    Vector3 p3 = Vertices[indexes.p3];
//                    if (Points[indexes.p1] == _errorPoint)
//                        continue;
//                    if (Points[indexes.p2] == _errorPoint)
//                        continue;
//                    if (Points[indexes.p3] == _errorPoint)
//                        continue;

//                    Vector3 lightDirection = p1 - lightPos;
//                    lightDirection.Normalize();

//                    Vector3 side1 = p1 - p2;
//                    Vector3 side2 = p1 - p3;
//                    Vector3 normalDir = Vector3.Cross(side1, side2);
//                    normalDir.Normalize();

//                    // Calculate the difference in rays between the light direction and the normal vector using Vector3.Dot
//                    float dotProduct = Vector3.Dot(-normalDir, lightDirection);

//                    // mix colors based on the difference in rays
//                    Color color = Color.Lerp(colors[i], lightColor, dotProduct / 3);

//                    int distance = (int)Vector3.Distance(cameraPosition, p1);
//                    DrawTriangle(ref screenBuffer, color, cameraPosition, pitch, yaw, distance, indexes, Points);
//                    continue;
//                }
//            }
//        }
//        public readonly unsafe void DrawTriangle(
//            ref TextureBuffer screenBuffer,
//            Color color,
//            Vector3 cameraPosition,
//            float pitch,
//            float yaw,
//            int distance,
//            (int p1, int p2, int p3) indexes,
//            Point[] Points
//            )
//        {
//            Point p1 = Points[indexes.p1];
//            Point p2 = Points[indexes.p2];
//            Point p3 = Points[indexes.p3];


//            /* calculates a bounding rectangle for the square */
//            int xmin = Math.Max(General.min3(p1.X, p2.X, p3.X), 0);
//            int ymin = Math.Max(General.min3(p1.Y, p2.Y, p3.Y), 0);
//            int xmax = Math.Min(General.max3(p1.X, p2.X, p3.X), _CachedscreenSize.X);
//            int ymax = Math.Min(General.max3(p1.Y, p2.Y, p3.Y), _CachedscreenSize.Y);

//            int width = xmax - xmin;
//            int height = ymax - ymin;

//            if (width < 1 || height < 1) return;

//            int BYminusCY = p2.Y - p3.Y;
//            int AXminusCX = p1.X - p3.X;
//            int CXminusBX = p3.X - p2.X;

//            fixed (Color* screenBufferColorPtr = screenBuffer.Pixels)
//            fixed (int* screenBufferDistancePtr = screenBuffer.Distance)
//                for (int y = ymin; y < ymax; y++)
//                {
//                    int yTimesWidth = y * _CachedscreenSize.X;
//                    for (int x = xmin; x < xmax; x++)
//                    {
//                        int index = yTimesWidth + x;
//                        if (distance > screenBufferDistancePtr[index]) continue;
//                        if (!Triangle.IsPointInTriangle(x, y, p1, p2, p3, BYminusCY, AXminusCX, CXminusBX)) continue;

//                        screenBufferColorPtr[index] = color;
//                        screenBufferDistancePtr[index] = distance;
//                    }
//                }
//        }
//        public readonly unsafe void DrawSquare(
//            ref TextureBuffer screenBuffer,
//            Color color,
//            Vector3 cameraPosition,
//            float pitch,
//            float yaw,
//            int distance,
//            (int p1, int p2, int p3, int p4) indexes,
//            Point[] Points
//            )
//        {
//            Point p1 = Points[indexes.p1];
//            Point p2 = Points[indexes.p2];
//            Point p3 = Points[indexes.p3];
//            Point p4 = Points[indexes.p4];


//            /* calculates a bounding rectangle for the square */
//            int xmin = Math.Max(General.min4(p1.X, p2.X, p3.X, p4.X), 0);
//            int ymin = Math.Max(General.min4(p1.Y, p2.Y, p3.Y, p4.Y), 0);
//            int xmax = Math.Min(General.max4(p1.X, p2.X, p3.X, p4.X), _CachedscreenSize.X);
//            int ymax = Math.Min(General.max4(p1.Y, p2.Y, p3.Y, p4.Y), _CachedscreenSize.Y);

//            int width = xmax - xmin;
//            int height = ymax - ymin;

//            if (width < 1 || height < 1) return;

//            // Precompute calculations for edge functions
//            int p2minusp1X = p2.X - p1.X;
//            int p2minusp1Y = p2.Y - p1.Y;
//            int p3minusp2X = p3.X - p2.X;
//            int p3minusp2Y = p3.Y - p2.Y;
//            int p4minusp3X = p4.X - p3.X;
//            int p4minusp3Y = p4.Y - p3.Y;
//            int p1minusp4X = p1.X - p4.X;
//            int p1minusp4Y = p1.Y - p4.Y;

//            fixed (Color* screenBufferColorPtr = screenBuffer.Pixels)
//            fixed (int* screenBufferDistancePtr = screenBuffer.Distance)
//                for (int y = ymin; y < ymax; y++)
//                {
//                    int yTimesWidth = y * _CachedscreenSize.X;
//                    for (int x = xmin; x < xmax; x++)
//                    {
//                        int index = yTimesWidth + x;
//                        if (distance > screenBufferDistancePtr[index]) continue;
//                        if (Square.IsPointNotInQuad(x, y, p1, p2, p3, p4,
//                            p2minusp1X,
//                            p2minusp1Y,
//                            p3minusp2X,
//                            p3minusp2Y,
//                            p4minusp3X,
//                            p4minusp3Y,
//                            p1minusp4X,
//                            p1minusp4Y)) continue;

//                        screenBufferColorPtr[index] = color;
//                        screenBufferDistancePtr[index] = distance;
//                    }
//                }
//        }
//        public static bool WorldPosToScreenPos(
//                Vector3 cameraPosition,
//                float pitch,
//                float yaw,
//                Vector3 objectPosition,
//                out Point screenPos
//            )
//        {
//            Vector3 relativePos = objectPosition - cameraPosition;
//            Vector3 rotatedrelativePos = General.RotateVector(relativePos, yaw, pitch);

//            if (rotatedrelativePos.Z < 0) { screenPos = _errorPoint; return false; } // Object is behind the camera, return as failed

//            screenPos = new Point(
//                (int)(rotatedrelativePos.X / rotatedrelativePos.Z * _scaleX + _screenCenter.X),
//                (int)(rotatedrelativePos.Y / rotatedrelativePos.Z * _scaleY + _screenCenter.Y)
//            );

//            return true;
//        }

//        public Vector3 Rotate(Vector3 vector, Vector3 pivotPoint, Vector2 rotation)
//            => General.RotateVector(vector - pivotPoint, rotation.X, rotation.Y) + pivotPoint;
//        public void SetRotation(Vector3 pivot, Vector2 rotation)
//        {
//            foreach (var vertexIndex in Enumerable.Range(0, Vertices.Count()))
//            {
//                Vertices[vertexIndex] = Rotate(Vertices[vertexIndex], pivot, rotation - Rotation);
//            }
//            PivotPoint = Rotate(PivotPoint, pivot, rotation - Rotation);
//            Rotation = rotation;
//        }
//        public void ChangeRotation(Vector3 pivot, Vector2 rotation)
//        {
//            foreach (var vertexIndex in Enumerable.Range(0, Vertices.Count()))
//            {
//                Vertices[vertexIndex] = Rotate(Vertices[vertexIndex], pivot, rotation);
//            }
//            PivotPoint = Rotate(PivotPoint, pivot, rotation);
//            Rotation += rotation;
//        }

//    }
//}
