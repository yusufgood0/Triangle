using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeGame.Generation
{
    internal struct Chunk
    {
        public int XIndex { get; }
        public int YIndex { get; }
        public float[,] HeightMap { get; }

        public const int ChunkSize = 16;
        public Chunk(int xIndex, int yIndex, float[,] heightMap)
        {
            XIndex = xIndex;
            YIndex = yIndex;
            HeightMap = heightMap;
        }
        static public Chunk FromHeightMap(int xIndex, int yIndex, HeightMap heightMap, float ScalarValue, float heightScale)
        {
            float[,] chunkHeightMap = heightMap.getHeightChunk(xIndex * ChunkSize, yIndex * ChunkSize, ChunkSize, ChunkSize, ScalarValue, heightScale);
            return new Chunk(xIndex, yIndex, chunkHeightMap);
        }

    }
}
