using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal class SystemRandomProvider : IRandomProvider
    {
        public int Next(int minInclusive, int maxExclusive) => Random.Shared.Next(minInclusive, maxExclusive);
        public double NextDouble() => Random.Shared.NextDouble();
    }
}
