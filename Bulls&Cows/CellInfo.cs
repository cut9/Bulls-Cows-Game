using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal class CellInfo
    {
        public OutputCharInfo Slot { get; private set; }
        public HashSet<OutputCharInfo> WrongGuesses { get; private set; } = new HashSet<OutputCharInfo>();
        public CellInfo() 
        {
            Slot = new OutputCharInfo();
            Slot.Char = '\0';
        }
        public bool TryAdd(OutputCharInfo userChar)
        {
            if (userChar.Type == CharType.RightPosition)
                Slot = userChar;
            if (userChar.Type == CharType.WrongPosition && !Contains(userChar))
                WrongGuesses.Add(userChar);
            if (userChar.Type == CharType.NotContained)
                return false;
            return true;
        }
        private bool Contains(OutputCharInfo userChar)
        {
            foreach (var item in WrongGuesses)
                if (item.Char == userChar.Char)
                    return true;
            return false;
        }
    }
}
