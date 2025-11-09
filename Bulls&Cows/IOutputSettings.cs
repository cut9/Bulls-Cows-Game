using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal interface IOutputSettings
    {
        bool DisplayRightRepetitions { get; }
        bool DisplayWrongRepetitions { get; }
        bool DisplayNumbersOnly { get; }
    }
}