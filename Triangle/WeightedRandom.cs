using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace random_generation_in_a_pixel_grid
{
    internal class WeightedRandom
    {
        
        readonly int[] _weights;
        readonly Random random;
        int MaxValue { get => _weights.Length; }
        public WeightedRandom(int[] weights, int? seed = null)
        {
            _weights = weights;

            if (seed != null)
            {
                random = new Random((int)seed);
            }
            else
            {
                random = new Random();
            }
        }
        public int Next()
        {
            int totalWeight = _weights.Sum();
            int randomNumber = random.Next(totalWeight);
            int index = 0;

            int culminatingWeight = 0;

            foreach (int weight in _weights)
            {
                culminatingWeight += weight;
                if (randomNumber < culminatingWeight)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
    }
}
