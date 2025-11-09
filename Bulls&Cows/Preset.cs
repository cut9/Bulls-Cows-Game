using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal record Preset(string Name, int NumberOfAttempts, HashSet<char> CharsPool, AnswerSettings AnswerSettings, OutputSettings OutputSettings);
}
