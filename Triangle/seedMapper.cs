using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace random_generation_in_a_pixel_grid
{
    internal class SeedMapper
    {
        public int[,] Values { get; set; }
        public int[,] Heights { get; set; }
        WeightedRandom random;
        public int height { get; }
        public int width { get; }
        public SeedMapper(int width, int height, int[] weights, int maxHeight, int? seed = null)
        {
            this.height = height;
            this.width = width;

            Random pureRandom;
            if (seed != null)
            {
                this.random = new WeightedRandom(weights, seed);
                pureRandom = new Random((int)seed);

            }
            else
            {
                this.random = new WeightedRandom(weights);
                pureRandom = new Random();
            }
            Values = new int[width, height];
            Heights = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Values[x, y] = random.Next();
                    Heights[x, y] = -pureRandom.Next(maxHeight);
                }
            }


        }
        public unsafe void draw(SpriteBatch spritebatch, Point Position, int pixelWidth, int pixelHeight, Texture2D texture, Color[] colors)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color = colors[Values[x, y]];
                    color = Color.Lerp(color, Color.White, Heights[x, y] / 50f);
                    spritebatch.Begin();
                    spritebatch.Draw(
                        texture,
                        new Rectangle(Position.X + pixelWidth * x, Position.Y + pixelHeight * y, pixelWidth, pixelHeight),
                        color
                        );
                    spritebatch.End();
                }
            }
        }
        public Vector3 GetNormal(int x, int y)
        {
            int xPlus1 = x + 1;
            int yPlus1 = y + 1;
            if (x <= 0 || y <= 0 || x >= width-2 || y >= height-2)
            {
                return Vector3.Down;
            }

            var p1 = new Vector3(x, Heights[x, y], y);
            var p2 = new Vector3(xPlus1, Heights[xPlus1, y], y);
            var p3 = new Vector3(x, Heights[x, yPlus1], yPlus1);
            var p4 = new Vector3(xPlus1, Heights[xPlus1, yPlus1], yPlus1);

            Vector3 side2 = p1 - p2;
            Vector3 side3 = p1 - p3;
            Vector3 side4 = p1 - p4;
            Vector3 normalDir1 = Vector3.Cross(side3, side4);
            normalDir1.Normalize();

            Vector3 normalDir2 = Vector3.Cross(side3, side2);
            normalDir2.Normalize();

            Vector3 normalDir = (normalDir1 + normalDir2) / 2;
            return normalDir;

        }
        private int CountLandNeighbhors(int x, int y)
        {
            int count = 0;
            int xChecking;
            int yChecking;

            int xMin = Math.Min(x - 1, 0);
            int yMin = Math.Min(y - 1, 0);
            int xMax = Math.Min(x + 1, width);
            int yMax = Math.Min(y + 1, height);

            for (int xoffset = -1; xoffset < 2; xoffset++)
            {
                for (int yoffset = -1; yoffset < 2; yoffset++)
                {
                    xChecking = x + xoffset;
                    yChecking = y + yoffset;
                    if (yChecking > 0 && xChecking > 0 && yChecking < height && xChecking < width && Values[xChecking, yChecking] != 0)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        public (int x, int y) CubicBezier(float t,
            float x0, float x1, float x2, float x3,
            float y0, float y1, float y2, float y3
            )
        {
            double oneMinusT = 1 - t;

            int x = (int)(oneMinusT * (oneMinusT * (oneMinusT * x0 + t * x1)
                                    + t * (oneMinusT * x1 + t * x2))
                     + t * (oneMinusT * (oneMinusT * x1 + t * x2)
                                    + t * (oneMinusT * x2 + t * x3)));

            int y = (int)(oneMinusT * (oneMinusT * (oneMinusT * y0 + t * y1)
                                    + t * (oneMinusT * y1 + t * y2))
                     + t * (oneMinusT * (oneMinusT * y1 + t * y2)
                                    + t * (oneMinusT * y2 + t * y3)));

            return (x, y);
        }
        public void BezierSmoother(int Radius,
            float x0, float x1, float x2, float x3,
            float y0, float y1, float y2, float y3,
            int softMaxHeight = 1000,
            int landVarianceMin = 30,
            int landVarianceMax = 40
            )
        {
            Random rnd = new Random();
            int lasty = int.MinValue, lastx = int.MinValue;
            for (float t = 0; t <= 1; t += 0.01f)
            {
                var (x, y) = CubicBezier(t, x0, x1, x2, x3, y0, y1, y2, y3);
                var (distx, disty) = (x - lastx, y - lasty);
                if (x < 0 || y < 0 || x >= width || y >= height) continue;
                if (Heights[x, y] > softMaxHeight) continue;
                CreateMound(x, y, rnd.Next(landVarianceMin, landVarianceMax));
                //make it so it doesent make a mound if it was too close to the last point
            }
        }
        public void ApplySeaLevel(int seaLevel)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (Heights[x, y] > seaLevel)
                    {
                        Values[x, y] = 0;
                    }
                    else
                    {
                        Values[x, y] = 1;
                    }
                }
            }
        }
        public void CreateMound(int posX, int posY, int Radius)
        {
            int MoundPeak = (int)Math.Pow(Radius * Radius + Radius * Radius, 0.7f);

            for (int x = posX - Radius; x < posX + Radius; x++)
            {
                for (int y = posY - Radius; y < posY + Radius; y++)
                {
                    if (x > 0 && y > 0 && x < width && y < height)
                    {
                        float num = x - posX;
                        float num2 = y - posY;
                        int AdditionalHeight = MoundPeak - (int)Math.Pow(num * num + num2 * num2, 0.7f) - 10;
                        Heights[x, y] -= AdditionalHeight;
                    }
                }
            }
        }

        public void SmoothenHeights(int Level)
        {
            int[,] newHeights = new int[width, height];
            int xChecking;
            int yChecking;
            int LowRange = -Level;
            int HighRange = Level + 1;
            int amountPerCheck = Level * 2 + 1;
            int average = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (Values[x, y] == 0)
                    {
                        Heights[x, y] = 0;
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    average = 0;
                    for (int xOffset = LowRange; xOffset < HighRange; xOffset++)
                    {
                        xChecking = x + xOffset;
                        if (xChecking > 0 && xChecking < width)
                        {
                            average += Heights[xChecking, y];
                        }
                    }
                    average /= amountPerCheck;
                    newHeights[x, y] = average;
                }
            }
            Heights = newHeights;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    average = 0;
                    for (int yOffset = LowRange; yOffset < HighRange; yOffset++)
                    {
                        yChecking = y + yOffset;
                        if (yChecking > 0 && yChecking < height)
                        {
                            average += Heights[x, yChecking];
                        }
                    }
                    average /= amountPerCheck;
                    newHeights[x, y] = average;
                }
            }
            Heights = newHeights;
        }
        public void SmoothenTerrain()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (CountLandNeighbhors(x, y) > 4)
                    {
                        Values[x, y] = 1;
                    }
                    else
                    {
                        Values[x, y] = 0;
                    }
                }
            }
        }
        public int HeightAtPosition(Vector3 Position, int MapCellSize)
        {
            int playerXIndex = (int)Position.X / MapCellSize;
            int playerYIndex = (int)Position.Z / MapCellSize;
            if (playerXIndex >= 0 && playerYIndex >= 0 && playerXIndex < width && playerYIndex < height)
            {
                return Heights[playerXIndex, playerYIndex];
            }
            return int.MinValue; //outside of map
        }
    }
}
