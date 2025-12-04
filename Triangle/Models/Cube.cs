using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.GameAsset;
using SlimeGame.Models.Shapes;

namespace SlimeGame.Models
{
    internal struct Cube : GenericModel
    {
        BoundingBox GenericModel.BoundingBox => new(Position, BRB);
        Shape[] GenericModel.Shapes => CreateSquares();
        Color GenericModel.Color { get => _color; set => _color = value; }

        Vector3 GenericModel.Position { get => TLF; set => TLF = value; }
        VertexPositionColorNormal[] GenericModel.Vertices
        {
            get
            {
                VertexPositionColorNormal[] verts = new VertexPositionColorNormal[Vertices.Length];
                Vector3 center = Center;
                for (int i = 0; i < Vertices.Length; i++)
                {
                    verts[i] = new VertexPositionColorNormal(
                        Vertices[i],
                        _color,
                        Vector3.Normalize(Vertices[i] - center));
                }
                return verts;
            }
        }
        Vector3[] Vertices
        {
            get => 
                _rotation != Vector2.Zero 
                ?
                new Vector3[]
                {
                    Rotation(TLF), Rotation(TRF), Rotation(BLF), Rotation(BRF),
                    Rotation(TLB), Rotation(TRB), Rotation(BLB), Rotation(BRB)
                }
                :
                new Vector3[]
                {
                        TLF, TRF, BLF, BRF,
                        TLB, TRB, BLB, BRB
                };
        }

        int[] GenericModel.Indeces => Triangles;
        static int[] Triangles = new int[]
            {
                0, 1, 2, 1, 3, 2, // Front
                6, 5, 4, 6, 7, 5, // Back
                4, 1, 0, 4, 5, 1, // Top
                2, 3, 6, 3, 7, 6, // Bottom
                0, 2, 4, 2, 6, 4, // Left
                5, 3, 1, 5, 7, 3  // Right
            };
        static (int a, int b, int c, int d)[] Squares = new (int, int, int, int)[]
            {
                (2, 3, 1, 0), // Front face (from triangles (0,1,2) and (1,3,2))
                (4, 5, 7, 6), // Back face (from triangles (4,5,6) and (5,7,6))
                (5, 4, 0, 1), // Top face (from triangles (0,1,4) and (1,5,4))
                (6, 7, 3, 2), // Bottom face (from triangles (2,3,6) and (3,7,6))
                (4, 6, 2, 0), // Left face (from triangles (0,2,4) and (2,6,4))
                (1, 3, 7, 5)  // Right face (from triangles (1,3,5) and (3,7,5))
            };
        /*
         * TLF, TRF, BLF, BRF,
         * TLB, TRB, BLB, BRB
         */
        public Shape[] _cachedSquares = null;
        float _xSize;
        float _ySize;
        float _zSize;
        Color _color;
        Vector2 _rotation;
        Vector3 _pivotPoint;

        public Vector3 TLF;
        public Vector3 Position { get => TLF; set => TLF = value; }
        public Vector3 TRF => new Vector3(TLF.X + _xSize, TLF.Y, TLF.Z);
        public Vector3 BLF => new Vector3(TLF.X, TLF.Y + _ySize, TLF.Z);
        public Vector3 BRF => new Vector3(TLF.X + _xSize, TLF.Y + _ySize, TLF.Z);
        public Vector3 TLB => new Vector3(TLF.X, TLF.Y, TLF.Z + _zSize);
        public Vector3 TRB => new Vector3(TLF.X + _xSize, TLF.Y, TLF.Z + _zSize);
        public Vector3 BLB => new Vector3(TLF.X, TLF.Y + _ySize, TLF.Z + _zSize);
        public Vector3 BRB => new Vector3(TLF.X + _xSize, TLF.Y + _ySize, TLF.Z + _zSize);
        public Vector3 Rotation(Vector3 vector)
            => General.RotateVector(vector - _pivotPoint, _rotation.X, _rotation.Y) + _pivotPoint;
        public Cube(Vector3 TLF, float xSize, float ySize, float zSize, Color color = new(), Vector2 rotation = new Vector2())
        {
            this.Position = TLF;
            this._xSize = xSize;
            this._ySize = ySize;
            this._zSize = zSize;
            this._color = color;
            this._rotation = rotation == new Vector2() ? Vector2.Zero : rotation;
        }
        public Shape[] GetTriangles
        {
            get => Triangle.ModelConstructor(Triangles, new Vector3[]
                {
                    TLF, TRF, BLF, BRF,
                    TLB, TRB, BLB, BRB
                },
                _color);
        }
        public Vector3 Center
        {
            get => new Vector3(Position.X + _xSize / 2, Position.Y + _ySize / 2, Position.Z + _zSize / 2);
        }
        public Vector3 Opposite
        {
            get => new Vector3(Position.X + _xSize, Position.Y + _ySize, Position.Z + _zSize);
        }
        public Cube(Cube Cube)
        {
            TLF = Cube.TLF;
            _xSize = Cube._xSize;
            _ySize = Cube._ySize;
            _zSize = Cube._zSize;
            _color = Cube._color;
            _rotation = Cube._rotation;
        }
        public GenericModel Move(Vector3 offset)
        {
            Cube cube = new(this);
            cube.TLF += offset; // other 7 points are dependant on the top-left-frontmost point
            cube._cachedSquares = null; // old squares become invalid
            return cube;
        }
        public void DrawAsWhole(
            ref TextureBuffer screenBuffer,
            Color color,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            int distance
            )
        {
            foreach (Square square in CreateSquares())
            {
                // Draw each triangle in the square
                square.Draw(
                    ref screenBuffer,
                    cameraPosition,
                    pitch,
                    yaw,
                    distance,
                    _color
                );
            }
        }
        public void SetRotation(Vector3 pivot, Vector2 rotation)
        {
            _rotation = rotation;
            _pivotPoint = pivot;
            RecreateSquares();
        }
        public void ChangeRotation(Vector3 pivot, Vector2 rotation)
        {
            _rotation = _rotation + rotation;
            _pivotPoint = pivot;
            RecreateSquares();
        }
        public void RecreateSquares()
        {
            DiscardSquares();
            CreateSquares();
        }
        public Shape[] CreateSquares()
        {
            if (_cachedSquares == null)
            {
                Vector3[] _vertices;
                if (_rotation != Vector2.Zero)
                    _vertices = new Vector3[]
                    {
                        Rotation(TLF), Rotation(TRF), Rotation(BLF), Rotation(BRF),
                        Rotation(TLB), Rotation(TRB), Rotation(BLB), Rotation(BRB)
                    };
                else
                    _vertices = new Vector3[]
                    {
                        TLF, TRF, BLF, BRF,
                        TLB, TRB, BLB, BRB
                    };
                _cachedSquares = new Shape[6];
                for (int i = 0; i < 6; i++)
                {
                    _cachedSquares[i] = new Square(
                        _vertices[Squares[i].a],
                        _vertices[Squares[i].b],
                        _vertices[Squares[i].c],
                        _vertices[Squares[i].d],
                        _color
                    );
                }
            }
            return _cachedSquares;
        }
        public void DiscardSquares()
        {
            _cachedSquares = null;
        }
    }
}
