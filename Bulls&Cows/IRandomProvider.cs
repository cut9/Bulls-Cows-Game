using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal interface IRandomProvider
    {
        int Next(int minInclusive, int maxExclusive);
        double NextDouble();
    }
}
