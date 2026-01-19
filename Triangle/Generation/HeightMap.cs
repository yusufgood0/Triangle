using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Generation
{
    internal class HeightMap
    {
        static FastNoiseLite FastNoiseLite;
        static float amplitude = 0.8f;
        public HeightMap(int seed)
        {
            FastNoiseLite = new FastNoiseLite(seed);
            FastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            FastNoiseLite.SetFrequency(0.01f);
        }
        public float GetHeight(float x, float y, float noiseScale, float heightScale)
            => -GetNormalizedHeight(x, y, noiseScale) * heightScale;
        public float GetNormalizedHeight(float x, float y, float noiseScale)
            => ((FastNoiseLite.GetNoise(x * noiseScale, y * noiseScale) * FastNoiseLite.GetNoise(x * noiseScale* amplitude, y * noiseScale * amplitude)) + 1) / 2;
        public float[,] getHeightChunk(int startX, int startY, int width, int Height, float noiseScale, float heightScale)
        {
            float[,] heightMap = new float[width, Height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    heightMap[x, y] = GetHeight(startX + x, startY + y, noiseScale, heightScale);
                }
            }
            return heightMap;
        }

    }
}
