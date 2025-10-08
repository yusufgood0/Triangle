using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Triangle
{
    internal struct Cube : Model
    {
        BoundingBox Model.BoundingBox => new(Position, BRB);
        Shape[] Model.Shapes => CreateSquares();
        static (int, int, int)[] Triangles = new (int, int, int)[]
            {
                (0, 1, 2), (1, 3, 2), // Front
                (6, 5, 4), (6, 7, 5), // Back
                (4, 1, 0), (4, 5, 1), // Top
                (2, 3, 6), (3, 7, 6), // Bottom
                (0, 2, 4), (2, 6, 4), // Left
                (5, 3, 1), (5, 7, 3)  // Right
            };
        static (int a, int b, int c, int d)[] Squares = new (int, int, int, int)[]
            {
                (0, 1, 3, 2), // Front face (from triangles (0,1,2) and (1,3,2))
                (4, 5, 7, 6), // Back face (from triangles (4,5,6) and (5,7,6))
                (1, 0, 4, 5), // Top face (from triangles (0,1,4) and (1,5,4))
                (2, 3, 7, 6), // Bottom face (from triangles (2,3,6) and (3,7,6))
                (0, 2, 6, 4), // Left face (from triangles (0,2,4) and (2,6,4))
                (1, 3, 7, 5)  // Right face (from triangles (1,3,5) and (3,7,5))
            };
        /*
         * TLF, TRF, BLF, BRF,
         * TLB, TRB, BLB, BRB
         */
        public Shape[] _cachedSquares = null;
        float xSize;
        float ySize;
        float zSize;
        public Vector3 Position { get => TLF; set => TLF = value; }
        public Vector3 TLF;
        public Vector3 TRF => new Vector3(TLF.X + xSize, TLF.Y, TLF.Z);
        public Vector3 BLF => new Vector3(TLF.X, TLF.Y + ySize, TLF.Z);
        public Vector3 BRF => new Vector3(TLF.X + xSize, TLF.Y + ySize, TLF.Z);
        public Vector3 TLB => new Vector3(TLF.X, TLF.Y, TLF.Z + zSize);
        public Vector3 TRB => new Vector3(TLF.X + xSize, TLF.Y, TLF.Z + zSize);
        public Vector3 BLB => new Vector3(TLF.X, TLF.Y + ySize, TLF.Z + zSize);
        public Vector3 BRB => new Vector3(TLF.X + xSize, TLF.Y + ySize, TLF.Z + zSize);
        public Cube(Vector3 TLF, float xSize, float ySize, float zSize)
        {
            this.TLF = TLF;
            this.xSize = xSize;
            this.ySize = ySize;
            this.zSize = zSize;
        }
        public Shape[] GetTriangles
        {
            get => Triangle.ModelConstructor(Triangles, new Vector3[]
            {
                TLF, TRF, BLF, BRF,
                TLB, TRB, BLB, BRB
            });
        }
        public Vector3 Center
        {
            get => new Vector3(Position.X + xSize / 2, Position.Y + ySize / 2, Position.Z + zSize / 2);
        }
        public Vector3 Opposite
        {
            get => new Vector3(Position.X + xSize, Position.Y + ySize, Position.Z + zSize);
        }
        public Cube(Cube Cube)
        {
            TLF = Cube.TLF;
            xSize = Cube.xSize;
            ySize = Cube.ySize;
            zSize = Cube.zSize;
        }
        public Model Move(Vector3 offset)
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
                    color,
                    cameraPosition,
                    pitch,
                    yaw,
                    distance
                );
            }
        }
        public Shape[] CreateSquares()
        {

            if (_cachedSquares == null)
            {
                Vector3[] _vertices = new Vector3[]
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
                        _vertices[Squares[i].d]
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
