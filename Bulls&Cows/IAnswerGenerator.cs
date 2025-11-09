using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal interface IAnswerGenerator
    {
        public void SetSettings(IReadOnlySet<char> pool, IAnswerSettings settings, IRandomProvider rng);
        bool TryGenerate(out string answer);
    }
}
