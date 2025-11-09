using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal record OutputSettings(bool DisplayRightRepetitions, bool DisplayWrongRepetitions, bool DisplayNumbersOnly) : IOutputSettings;
}