using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal class DailyRandomProvider : IRandomProvider
    {
        private static readonly DateTime PivotDate = new DateTime(2000, 1, 1);
        private static readonly int RandomMultiplier = 256;
        public static int Seed 
        {
            get
            {
                return (DateTime.Now - PivotDate).Days * RandomMultiplier;
            }
        } 
        private readonly Random _rng = new Random(Seed);
        public int Next(int minInclusive, int maxExclusive) => _rng.Next(minInclusive, maxExclusive);
        public double NextDouble() => _rng.NextDouble();
    }
}
