//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using SlimeGame.GameAsset;
//using SlimeGame.Models.Shapes;


//namespace SlimeGame.Models
//{

//    internal struct MeshModel(
//            Vector3 OriginPosition,
//            Vector3[] Vertices,
//            Color[] Colors,
//            (int p1, int p2, int p3)[]? TriangleIndeces = null,
//            (int p1, int p2, int p3, int p4)[]? SquareIndices = null
//            ) : GenericModel
//    {
//        Shape[] GenericModel.Shapes => GetShapes(); // Unrecommended: called multiple times per frame
//        Vector3 GenericModel.Position { get => OriginPosition; set => OriginPosition = value; }
//        Color GenericModel.Color { get => Colors[0]; set => Colors[0] = value; }
//        BoundingBox GenericModel.BoundingBox => GetBoundingBox();

//        //(int p1, int p2, int p3, int p4)[]? SquareIndices;
//        //(int p1, int p2, int p3)[]? TriangleIndeces;
//        //Vector3[] Vertices;
//        //Color[] Colors;
//        //Vector3 OriginPosition;

//        //public MeshModel
//        //    (
//        //    Vector3 OriginPosition,
//        //    Vector3[] Vertices, 
//        //    Color[] Colors, 
//        //    (int p1, int p2, int p3)[]? TriangleIndeces = null, 
//        //    (int p1, int p2, int p3, int p4)[]? SquareIndices = null
//        //    )
//        //{
//        //    this.OriginPosition = OriginPosition;
//        //    this.Colors = Colors;
//        //    this.SquareIndices = SquareIndices;
//        //    this.TriangleIndeces = TriangleIndeces;
//        //    this.Vertices = Vertices;
//        //}

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

//        private Shape[] GetShapes()
//        {
//            List<Shape> shapes = new List<Shape>();
//            if (SquareIndices != null)
//            {
//                for (int i = 0; i < SquareIndices.Length; i++)
//                {
//                    shapes.Add(new Square(
//                        Vertices[SquareIndices[i].p1],
//                        Vertices[SquareIndices[i].p2],
//                        Vertices[SquareIndices[i].p3],
//                        Vertices[SquareIndices[i].p4],
//                        Colors[i]
//                        ));
//                }
//            }
//            if (TriangleIndeces != null)
//            {
//                for (int i = 0; i < TriangleIndeces.Length; i++)
//                {
//                    shapes.Add(new Triangle(
//                        Vertices[TriangleIndeces[i].p1],
//                        Vertices[TriangleIndeces[i].p2],
//                        Vertices[TriangleIndeces[i].p3],
//                        Colors[i]
//                        ));
//                }
//            }
//            return shapes.ToArray();
//        }
//        private BoundingBox GetBoundingBox()
//        {
//            Vector3 min = new Vector3(float.MaxValue);
//            Vector3 max = new Vector3(float.MinValue);
//            if (SquareIndices != null)
//            {
//                for (int i = 0; i < SquareIndices.Length; i++)
//                {
//                    min = Min(min, Vertices[SquareIndices[i].p1]);
//                    min = Min(min, Vertices[SquareIndices[i].p2]);
//                    min = Min(min, Vertices[SquareIndices[i].p3]);
//                    min = Min(min, Vertices[SquareIndices[i].p4]);
//                }
//            }
//            if (TriangleIndeces != null)
//            {
//                for (int i = 0; i < TriangleIndeces.Length; i++)
//                {
//                    min = Min(min, Vertices[TriangleIndeces[i].p1]);
//                    min = Min(min, Vertices[TriangleIndeces[i].p2]);
//                    min = Min(min, Vertices[TriangleIndeces[i].p3]);
//                }
//            }
//            if (min == new Vector3(float.MaxValue)) throw new Exception("MeshModel has no vertices to calculate BoundingBox.");
//            return new BoundingBox(min, max);
//        }
//        private Vector3 Min(Vector3 a, Vector3 b)
//        {
//            return new Vector3(
//                MathF.Min(a.X, b.X),
//                MathF.Min(a.Y, b.Y),
//                MathF.Min(a.Z, b.Z)
//                );
//        }
//        private Vector3 Max(Vector3 a, Vector3 b)
//        {
//            return new Vector3(
//                MathF.Max(a.X, b.X),
//                MathF.Max(a.Y, b.Y),
//                MathF.Max(a.Z, b.Z)
//                );
//        }
//        public void DrawVertices(
//            ref TextureBuffer screenBuffer,
//            Vector3 cameraPosition,
//            float pitch,
//            float yaw,
//            int PointSize = 2
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
//            for (int i = 0; i < Vertices.Length; i++)
//            {
//                var Vertex = Vertices[i];
//                var point = Points[i];
//                if (point == _errorPoint) continue;
//                int pointSize = (int)(PointSize / (0.1f*Vector3.Distance(Vertex, cameraPosition)));
//                DrawSquare(
//                    ref screenBuffer,
//                    Color.White,
//                    cameraPosition,
//                    pitch,
//                    yaw,
//                    (int)Vector3.Distance(cameraPosition, Vertex),
//                    (0, 1, 2, 3),
//                    new Point[] { 
//                        new Point(point.X - pointSize, point.Y - pointSize),
//                        new Point(point.X + pointSize, point.Y - pointSize),
//                        new Point(point.X + pointSize, point.Y + pointSize),
//                        new Point(point.X - pointSize, point.Y + pointSize)
//                    }
//                    );
//            }
//        }
//        public void Draw(
//            ref TextureBuffer screenBuffer,
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

//                    Color color;
//                    if (lightIntensity != 0)
//                    {
//                        // Compute normalized light direction (from point toward light)
//                        Vector3 lightDirection = Vector3.Normalize(lightPos - p1);

//                        // Compute triangle sides
//                        Vector3 side1 = p2 - p1;
//                        Vector3 side2 = p3 - p1;
//                        Vector3 side3 = p4 - p1;

//                        // Compute two normals (e.g. for adjacent triangles)
//                        Vector3 normalDir1 = Vector3.Normalize(Vector3.Cross(side1, side2));
//                        Vector3 normalDir2 = Vector3.Normalize(Vector3.Cross(side2, side3));

//                        // Average the normals
//                        Vector3 normalDir = (normalDir1 + normalDir2) * 0.5f;

//                        // Calculate how much the surface faces the light (clamped so no negative lighting)
//                        float dotProduct = MathF.Max(Vector3.Dot(normalDir, lightDirection), 0f);

//                        // Mix (Lerp) colors based on light intensity
//                        color = Color.Lerp(Colors[i], lightColor, dotProduct * lightIntensity + 0.25f);
//                    }
//                    else
//                    {
//                        color = Colors[i];
//                    }
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

//                    Color color;
//                    if (lightIntensity != 0)
//                    {
//                        Vector3 lightDirection = p1 - lightPos;
//                        lightDirection.Normalize();

//                        Vector3 side1 = p1 - p2;
//                        Vector3 side2 = p1 - p3;
//                        Vector3 normalDir = Vector3.Cross(side1, side2);
//                        normalDir.Normalize();

//                        // Calculate the difference in rays between the light direction and the normal vector using Vector3.Dot
//                        float dotProduct = Vector3.Dot(-normalDir, lightDirection);
//                        // mix colors based on the difference in rays
//                        color = Color.Lerp(Colors[i], lightColor, dotProduct / 3);
//                    }
//                    else
//                    {
//                        color = Colors[i];
//                    }
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

//            fixed (Color* screenBufferColorPtr = screenBuffer.Pixels)
//            fixed (int* screenBufferDistancePtr = screenBuffer.Distance)
//                for (int y = ymin; y < ymax; y++)
//                {
//                    int yTimesWidth = y * _CachedscreenSize.X;
//                    for (int x = xmin; x < xmax; x++)
//                    {
//                        int index = yTimesWidth + x;
//                        if (distance > screenBufferDistancePtr[index]) continue;
//                        if (!IsPointInQuad(x, y, p1, p2, p3, p4)) continue;

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

//        public static bool IsPointInQuad(int x, int y, Point p1, Point p2, Point p3, Point p4)
//        {
//            int c1 = (p2.X - p1.X) * (y - p1.Y) - (p2.Y - p1.Y) * (x - p1.X);
//            int c2 = (p3.X - p2.X) * (y - p2.Y) - (p3.Y - p2.Y) * (x - p2.X);
//            int c3 = (p4.X - p3.X) * (y - p3.Y) - (p4.Y - p3.Y) * (x - p3.X);
//            int c4 = (p1.X - p4.X) * (y - p4.Y) - (p1.Y - p4.Y) * (x - p4.X);

//            bool hasNeg = (c1 < 0) || (c2 < 0) || (c3 < 0) || (c4 < 0);
//            bool hasPos = (c1 > 0) || (c2 > 0) || (c3 > 0) || (c4 > 0);

//            return !(hasNeg && hasPos);
//        }
//    }
//}
