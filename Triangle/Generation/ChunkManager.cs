using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Generation
{
    internal class ChunkManager
    {
        public Chunk[,] _loadedChunks;
        private object _loadedChunksAsyncLocked = false;
        HeightMap _heightMap;
        (int XIndex, int YIndex) CurrentPlayerChunkIndex = (int.MaxValue, int.MaxValue);
        int _renderDistance;
        public float NoiseScale { get; }
        public float TileSize { get; }
        public float HeightScale;
        public float seaLevel = -100;
        public VertexPositionColorNormal[,] _MapVertexPositionColorNormals;
        private int _StartVerticesX = 0;
        private int _StartVerticesY = 0;
        private int _startX = 0;
        private int _startY = 0;

        public ChunkManager(int seed, Vector3 position, int renderDistance, float scalarValue, float tileSize, float heightScale)
        {
            _heightMap = new HeightMap(seed);
            NoiseScale = scalarValue;
            TileSize = tileSize;
            HeightScale = heightScale;
            _renderDistance = renderDistance;

            ForceLoadedChunks(position);

            int width = _loadedChunks.GetLength(0) * Chunk.ChunkSize;
            int height = _loadedChunks.GetLength(1) * Chunk.ChunkSize;
            _MapVertexPositionColorNormals = new VertexPositionColorNormal[width, height];
            for (int x = 0; x < _MapVertexPositionColorNormals.GetLength(0); x++)
            {
                for (int y = 0; y < _MapVertexPositionColorNormals.GetLength(1); y++)
                {
                    _MapVertexPositionColorNormals[x, y] = default;
                }
            }

        }
        public Vector3 GetNormal(Vector3 position)
        {
            var p1 = new Vector3(position.X, HeightAtPosition(position + new Vector3(0, 0, 1f)), position.Z) + new Vector3(0, 0, 1f);
            var p2 = new Vector3(position.X, HeightAtPosition(position + new Vector3(1f, 0, 0)), position.Z) + new Vector3(1f, 0, 0);
            var p3 = new Vector3(position.X, HeightAtPosition(position + new Vector3(1f, 0, 1f)), position.Z) + new Vector3(1f, 0, 1f);

            Vector3 side2 = p1 - p2;
            Vector3 side3 = p1 - p3;

            Vector3 normalDir = Vector3.Cross(side3, side2);
            normalDir.Normalize();
            return normalDir;
        }
        public bool IsPointAboveTerrain(Vector3 pos) => pos.Y > HeightAtPosition(pos);
        //public float AccurateHeightAtPosition(Vector3 worldPos)
        //{
        //    return _heightMap.GetHeight(worldPos.X / TileSize, worldPos.Y / TileSize, NoiseScale, HeightScale);
        //}
        public float HeightAtPosition(Vector3 worldPos)
        {

            var chunkIndex = ChunkIndex(worldPos);

            int localChunkX = chunkIndex.XIndex - _startX;
            int localChunkY = chunkIndex.YIndex - _startY;

            if (localChunkX < 0 || localChunkX >= _loadedChunks.GetLength(0) ||
                localChunkY < 0 || localChunkY >= _loadedChunks.GetLength(1))
                return 0;

            Chunk chunk = _loadedChunks[localChunkX, localChunkY];

            float chunkWorldSize = Chunk.ChunkSize * TileSize;

            int tileX = (int)MathF.Floor(
                (worldPos.X - chunkIndex.XIndex * chunkWorldSize) / TileSize
            );
            int tileY = (int)MathF.Floor(
                (worldPos.Z - chunkIndex.YIndex * chunkWorldSize) / TileSize
            );
            if (chunk.HeightMap == null || tileX >= Chunk.ChunkSize || tileY >= Chunk.ChunkSize)
            {
                return worldPos.Y;
            }
            return chunk.HeightMap[tileX, tileY];
            //return _heightMap.GetHeight(worldPos.X / TileSize, worldPos.Z / TileSize, NoiseScale, HeightScale);
        }

        public Task<VertexPositionColorNormal[,]> GetVerticesAsync(
            Color[] colors,
            Vector3 playerPos)
        {
            return Task.Run(() =>
            {
                lock (_loadedChunksAsyncLocked)
                {
                    int width = _MapVertexPositionColorNormals.GetLength(0);
                    int height = _MapVertexPositionColorNormals.GetLength(1);
                    var Heights = MergeLoadedChunksHeightMap(playerPos);

                    var newMapVertexPositionColorNormal = new VertexPositionColorNormal[width, height];

                    int shiftX = (_startX - _StartVerticesX) * Chunk.ChunkSize;
                    int shiftY = (_startY - _StartVerticesY) * Chunk.ChunkSize;

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            int originX = x + shiftX;
                            int originY = y + shiftY;
                            if (originX >= 0 && originX < width && originY >= 0 && originY < height)
                            {
                                newMapVertexPositionColorNormal[x, y] = _MapVertexPositionColorNormals[originX, originY];
                            }
                            else
                            {
                                newMapVertexPositionColorNormal[x, y] = default;
                            }
                        }
                    }

                    var currentChunkIndex = ChunkIndex(playerPos);

                    int startChunkX = _loadedChunks[0, 0].XIndex;
                    int startChunkY = _loadedChunks[0, 0].YIndex;

                    Vector2 worldOffset = new Vector2(
                            startChunkX * Chunk.ChunkSize * TileSize,
                            startChunkY * Chunk.ChunkSize * TileSize
                        );

                    //assigns positions and colors
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (newMapVertexPositionColorNormal[x, y] != default) continue;

                            newMapVertexPositionColorNormal[x, y].Position = new Vector3(
                                worldOffset.X + x * TileSize,
                                Heights[x, y],
                                worldOffset.Y + y * TileSize
                                );

                            Color color = colors[Heights[x, y] > seaLevel ? 1 : 0];
                            //color = Color.Lerp(
                            //    Color.Yellow,
                            //    Color.DarkGreen,
                            //    _heightMap.GetNormalizedHeight(
                            //        newMapVertexPositionColorNormal[x, y].Position.X * 2,
                            //        newMapVertexPositionColorNormal[x, y].Position.Z * 2,
                            //        NoiseScale / 300));
                            color.A = 255;
                            newMapVertexPositionColorNormal[x, y].Color = color;
                        }
                    }
                    //assigns normals
                    for (int y = 0; y < height - 1; y++)
                    {
                        for (int x = 0; x < width - 1; x++)
                        {
                            int index = y * width + x;
                            if (newMapVertexPositionColorNormal[x, y].Normal != Vector3.Zero) continue;

                            // finds the four points of the quad
                            Vector3 p1 = newMapVertexPositionColorNormal[x, y].Position;
                            Vector3 p2 = newMapVertexPositionColorNormal[x + 1, y].Position;
                            Vector3 p3 = newMapVertexPositionColorNormal[x + 1, y + 1].Position;
                            Vector3 p4 = newMapVertexPositionColorNormal[x, y + 1].Position;

                            // finds normal direction
                            Vector3 side1 = p2 - p1;
                            Vector3 side2 = p3 - p1;
                            Vector3 side3 = p4 - p1;

                            Vector3 normalDir1 = Vector3.Cross(side1, side2);
                            normalDir1.Normalize();

                            Vector3 normalDir2 = Vector3.Cross(side2, side3);
                            normalDir2.Normalize();

                            Vector3 normalDir = (normalDir1 + normalDir2) / 2;
                            if (normalDir.Y > 0)
                            {
                                normalDir = -normalDir;
                            }
                            // assigns normals to each vertex
                            newMapVertexPositionColorNormal[x, y].Normal = normalDir;
                        }
                    }
                    //// Fix 
                    ///edge (x = width - 1), copy from x - 1
                    //for (int y = 1; y < height; y++)
                    //{
                    //    int x = width - 1;
                    //    newMapVertexPositionColorNormal[x, y].Normal =
                    //        newMapVertexPositionColorNormal[x - 1, y].Normal;
                    //}

                    //// Fix bottom edge (y = height - 1), copy from y - 1
                    //for (int x = 0; x < width; x++)
                    //{
                    //    int y = height - 1;
                    //    newMapVertexPositionColorNormal[x, y].Normal =
                    //        newMapVertexPositionColorNormal[x, y - 1].Normal;
                    //}
                    _StartVerticesX = _startX;
                    _StartVerticesY = _startY;
                    return newMapVertexPositionColorNormal;
                }

            });
        }
        public (int, int) MergeLoadedChunksHeightMapDimensions =>
            (_loadedChunks.GetLength(0) * Chunk.ChunkSize, _loadedChunks.GetLength(1) * Chunk.ChunkSize);
        public float[,] MergeLoadedChunksHeightMap(Vector3 position)
        {
            (int totalSizeX, int totalSizeY) = MergeLoadedChunksHeightMapDimensions;


            float[,] mergedHeightMap = new float[totalSizeX, totalSizeY];

            for (int chunkX = 0; chunkX < _loadedChunks.GetLength(0); chunkX++)
            {
                for (int chunkY = 0; chunkY < _loadedChunks.GetLength(1); chunkY++)
                {
                    Chunk chunk = _loadedChunks[chunkX, chunkY];
                    for (int x = 0; x < Chunk.ChunkSize; x++)
                    {
                        for (int y = 0; y < Chunk.ChunkSize; y++)
                        {
                            float heightValue = chunk.HeightMap[x, y];
                            mergedHeightMap[chunkX * Chunk.ChunkSize + x, chunkY * Chunk.ChunkSize + y] = heightValue;
                        }
                    }
                }
            }
            return mergedHeightMap;
        }
        public bool TryTranslateAndGetNewLoadedChunks(Vector3 playerPosition)
        {
            int sqrtNumChunks = _renderDistance * 2 + 1;

            var CurrentChunkIndex = ChunkIndex(playerPosition);
            var startX = CurrentChunkIndex.XIndex - _renderDistance;
            var startY = CurrentChunkIndex.YIndex - _renderDistance;

            if (CurrentPlayerChunkIndex == CurrentChunkIndex) return false;
            lock (_loadedChunksAsyncLocked)
            {
                //_loadedChunks = new Chunk[sqrtNumChunks, sqrtNumChunks];
                TranslateAndGetNewLoadedChunks(sqrtNumChunks, startX, startY);
            }
            CurrentPlayerChunkIndex = CurrentChunkIndex;
            return true;
        }
        void TranslateAndGetNewLoadedChunks(int sqrtNumChunks, int startX, int startY)
        {
            var newChunks = new Chunk[_loadedChunks.GetLength(0), _loadedChunks.GetLength(1)];

            int shiftX = startX - _startX;
            int shiftY = startY - _startY;

            for (int x = 0; x < sqrtNumChunks; x++)
            {
                for (int y = 0; y < sqrtNumChunks; y++)
                {
                    int originX = x + shiftX;
                    int originY = y + shiftY;
                    if (originX >= 0 && originX < _loadedChunks.GetLength(0) && originY >= 0 && originY < _loadedChunks.GetLength(1))
                    {
                        newChunks[x, y] = _loadedChunks[originX, originY];
                    }
                    else
                    {
                        newChunks[x, y] = Chunk.FromHeightMap(startX + x, startY + y, _heightMap, NoiseScale, HeightScale);
                    }
                }
            }
            _startX = startX;
            _startY = startY;
            _loadedChunks = newChunks;
        }
        bool ForceLoadedChunks(Vector3 playerPosition)
        {
            int sqrtNumChunks = _renderDistance * 2 + 1;

            var CurrentChunkIndex = ChunkIndex(playerPosition);
            var startX = CurrentChunkIndex.XIndex - _renderDistance;
            var startY = CurrentChunkIndex.YIndex - _renderDistance;

            lock (_loadedChunksAsyncLocked)
            {
                _loadedChunks = new Chunk[sqrtNumChunks, sqrtNumChunks];
                ForceLoadedChunks(sqrtNumChunks, startX, startY);
            }
            CurrentPlayerChunkIndex = CurrentChunkIndex;
            return true;
        }
        void ForceLoadedChunks(int sqrtNumChunks, int startX, int startY)
        {
            for (int x = 0; x < sqrtNumChunks; x++)
            {
                for (int y = 0; y < sqrtNumChunks; y++)
                {
                    _loadedChunks[x, y] = Chunk.FromHeightMap(startX + x, startY + y, _heightMap, NoiseScale, HeightScale);
                }
            }
            _startX = startX;
            _startY = startY;
        }

        public void OffsetHeightAtPosition(Vector3 worldPos, float offset, Color? color = null)
        {
            var chunkIndex = ChunkIndex(worldPos);
            float chunkWorldSize = Chunk.ChunkSize * TileSize;

            int tileX = (int)MathF.Floor(
                (worldPos.X - chunkIndex.XIndex * chunkWorldSize) / TileSize
            );
            int tileY = (int)MathF.Floor(
                (worldPos.Z - chunkIndex.YIndex * chunkWorldSize) / TileSize
            );
            int localChunkX = chunkIndex.XIndex - _startX;
            int localChunkY = chunkIndex.YIndex - _startY;

            if (localChunkX < 0 || localChunkX >= _loadedChunks.GetLength(0) ||
                localChunkY < 0 || localChunkY >= _loadedChunks.GetLength(1))
                return;

            Chunk chunk = _loadedChunks[localChunkX, localChunkY];



            if (tileX < 0 || tileX >= Chunk.ChunkSize ||
                tileY < 0 || tileY >= Chunk.ChunkSize)
                return;

            int worldX = localChunkX * Chunk.ChunkSize + tileX;
            int worldY = localChunkY * Chunk.ChunkSize + tileY;
            ref var vertex = ref _MapVertexPositionColorNormals[worldX, worldY];

            chunk.HeightMap[tileX, tileY] += offset;
            vertex.Position.Y += offset;
            if (color != null)
            {
                vertex.Color = (Color)color;
            }
        }
        public (int x, int y) GetTileWorldIndex(Vector3 worldPos)
        {
            var chunkIndex = ChunkIndex(worldPos);
            float chunkWorldSize = Chunk.ChunkSize * TileSize;

            int tileX = (int)MathF.Floor(
                (worldPos.X - chunkIndex.XIndex * chunkWorldSize) / TileSize
            );
            int tileY = (int)MathF.Floor(
                (worldPos.Z - chunkIndex.YIndex * chunkWorldSize) / TileSize
            );
            int localChunkX = chunkIndex.XIndex - _startX;
            int localChunkY = chunkIndex.YIndex - _startY;
            int x = localChunkX * Chunk.ChunkSize + tileX;
            int y = localChunkY * Chunk.ChunkSize + tileY;
            return (x, y);
        }
        public void OffsetHeightAtIndex((int x, int y) TileWorldIndex, float offset, Color? color = null, float lerpValue = 0)
        {
            (int x, int y) = TileWorldIndex;
            int tileX, tileY;
            int localChunkX, localChunkY;
            tileX = x % Chunk.ChunkSize;
            tileY = y % Chunk.ChunkSize;
            localChunkX = (x - tileX) / Chunk.ChunkSize;
            localChunkY = (y - tileY) / Chunk.ChunkSize;

            if (localChunkX < 0 || localChunkX >= _loadedChunks.GetLength(0) ||
                localChunkY < 0 || localChunkY >= _loadedChunks.GetLength(1))
                return;

            Chunk chunk = _loadedChunks[localChunkX, localChunkY];

            float chunkWorldSize = Chunk.ChunkSize * TileSize;


            if (tileX < 0 || tileX >= Chunk.ChunkSize ||
                tileY < 0 || tileY >= Chunk.ChunkSize)
                return;

            int worldX = localChunkX * Chunk.ChunkSize + tileX;
            int worldY = localChunkY * Chunk.ChunkSize + tileY;
            ref var vertex = ref _MapVertexPositionColorNormals[worldX, worldY];

            chunk.HeightMap[tileX, tileY] += offset;
            vertex.Position.Y += offset;
            if (color != null)
            {
                vertex.Color = Color.Lerp(vertex.Color, (Color)color, lerpValue);
            }
        }
        public void CreateCrater(Vector3 pos, int Size, float strength, Color? color = null)
        {
            (int centerX, int centerY) = GetMergedHeightMapIndex(pos);
            for (int x = -Size; x <= Size; x++)
            {
                for (int y = -Size; y <= Size; y++)
                {
                    float z = (-MathF.Sqrt(x * x + y * y) + (float)Size) / (float)Size;
                    //float z = (MathF.Sqrt(Size * Size - (x * x + y * y)));
                    if (z < 0) continue;
                    OffsetHeightAtIndex((x + centerX, y + centerY), z * strength, color ?? Color.Black, z);
                }
            }
        }
        public void CreateCrater(Vector3 pos, int Radius, Color? color = null)
        {
            (int centerX, int centerY) = GetMergedHeightMapIndex(pos);
            for (int x = -Radius; x <= Radius; x++)
            {
                for (int y = -Radius; y <= Radius; y++)
                {
                    //float z = (-MathF.Sqrt(x * x + y * y) + (float)Size) / (float)Size;
                    float z = (MathF.Sqrt((float)Radius * (float)Radius - ((float)x * (float)x + (float)y * (float)y)));
                    //if (z < 0) continue;
                    OffsetHeightAtIndex((x + centerX, y + centerY), z, color ?? Color.Black, z);
                }
            }
        }
        public (int mergedX, int mergedY) GetMergedHeightMapIndex(Vector3 worldPos)
        {
            // Determine which chunk the position is in
            var chunkIndex = ChunkIndex(worldPos);

            int localChunkX = chunkIndex.XIndex - _startX;
            int localChunkY = chunkIndex.YIndex - _startY;

            int width = _loadedChunks.GetLength(0);
            int height = _loadedChunks.GetLength(1);
            if (localChunkX < 0 || localChunkX >= width ||
                localChunkY < 0 || localChunkY >= height)
            {
                return (width / 2, height / 2); // returns the center in case of error
            }

            // Determine the tile index inside that chunk
            float chunkWorldSize = Chunk.ChunkSize * TileSize;

            int tileX = (int)MathF.Floor((worldPos.X - (chunkIndex.XIndex * chunkWorldSize)) / TileSize);
            int tileY = (int)MathF.Floor((worldPos.Z - (chunkIndex.YIndex * chunkWorldSize)) / TileSize);

            // Convert to merged heightmap index
            int mergedX = localChunkX * Chunk.ChunkSize + tileX;
            int mergedY = localChunkY * Chunk.ChunkSize + tileY;

            return (mergedX, mergedY);
        }
        public (int mergedX, int mergedY) GetMergedIndicesFromWorldPos(Vector3 worldPos)
        {
            // First, determine which chunk the position is in
            var chunkIndex = ChunkIndex(worldPos);

            int localChunkX = chunkIndex.XIndex - _startX;
            int localChunkY = chunkIndex.YIndex - _startY;

            // Bounds check
            if (localChunkX < 0 || localChunkX >= _loadedChunks.GetLength(0) ||
                localChunkY < 0 || localChunkY >= _loadedChunks.GetLength(1))
                throw new Exception("World position is outside loaded chunks");

            // Size of one chunk in world units
            float chunkWorldSize = Chunk.ChunkSize * TileSize;

            // Determine the tile indices inside that chunk
            int tileX = (int)MathF.Floor((worldPos.X - chunkIndex.XIndex * chunkWorldSize) / TileSize);
            int tileY = (int)MathF.Floor((worldPos.Z - chunkIndex.YIndex * chunkWorldSize) / TileSize);

            // Convert to merged heightmap coordinates
            int mergedX = localChunkX * Chunk.ChunkSize + tileX;
            int mergedY = localChunkY * Chunk.ChunkSize + tileY;

            return (mergedX, mergedY);
        }
        public (int XIndex, int YIndex) ChunkIndex(Vector3 position)
        {
            float chunkWorldSize = Chunk.ChunkSize * TileSize;

            return (
                (int)MathF.Floor(position.X / chunkWorldSize),
                (int)MathF.Floor(position.Z / chunkWorldSize)
            );
        }
    }
}
